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


    /**
    addComment method here
    */

    public void CloseDetails()
    {
        StoryViewerUI.SetActive(false);
    }
}
