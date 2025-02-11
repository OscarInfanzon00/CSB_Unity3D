using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;

public class BlockManager : MonoBehaviour
{
    public Button blockButton;
    public InputField targetUserField; // Input field to enter the user ID

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        blockButton.onClick.AddListener(BlockSelectedUser);
    }

    void BlockSelectedUser()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogError("No user is logged in.");
            return;
        }

        string currentUserId = auth.CurrentUser.UserId;
        string targetUser = targetUserField.text.Trim(); // Get the input field value

        if (string.IsNullOrEmpty(targetUser))
        {
            Debug.LogError("Please enter a valid user ID.");
            return;
        }

        DocumentReference userDoc = db.Collection("BlockedUsers").Document(currentUserId);

        // Create a dictionary for the new blocked user
        Dictionary<string, object> blockData = new Dictionary<string, object>
        {
            { "blocked", new List<string> { targetUser } }
        };

        // Use SetOptions.Merge() to add the user without overwriting
        userDoc.SetAsync(blockData, SetOptions.Merge()).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log($"User {targetUser} blocked successfully.");
            else
                Debug.LogError("Failed to block user: " + task.Exception);
        });
    }
}
