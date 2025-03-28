using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;
using System;

public class CurrentStoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseUser currentUser;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    public TMP_InputField searchInputField;
    private List<Story> myStories = new List<Story>();

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        LoadMyStories();

        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(FilterStories);
        }
    }

    void LoadMyStories()
    {
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    myStories.Clear();
                    foreach (Transform child in storyListContainer)
                    {
                        Destroy(child.gameObject);
                    }

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        Dictionary<string, object> data = doc.ToDictionary();

                        // Check if the first user in the list matches the current user's ID
                        if (data.ContainsKey("users"))
                        {
                            List<object> usersList = (List<object>)data["users"];
                            if (usersList.Count > 0 && usersList[0].ToString() == currentUser.UserId)
                            {
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
                                Story newStory = new Story(
                                    data["storyID"].ToString(), 
                                    previewText, 
                                    storyTexts, 
                                    usernames, 
                                    timestamp
                                );

                                myStories.Add(newStory);
                            }
                        }
                    }

                    // Sort stories by timestamp (latest first)
                    myStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));

                    // Display stories
                    foreach (Story story in myStories)
                    {
                        CreateStoryCard(story);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load my stories: " + e);
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }

    void FilterStories(string filterText)
    {
        List<Story> filteredStories = myStories.FindAll(story =>
            story.storyTexts.Count > 0 && story.storyTexts[0].ToLower().Contains(filterText.ToLower())
        );
        DisplayStories(filteredStories);
    }

    void DisplayStories(List<Story> stories)
    {
        foreach (Transform child in storyListContainer)
        {
            Destroy(child.gameObject);
        }

        if (stories.Count != myStories.Count)
        {
            foreach (Story story in stories)
            {
                CreateStoryCard(story);
            }
        }
        else
        {
            LoadMyStories();
        }
    }

    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        cardUI.storyID = story.storyRealID;
    }

    public void RemoveStories()
    {
        int children = storyListContainer.childCount;
        Debug.Log(children + " stories to delete");

        for (int i = 0; i < children; i++)
        {
            GameObject.Destroy(storyListContainer.GetChild(i).gameObject);
            Debug.Log("Deleted a story");
        }

        LoadMyStories();
    }
}