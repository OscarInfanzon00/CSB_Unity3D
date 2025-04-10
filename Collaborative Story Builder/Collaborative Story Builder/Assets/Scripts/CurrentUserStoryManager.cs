using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using System;
using TMPro;

public class CurrentUserStoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseUser currentUser;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    public string storyID;
    public TMP_Text NoStoriesInFeed;
    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        LoadUserStories();
    }

    public void LoadUserStories()
    {
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    List<Story> userStories = new List<Story>();

                    foreach (Transform child in storyListContainer)
                    {
                        Destroy(child.gameObject);  // Clear previous entries
                    }

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        Dictionary<string, object> data = doc.ToDictionary();
                        if (!data.ContainsKey("users") || !(data["users"] is List<object> userList) || userList.Count == 0)
                            continue;  // Skip if no users field or empty list

                        if (!data.ContainsKey("ownerID") || data["ownerID"].ToString() != currentUser.UserId)
                            continue;  // Skip if current user is not the owner

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

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(data["storyID"].ToString(), previewText, storyTexts, usersnames, timestamp);
                        userStories.Add(newStory);
                    }

                    if (userStories.Count == 0)
                    {
                        NoStoriesInFeed.text = "No Stories here, Play to create one!";
                    }
                    else
                    {
                        NoStoriesInFeed.text = "";
                    }

                        // Sort stories by timestamp (latest first)
                        userStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));
                    foreach (Story story in userStories)
                    {
                        CreateStoryCard(story);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load stories: " + e);
                    NoStoriesInFeed.text = e.Message;

                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
                NoStoriesInFeed.text = "Failed to load stories, Please try again later.";
            }
        });
    }
    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        cardUI.storyID = story.storyRealID;
    }
}
