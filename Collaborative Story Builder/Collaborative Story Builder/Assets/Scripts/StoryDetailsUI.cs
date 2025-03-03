using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using Firebase.Extensions;

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

    public Transform commentsContainer;
    public GameObject commentPrefab; 

    private void Awake()
    {
        Instance = this;
        closeButton.onClick.AddListener(CloseDetails);
    }

    public void ShowStoryDetails(Story story, string storyID)
    {
        storyContent.text = string.Join("\n\n", story.storyTexts);
        storyAuthors.text = "Authors: " + string.Join(", ", story.users);
        storyDate.text = "Date: " + story.timestamp.ToString("MM/dd/yyyy HH:mm");

        this.storyID = storyID;
<<<<<<< Updated upstream
=======
        updateLikes();

        LoadComments();
    }
>>>>>>> Stashed changes


        //story likes
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("likes"))
                {
                    int likes = snapshot.GetValue<int>("likes");
                    storyLikes.text = "Likes: " + likes.ToString();
                }
                else
                {
                    storyLikes.text = "Likes: 0";  // Default if no likes field is found
                }
            }
            else
            {
                Debug.LogError("Failed to fetch likes: " + task.Exception);
                storyLikes.text = "Likes: N/A";  // Show error state in UI
            }
        });


        //addComment()
    }


<<<<<<< Updated upstream
    /**
    addComment method here
    */
=======
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
            Destroy(child.gameObject); // Clear previous comments
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

                    foreach (var comment in comments)
                    {
                        string userID = comment["userID"];
                        string text = comment["text"];
                        CreateCommentUI(userID, text);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load comments: " + task.Exception);
            }
        });
    }



    void CreateCommentUI(string userID, string text)
    {
        GameObject newComment = Instantiate(commentPrefab, commentsContainer);
        TMP_Text commentText = newComment.GetComponent<TMP_Text>();

        commentText.text = $"{userID}: {text}";
    }


    public void TogglePanel()
    {
        if (commentingPanel != null)
        {
            commentingPanel.SetActive(!commentingPanel.activeSelf);
        }
    }
>>>>>>> Stashed changes

    public void CloseDetails()
    {
        StoryViewerUI.SetActive(false);
    }
}
