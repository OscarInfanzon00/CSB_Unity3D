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

    public Transform commentsContainer;
    public GameObject commentPrefab;
    public TMP_Text loveText, funnyText, sadText, angryText;

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

        LoadComments();
        LoadReactions();
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

    public void LoadComments()
    {
        foreach (Transform child in commentsContainer)
        {
            Destroy(child.gameObject);
        }

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);
        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("comments"))
                {
                    List<Dictionary<string, string>> comments = snapshot.GetValue<List<Dictionary<string, string>>>("comments");

                    Dictionary<string, string> userCache = new Dictionary<string, string>();

                    foreach (var comment in comments)
                    {
                        string userID = comment["userID"];
                        string text = comment["text"];

                        if (userCache.ContainsKey(userID))
                        {
                            CreateCommentUI(userCache[userID], text);
                        }
                        else
                        {
                            FetchUsername(userID, text, userCache);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load comments: " + task.Exception);
            }
        });
    }

    void FetchUsername(string userID, string text, Dictionary<string, string> userCache)
    {
        DocumentReference userRef = db.Collection("Users").Document(userID);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot userSnapshot = task.Result;
                if (userSnapshot.Exists && userSnapshot.ContainsField("username"))
                {
                    string username = userSnapshot.GetValue<string>("username");
                    userCache[userID] = username;
                    CreateCommentUI(username, text);
                }
                else
                {
                    Debug.LogWarning($"Username not found for userID: {userID}");
                    CreateCommentUI("Unknown User", text);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch username: " + task.Exception);
                CreateCommentUI("Unknown User", text);
            }
        });
    }

    void CreateCommentUI(string username, string text)
    {
        GameObject newComment = Instantiate(commentPrefab, commentsContainer);
        TMP_Text commentText = newComment.GetComponent<TMP_Text>();
        commentText.text = $"{username} said: {text}";
    }

    public void reactLove()
    {
        SaveReaction(0);
    }
    public void reactFunny()
    {
        SaveReaction(1);
    }
    public void reactSad()
    {
        SaveReaction(2);
    }
    public void reactAngry()
    {
        SaveReaction(3);
    }

    public void SaveReaction(int reactionType)
    {
        string[] reactionFields = { "love", "funny", "sad", "angry" };

        if (reactionType < 0 || reactionType >= reactionFields.Length)
        {
            Debug.LogError("Invalid reaction type.");
            return;
        }

        string selectedReaction = reactionFields[reactionType];
        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.UpdateAsync(selectedReaction, FieldValue.Increment(1))
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log($"Reaction '{selectedReaction}' incremented successfully.");
                    LoadReactions();
                }
                else
                {
                    Debug.LogError("Failed to save reaction: " + task.Exception);
                }
            });
    }

    public void LoadReactions()
    {
        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    int love = snapshot.ContainsField("love") ? snapshot.GetValue<int>("love") : 0;
                    int funny = snapshot.ContainsField("funny") ? snapshot.GetValue<int>("funny") : 0;
                    int sad = snapshot.ContainsField("sad") ? snapshot.GetValue<int>("sad") : 0;
                    int angry = snapshot.ContainsField("angry") ? snapshot.GetValue<int>("angry") : 0;

                    UpdateUI(love, funny, sad, angry);
                }
            }
            else
            {
                Debug.LogError("Failed to load reactions: " + task.Exception);
            }
        });
    }

    private void UpdateUI(int love, int funny, int sad, int angry)
    {
        loveText.text = love.ToString();
        funnyText.text = funny.ToString();
        sadText.text = sad.ToString();
        angryText.text = angry.ToString();
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
