using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Solo_UploadStory : MonoBehaviour
{
    private UserData userData;
    public TMP_InputField storyInputField;
    private FirebaseFirestore db;
    public NotificationManager notificationManager;
    public Button closeButton;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userData = User.GetUser();
        closeButton.onClick.AddListener(closeScene);
    }

    public void SaveStory()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            return;
        }

        string userID = user.UserId;
        string storyID = Guid.NewGuid().ToString();

        string storyText = storyInputField.text;
        int wordCount = CountWords(storyText);
        List<string> storyTexts = new List<string>
    {
        storyInputField.text
    };

        List<string> usersID = new List<string> { userID };
        List<string> usersUsernames = new List<string> { userData.Username };

        Dictionary<string, object> storyData = new Dictionary<string, object>
    {
        { "storyID", storyID },
        { "storyTexts", storyTexts },
        { "timestamp", Firebase.Firestore.Timestamp.GetCurrentTimestamp() },
        { "usernames", usersUsernames },
        { "users", usersID },
        { "wordCount", wordCount }
    };

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                notificationManager.Notify($"Your story was successfully uploaded!\nWord count: {wordCount}", 3f);
                Debug.Log("Story saved successfully with ID: " + storyID);
                LevelSystem.AddXP(50);
                AddWordsToUser(user.UserId, wordCount);
            }
            else
            {
                Debug.LogError("Error saving story: " + task.Exception);
            }
        });
    }
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public void AddWordsToUser(string userId, int newWords)
    {
        DocumentReference userRef = db.Collection("Users").Document(userId);

        userRef.UpdateAsync("words", FieldValue.Increment(newWords)).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Successfully added {newWords} words to user {userId}.");
            }
            else
            {
                Debug.LogError("Failed to update words: " + task.Exception);
            }
        });
    }

    void Update()
    {

    }

    public void clearText(){
        storyInputField.text = " ";
    }

    public void closeScene(){
        SceneManager.LoadScene("Main_Menu");
    }
}

