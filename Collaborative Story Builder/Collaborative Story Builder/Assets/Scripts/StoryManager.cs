using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;

public class StoryManager : MonoBehaviour
{
    FirebaseFirestore db;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;
    private string storyID;

    public GameObject commentingPanel;

    public TMP_InputField userCommentInputField;

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




    public void AddComment()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            return;
        }

        string userID = user.UserId;
        string commentText = userCommentInputField.text.Trim();

        if (string.IsNullOrEmpty(commentText))
        {
            Debug.LogError("Comment cannot be empty.");
            return;
        }

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    List<List<string>> comments = snapshot.ContainsField("comments")
                        ? snapshot.GetValue<List<List<string>>>("comments")
                        : new List<List<string>>();

                    // Add new comment with user ID
                    comments.Add(new List<string> { userID, commentText });

                    storyRef.UpdateAsync("comments", comments).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("Comment added successfully.");
                            userCommentInputField.text = ""; // Clear input field after submission
                        }
                        else
                        {
                            Debug.LogError("Error adding comment: " + updateTask.Exception);
                        }
                    });
                }
                else
                {
                    Debug.LogError("Story does not exist.");
                }
            }
            else
            {
                Debug.LogError("Error retrieving story: " + task.Exception);
            }
        });
    }




    public void LikeStory()
{
    if (string.IsNullOrEmpty(storyID))
    {
        Debug.LogError("No story is currently selected.");
        return;
    }

    DocumentReference storyRef = db.Collection("Stories").Document(storyID);

    storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                int currentLikes = snapshot.ContainsField("likes") ? snapshot.GetValue<int>("likes") : 0;
                int newLikes = currentLikes + 1;

                storyRef.UpdateAsync("likes", newLikes).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"Story {storyID} now has {newLikes} likes!");
                    }
                    else
                    {
                        Debug.LogError($"Error updating likes: {updateTask.Exception}");
                    }
                });
            }
            else
            {
                Debug.LogError("Story not found.");
            }
        }
        else
        {
            Debug.LogError($"Error fetching story: {task.Exception}");
        }
    });
}

    
    
}
