using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using TMPro;


public class UploadStory : MonoBehaviour
{
    public TMP_InputField storyInputField;
    private FirebaseFirestore db;

    public NotificationManager notificationManager;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
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

        Dictionary<string, object> storyData = new Dictionary<string, object>
    {
        { "storyID", storyID },
        { "userID", userID },
        { "storyText", storyInputField.text},
        { "timestamp", Timestamp.GetCurrentTimestamp() }
    };

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                notificationManager.Notify("Your story was succesfully uploaded!", 3f);
                Debug.Log("Story saved successfully by user: " + userID);
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

