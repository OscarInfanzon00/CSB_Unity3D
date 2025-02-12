using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase;
using Firebase.Extensions;

public class UserData
{
    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!HOW TO USE!!!!!!!!!!!!!!!!!!!!!!!!!
    //private UserData user;
    //user = User.GetUser();
    //
    //Examples:
    //email.text = user.Email;
    //User.SaveUser(userId, email, email, 0, 0);
    public string UserID { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public int UserLevel { get; set; }
    public int Words { get; set; }

    public UserData(string userID, string username, string email, int userLevel, int words)
    {
        UserID = userID;
        Username = username;
        Email = email;
        UserLevel = userLevel;
        Words = words;
    }
}

public static class User
{
    private static FirebaseFirestore firestore;

    static User()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            firestore = FirebaseFirestore.GetInstance(app);
        });
    }
    
    public static void SaveUser(string userID, string username, string email, int userLVL, int words)
    {
        PlayerPrefs.SetString("SavedUserID", userID);
        PlayerPrefs.SetString("SavedUsername", username);
        PlayerPrefs.SetString("SavedEmail", email);
        PlayerPrefs.SetInt("lvl", userLVL);
        PlayerPrefs.SetInt("words", words);
        PlayerPrefs.Save();

        UserData user = new UserData(userID, username, email, userLVL, words);
        SaveUserToFirestore(user);
    }

    public static UserData GetUser()
    {
        string userID = PlayerPrefs.GetString("SavedUserID", "defaultID");
        string username = PlayerPrefs.GetString("SavedUsername", "defaultUser");
        string email = PlayerPrefs.GetString("SavedEmail", "defaultEmail");
        int userLVL = PlayerPrefs.GetInt("lvl", 0);
        int words = PlayerPrefs.GetInt("words", 0);

        return new UserData(userID, username, email, userLVL, words);
    }

    public static void SaveUserToFirestore(UserData user)
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore not initialized.");
            return;
        }

        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "userID", user.UserID },
            { "username", user.Username },
            { "email", user.Email },
            { "userLevel", user.UserLevel },
            { "words", user.Words }
        };

        DocumentReference userDocRef = firestore.Collection("Users").Document(user.UserID);
        
        userDocRef.SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("User data saved to Firestore.");
            }
            else
            {
                Debug.Log("Error saving user data: " + task.Exception);
            }
        });
    }

    public static void UpdateUserInfo(string userID, string username = null, string email = null, int? userLevel = null, int? words = null)
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore not initialized.");
            return;
        }

        DocumentReference userDocRef = firestore.Collection("Users").Document(userID);

        Dictionary<string, object> updates = new Dictionary<string, object>();

        if (username != null) updates["username"] = username;
        if (email != null) updates["email"] = email;
        if (userLevel.HasValue) updates["userLevel"] = userLevel.Value;
        if (words.HasValue) updates["words"] = words.Value;

        userDocRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("User info updated in Firestore.");
            }
            else
            {
                Debug.Log("Error updating user info: " + task.Exception);
            }
        });
    }

    public static void GetUserInfo(string userID, Action<UserData> callback)
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore not initialized.");
            return;
        }

        DocumentReference userDocRef = firestore.Collection("Users").Document(userID);

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    Dictionary<string, object> userData = snapshot.ToDictionary();

                    string userID = snapshot.Id;
                    string username = userData.ContainsKey("username") ? userData["username"].ToString() : "defaultUser";
                    string email = userData.ContainsKey("email") ? userData["email"].ToString() : "defaultEmail";
                    int userLevel = userData.ContainsKey("userLevel") ? Convert.ToInt32(userData["userLevel"]) : 0;
                    int words = userData.ContainsKey("words") ? Convert.ToInt32(userData["words"]) : 0;
                    callback(new UserData(userID, username, email, userLevel, words));
                }
                else
                {
                    Debug.Log("User not found.");
                }
            }
            else
            {
                Debug.Log("Error fetching user info: " + task.Exception);
            }
        });
    }
}