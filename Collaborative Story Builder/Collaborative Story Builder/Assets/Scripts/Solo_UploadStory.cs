using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using TMPro;


public class UploadStory : MonoBehaviour
{
    private UserData userData;
    public TMP_InputField storyInputField;
    private FirebaseFirestore db;

    public NotificationManager notificationManager;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userData = User.GetUser();
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
        string timestamp = Timestamp.GetCurrentTimestamp().ToString();

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
        { "timestamp", timestamp },
        { "usernames", usersUsernames },
        { "users", usersID }
    };

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                notificationManager.Notify("Your story was successfully uploaded!", 3f);
                Debug.Log("Story saved successfully with ID: " + storyID);
            }
            else
            {
                Debug.LogError("Error saving story: " + task.Exception);
            }
        });
    }

    void Update()
    {

    }
}

