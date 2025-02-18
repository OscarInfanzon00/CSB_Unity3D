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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        
    }

    
    
    public void SaveStory(string storyText)
{
    // Get the current user's ID from Firebase Auth
    FirebaseAuth auth = FirebaseAuth.DefaultInstance;
    FirebaseUser user = auth.CurrentUser;

    if (user == null)
    {
        Debug.LogError("No user is currently signed in.");
        return;
    }

    string userID = user.UserId;  // Firebase UID of the logged-in user
    string storyID = Guid.NewGuid().ToString(); // Unique Story ID

<<<<<<< Updated upstream
    // Create a dictionary to store the story data
    Dictionary<string, object> storyData = new Dictionary<string, object>
    {
        { "storyID", storyID },
        { "userID", userID }, // Logged-in user's ID
        { "storyText", storyText }, // The story text they wrote
        { "timestamp", Timestamp.GetCurrentTimestamp() } // Optional: Adds a timestamp
    };
=======
        string userID = user.UserId;
        string storyID = Guid.NewGuid().ToString();
        Timestamp timestamp = Timestamp.GetCurrentTimestamp(); 

        List<string> storyTexts = new List<string>
        {
            storyInputField.text
        };

        List<string> usersID = new List<string> { userID };
        List<string> usersUsernames = new List<string> { userData.Username };

        List<List<string>> comments = new List<List<string>>();

        Dictionary<string, object> storyData = new Dictionary<string, object>
        {
            { "storyID", storyID },
            { "storyTexts", storyTexts },
            { "timestamp", timestamp },  
            { "usernames", usersUsernames },
            { "users", usersID },
            { "comments", comments } 
        };
>>>>>>> Stashed changes

    // Reference to the "Stories" collection in Firestore
    DocumentReference storyRef = db.Collection("Stories").Document(storyID);

    // Save data to Firestore
    storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            Debug.Log("Story saved successfully by user: " + userID);
        }
        else
        {
            Debug.LogError("Error saving story: " + task.Exception);
        }
    });
}

public void StoryUploader()
    {
        string storyText = storyInputField.text; // Get user input text
        SaveStory(storyText);
    }

/*
    public void TestSaveStory()
{
    // Sample test data
    List<string> testUsers = new List<string> { "Chris", "Marco", "Josh" };
    List<string> testStoryTexts = new List<string>
    {
        "This is the first string of the story.",
        "Following the first string is this part of the story.",
        "Closing the story with this string."
    };

    // Call the SaveStory method with test data
    SaveStory(testUsers, testStoryTexts);

    // Log for debugging
    Debug.Log("TestSaveStory: Saving test story...");

    }
*/

    // Update is called once per frame
    void Update()
    {
        
    }
}

