using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class StoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    private string storyID;

    public GameObject commentingPanel;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadStories();
    }

    void LoadStories()
    {
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
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

                    List<string> users = new List<string>();
                    if (data.ContainsKey("usernames"))
                    {
                        foreach (var item in (List<object>)data["usernames"])
                        {
                            users.Add(item.ToString());
                        }
                    }

                    storyID = (string)data["storyID"];
                    Timestamp timestamp = (Timestamp)data["timestamp"];
                    string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                    Story newStory = new Story(previewText, storyTexts, users, timestamp.ToDateTime());

                    CreateStoryCard(newStory);
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }

    public void TogglePanel()
    {
        if (commentingPanel != null)
        {
            commentingPanel.SetActive(!commentingPanel.activeSelf);
        }
    }



    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        cardUI.storyID = storyID;
    }
}
