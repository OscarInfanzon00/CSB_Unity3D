using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
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
    public static void SaveUser(string userID, string username, string email, int userLVL, int words)
    {
        PlayerPrefs.SetString("SavedUserID", userID);
        PlayerPrefs.SetString("SavedUsername", username);
        PlayerPrefs.SetString("SavedEmail", email);
        PlayerPrefs.SetInt("lvl", userLVL);
        PlayerPrefs.SetInt("words", words);
        PlayerPrefs.Save();
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
}