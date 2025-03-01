using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;

public class StoryDetailsUI : MonoBehaviour
{
    public Button closeButton;
    public static StoryDetailsUI Instance;
    public TMP_Text storyContent;
    public TMP_Text storyAuthors;
    public TMP_Text storyDate;
    public GameObject StoryViewerUI;
    public string storyID;
    public TMP_Text storyLikes;
    FirebaseFirestore db;
    public GameObject commentingPanel;
    public TMP_InputField userCommentInputField;

    private void Awake()
    {
        Instance = this;
        closeButton.onClick.AddListener(CloseDetails);
        db = FirebaseFirestore.DefaultInstance;
    }

    public void ShowStoryDetails(Story story, string storyID)
    {
        storyContent.text = string.Join("\n\n", story.storyTexts);
        storyAuthors.text = "Authors: " + string.Join(", ", story.users);
        storyDate.text = "Date: " + story.timestamp.ToString("MM/dd/yyyy HH:mm");

        this.storyID = storyID;
        updateLikes();
    }

    public void updateLikes()
    {
        DocumentReference storyRef = db.Collection("Stories").Document(storyID);
        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("likes"))
                {
                    int likes = snapshot.GetValue<int>("likes");
                    storyLikes.text = likes.ToString();
                }
                else
                {
                    storyLikes.text = "0";  // Default if no likes field is found
                }
            }
            else
            {
                Debug.LogError("Failed to fetch likes: " + task.Exception);
                storyLikes.text = "N/A";  // Show error state in UI
            }
        });
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
                    List<Dictionary<string, string>> comments = snapshot.ContainsField("comments")
                        ? snapshot.GetValue<List<Dictionary<string, string>>>("comments")
                        : new List<Dictionary<string, string>>();

                    // Add new comment as an object (dictionary)
                    Dictionary<string, string> newComment = new Dictionary<string, string>
                    {
                    { "userID", userID },
                    { "text", commentText }
                    };
                    comments.Add(newComment);

                    storyRef.UpdateAsync("comments", comments).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("Comment added successfully.");
                            userCommentInputField.text = ""; // Clear input field after submission
                            TogglePanel();
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
                            updateLikes();
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

    public void TogglePanel()
    {
        if (commentingPanel != null)
        {
            commentingPanel.SetActive(!commentingPanel.activeSelf);
        }
    }

    public void CloseDetails()
    {
        StoryViewerUI.SetActive(false);
    }
}
