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

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadStories();
    }

    void LoadStories()
    {
         Debug.Log("1");
        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("2");
            if (task.IsCompletedSuccessfully)
            {
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                     Debug.Log("3");
                    Dictionary<string, object> data = doc.ToDictionary();

                    List<string> storyTexts = new List<string>();
                    if (data.ContainsKey("storyTexts"))
                    {
                         Debug.Log("5");
                        foreach (var item in (List<object>)data["storyTexts"])
                        {
                             Debug.Log("6");
                            storyTexts.Add(item.ToString());
                        }
                    }

 Debug.Log("7");
                    List<string> users = new List<string>();
                    if (data.ContainsKey("usernames"))
                    {
                         Debug.Log("8");
                        foreach (var item in (List<object>)data["usernames"])
                        {
                             Debug.Log("9");
                            users.Add(item.ToString());
                        }
                        Debug.Log("10");
                    }
Debug.Log("11");
                    Timestamp timestamp = (Timestamp)data["timestamp"];
Debug.Log("12");
                    string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                    Debug.Log("13");
 Debug.Log(previewText);
  Debug.Log(storyTexts);
  Debug.Log(users);
   Debug.Log(timestamp.ToDateTime());
                    Story newStory = new Story(previewText, storyTexts, users, timestamp.ToDateTime());

 Debug.Log("10");
                    CreateStoryCard(newStory);
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }



    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
    }
}
