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
    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        LoadCurrentUserStories();
    }
    void LoadCurrentUserStories()
    {
        db.Collection("Stories").WhereArrayContains("users", currentUser.UserId)
        .GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            { 
                foreach (Transform child in storyListContainer)
                {
                    Destroy(child.gameObject);
                }
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    Dictionary<string, object> data = doc .ToDictionary();
                    List<string> storyTexts = data.ContainsKey("storyTexts")
                        ? ((List<object>)data["storyTexts"]).ConvertAll(item => item.ToString())
                        : new List<string>();
                    string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                    string storyID = data.ContainsKey("storyID") ? (string)data["storyID"] : "";

                    Story newStory = new Story(storyID, previewText, storyTexts, null, System.DateTime.MinValue);
                    CreateStoryCard(newStory);
                }
            }
            else
            {
                Debug.LogError("Failed to load user stories: " + task.Exception);
            }
        });
    }
    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
    }
}