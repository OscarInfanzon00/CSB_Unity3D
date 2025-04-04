using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseUser currentUser;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    public TMP_InputField searchInputField;
    public string storyID;
    public Button refreshButton;
    private HashSet<string> friendsSet = new HashSet<string>();
    bool allTheStoriesShowingUp = true;
    private List<Story> allStories = new List<Story>();
    public TMP_Text Message;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        refreshButton.onClick.AddListener(RefreshStories);
        LoadFriends();
        LoadStories();

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(FilterStories);
        }
    }

    void LoadFriends()
    {
        db.Collection("Friends").Document(User.GetUser().UserID).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot doc = task.Result;
                if (doc.Exists)
                {
                    Dictionary<string, object> userData = doc.ToDictionary();
                    if (userData.ContainsKey("friends"))
                    {
                        List<object> friendsList = (List<object>)userData["friends"];
                        foreach (var friend in friendsList)
                        {
                            friendsSet.Add(friend.ToString());
                        }
                    }
                }
                else
                {
                    Debug.LogError("User document does not exist");
                }
            }
            else
            {
                Debug.LogError("Failed to load friends: " + task.Exception);
            }
        });
    }
    public void ClearStoryContainer()
    {
        // Clear all story cards from the container
        foreach (Transform child in storyListContainer)
        {
            Destroy(child.gameObject);
        }
    }
    void LoadStories()
    {
        ClearStoryContainer();
        allStories.Clear();
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    allStories.Clear();
                    List<Story> friendStories = new List<Story>();
                    List<Story> otherStories = new List<Story>();
                    foreach (Transform child in storyListContainer)
                    {
                        Destroy(child.gameObject);
                    }

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        Dictionary<string, object> data = doc.ToDictionary();
                        List<string> storyTexts = new List<string>();

                        if (data.ContainsKey("storyTexts"))
                        {
                            foreach (var item in (List<object>)data["storyTexts"])
                            {
                                storyTexts.Add(item.ToString());
                            }
                        }

                        List<string> usersnames = new List<string>();
                        if (data.ContainsKey("usernames"))
                        {
                            foreach (var item in (List<object>)data["usernames"])
                            {
                                usersnames.Add(item.ToString());
                            }
                        }

                        List<string> users = new List<string>();
                        if (data.ContainsKey("users"))
                        {
                            foreach (var item in (List<object>)data["users"])
                            {
                                users.Add(item.ToString());
                            }
                        }

                        storyID = data.ContainsKey("storyID") ? (string)data["storyID"] : "";

                        DateTime timestamp = DateTime.MinValue;
                        if (data.ContainsKey("timestamp"))
                        {
                            object timestampObj = data["timestamp"];
                            if (timestampObj is Firebase.Firestore.Timestamp firestoreTimestamp)
                            {
                                timestamp = firestoreTimestamp.ToDateTime();
                            }
                            else
                            {
                                Debug.LogError($"Invalid timestamp format: {timestampObj.GetType()}");
                            }
                        }

                        // Check if any user in the story is a friend
                        bool isFriend = false;
                        foreach (var user in users)
                        {
                            if (friendsSet.Contains(user)) // If user is a friend
                            {
                                isFriend = true;
                                break; // Stop checking once we find a match
                            }
                        }

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(data["storyID"].ToString(), previewText, storyTexts, usersnames, timestamp);
                        if (isFriend)
                        {
                            friendStories.Add(newStory); // Add to friend stories
                        }
                        else
                        {
                            otherStories.Add(newStory); // Add to other stories
                        }
                        allStories.Add(newStory);
                    }

                    if (friendStories.Count == 0 && otherStories.Count == 0)
                    {
                        Message.text = "No Stories available right now!";
                    }
                    else
                    {
                        Message.text = "";
                    }


                    // Sort stories by timestamp (latest first)
                    friendStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));
                    otherStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));




                    // Display friend stories first, followed by other stories
                    foreach (Story story in friendStories)
                    {
                        CreateStoryCard(story, true);

                    }

                    foreach (Story story in otherStories)
                    {
                        CreateStoryCard(story, false);
                    }


                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load stories: " + e);
                    Message.text = e.Message;
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
                Message.text = "Failed to load stories: " + task.Exception;
            }
        });
    }

    public void loadStoryByID()
    {
        ClearStoryContainer();
        string[] listIDs = getBookmarks();
        Debug.Log($"Bookmarks to load: {string.Join(", ", listIDs)}");

        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    List<Story> matchedStories = new List<Story>();

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {

                        Dictionary<string, object> data = doc.ToDictionary();

                        if (!data.ContainsKey("storyID"))
                        {
                            Debug.LogWarning("No storyID found in document");
                            continue; // Skip if no storyID
                        }

                        storyID = (string)data["storyID"];
                        Debug.Log($"Found storyID: {storyID}");

                        // Only continue if the storyID is in the listIDs
                        if (!Array.Exists(listIDs, id => id.Trim() == storyID.Trim()))
                        {
                            Debug.Log($"Skipping story with ID: {storyID} (not in bookmarks)");
                            continue;
                        }

                        List<string> storyTexts = new List<string>();
                        if (data.ContainsKey("storyTexts"))
                        {
                            foreach (var item in (List<object>)data["storyTexts"])
                            {
                                storyTexts.Add(item.ToString());
                            }
                        }

                        List<string> usernames = new List<string>();
                        if (data.ContainsKey("usernames"))
                        {
                            foreach (var item in (List<object>)data["usernames"])
                            {
                                usernames.Add(item.ToString());
                            }
                        }

                        DateTime timestamp = DateTime.MinValue;
                        if (data.ContainsKey("timestamp"))
                        {
                            object timestampObj = data["timestamp"];
                            if (timestampObj is Firebase.Firestore.Timestamp firestoreTimestamp)
                            {
                                timestamp = firestoreTimestamp.ToDateTime();
                            }
                            else
                            {
                                Debug.LogError($"Invalid timestamp format: {timestampObj.GetType()}");
                            }
                        }

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(data["storyID"].ToString(), previewText, storyTexts, usernames, timestamp);

                        matchedStories.Add(newStory);
                    }

                    // Sort stories by timestamp (latest first)
                    matchedStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));

                    // Display matched stories
                    foreach (Story story in matchedStories)
                    {
                        CreateStoryCard(story, false);
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load stories: " + e);
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }

    public string[] getBookmarks()
    {
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");
        Debug.Log($"Loaded Bookmarks: {bookMarkList}");
        return bookMarkList.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
    }
    public void removeStories()
    {
        ClearStoryContainer();
        StartCoroutine(ReloadStoryList());
    }

    private IEnumerator ReloadStoryList()
    {
        yield return new WaitForSeconds(0.1f); // Wait for UI update
        if (allTheStoriesShowingUp)
        {
            loadStoryByID();
            allTheStoriesShowingUp = false;
        }
        else
        {
            LoadStories();
            allTheStoriesShowingUp = true;
        }
    }
    public void RefreshStories()
    {
        // Show loading indicator if you have one
        
        // Clear existing stories
        ClearStoryContainer();
        
        // Reset story lists
        allStories.Clear();
        
        // Reload friends (in case friend list has changed)
        LoadFriends();
        
        // Reload stories
        LoadStories();
    }
    void FilterStories(string filterText)
    {
        List<Story> filteredStories = allStories.FindAll(story =>
            story.storyTexts.Count > 0 && story.storyTexts[0].ToLower().Contains(filterText.ToLower())
        );

        if (filteredStories.Count > 0)
        {
            Message.text = "";
        }
        else
        {
            Message.text = "No Stories available with that Name!";
        }
            DisplayStories(filteredStories);
    }

    void DisplayStories(List<Story> stories)
    {
        ClearStoryContainer();
        foreach (Transform child in storyListContainer)
        {
            Destroy(child.gameObject);
        }

        if (stories.Count != allStories.Count)
        {
            foreach (Story story in stories)
            {
                CreateStoryCard(story, false);
            }
        }else{
            LoadStories();
        }
    }
    public void RemoveStoryByID(string storyIdToRemove)
    {
        // Remove from the allStories list
        allStories.RemoveAll(story => story.storyRealID == storyIdToRemove);

        // Find and remove from UI if present
        foreach (Transform child in storyListContainer)
        {
            StoryCardUI cardUI = child.GetComponent<StoryCardUI>();
            if (cardUI != null && cardUI.storyID == storyIdToRemove)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }
    void CreateStoryCard(Story story, bool areFriends)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        cardUI.storyID = story.storyRealID;
        if (areFriends)
            cardUI.activateFriends();
    }
}