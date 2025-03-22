using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;

public class StoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseUser currentUser;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    public TMP_InputField searchInputField;
    private string storyID;
    private HashSet<string> friendsSet = new HashSet<string>();
    bool allTheStoriesShowingUp = true;
    private List<Story> allStories = new List<Story>();

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
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

    void LoadStories()
    {
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    allStories.Clear();
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

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(previewText, storyTexts, usersnames, timestamp);
                        allStories.Add(newStory);
                    }

                    DisplayStories(allStories);
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

    public void removeStories()
{
    // Part to delete all stories
    int children = storyListContainer.childCount;
    Debug.Log(children + " stories to delete");

    for (int i = 0; i < children; i++)
    {
        GameObject.Destroy(storyListContainer.GetChild(i).gameObject);
        Debug.Log("Deleted a story");
    }

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



//For Completition. This is teh code to finish / /For Completition. This is teh code to finish
    public void loadStoryByID()
{
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

                    string storyID = (string)data["storyID"];
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
                    Story newStory = new Story(previewText, storyTexts, usernames, timestamp);

                    matchedStories.Add(newStory);
                }

                // Sort stories by timestamp (latest first)
                matchedStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));

                // Display matched stories
                foreach (Story story in matchedStories)
                {
                    CreateStoryCard(story, true);
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


    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        if (areFriends)
            cardUI.activateFriends();
    
        // Pass the specific storyID from the Story object
        cardUI.storyID = story.storyID;

    }

}