using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;

public class StoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseUser currentUser;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    private string storyID;
    private HashSet<string> friendsSet = new HashSet<string>();

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        LoadFriends();
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
                    LoadStories(); 
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
                    List<Story> friendStories = new List<Story>(); 
                    List<Story> otherStories = new List<Story>(); 

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

                        storyID = (string)data["storyID"];
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
                        Story newStory = new Story(previewText, storyTexts, usersnames, timestamp);

                        if (isFriend)
                        {
                            friendStories.Add(newStory); // Add to friend stories
                        }
                        else
                        {
                            otherStories.Add(newStory); // Add to other stories
                        }
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
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }

    // Create and display story cards
    void CreateStoryCard(Story story, bool areFriends)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        if(areFriends)
            cardUI.activateFriends();
        cardUI.storyID = storyID;
    }
}