﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;


public class FriendsListManager : MonoBehaviour
{
    public GameObject FriendListUI;
    public Transform contentArea;
    public GameObject FriendCardPrefab;
    public TMP_Text friendsCounterText;
    public Button closeButton;
    public Button refreshButton;
    public TMP_InputField searchField; // 🆕 Search input field

    public GameObject friendsPopup;
    public TMP_Text popupUsernameText;
    public TMP_Text popupLevelText;
    public TMP_Text popupWordsText;
    public Button closeFriendPopupBtn;
    public Button removeFriendButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    private string selectedFriendId;

    private List<(string friendId, string username, int level, int words)> allFriends = new(); // 🆕 Store all friends

    void Start()
    {
        closeFriendPopupBtn.onClick.AddListener(CloseFriendsPopup);
        closeButton.onClick.AddListener(CloseFriendsPanel);
        removeFriendButton.onClick.AddListener(RemoveFriend);
        refreshButton.onClick.AddListener(LoadFriendsList);
        searchField.onValueChanged.AddListener(FilterFriendsList); // 🆕 Add search listener

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    currentUserId = auth.CurrentUser.UserId;
                    OpenFriendsList();
                }
            }
        });
    }

    public void OpenFriendsList()
    {
        FriendListUI.SetActive(true);
        LoadFriendsList();
    }

    private void LoadFriendsList()
    {
        allFriends.Clear(); // 🆕 Clear stored friends

        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        db.Collection("Friends").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (!snapshot.Exists || !snapshot.ContainsField("friends"))
                {
                    UpdateFriendsCounter(0);
                    return;
                }

                List<string> friendsList = snapshot.GetValue<List<string>>("friends");
                UpdateFriendsCounter(friendsList.Count);

                foreach (string friendId in friendsList)
                {
                    FetchFriendDetails(friendId);
                }
            }
            else
            {
                UpdateFriendsCounter(0);
            }
        });
    }

    private void FetchFriendDetails(string friendId)
    {
        db.Collection("Users").Document(friendId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot friendSnapshot = task.Result;

                if (friendSnapshot.Exists)
                {
                    string username = friendSnapshot.ContainsField("username") ? friendSnapshot.GetValue<string>("username") : "Unknown User";
                    int level = friendSnapshot.ContainsField("userLevel") ? friendSnapshot.GetValue<int>("userLevel") : 0;
                    int words = friendSnapshot.ContainsField("words") ? friendSnapshot.GetValue<int>("words") : 0;

                    username = FormatUsername(username);

                    allFriends.Add((friendId, username, level, words)); // 🆕 Store friend in list

                    FilterFriendsList(searchField.text); // 🆕 Refresh displayed friends
                }
            }
        });
    }

    private void FilterFriendsList(string searchText)
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        string searchLower = searchText.ToLower();

        int count = 0;
        foreach (var friend in allFriends)
        {
            if (friend.username.ToLower().Contains(searchLower)) // Case-insensitive search
            {
                CreateFriendCard(friend.friendId, friend.username, friend.level, friend.words);
                count++;
            }
        }

        Debug.Log($"Filtered Friends: {count} matching '{searchText}'");
    }

    private void CreateFriendCard(string friendId, string username, int level, int words)
    {
        if (FriendCardPrefab == null) return;

        GameObject newFriendCard = Instantiate(FriendCardPrefab, contentArea);
        if (newFriendCard == null) return;

        FriendCardController friendController = newFriendCard.GetComponent<FriendCardController>();
        if (friendController != null)
        {
            // 🆕 Pass all details correctly
            friendController.Setup(username, level,friendId);
        }
    }


    private void UpdateFriendsCounter(int count)
    {
        if (friendsCounterText != null)
        {
            friendsCounterText.text = $"Friends: {count}";
        }
    }

    public void OpenFriendsPopup(string friendId, string username, int level, int words)
    {
        selectedFriendId = friendId;
        username = FormatUsername(username);

        if (popupUsernameText != null) popupUsernameText.text = username;
        if (popupLevelText != null) popupLevelText.text = "Level: " + level;
        if (popupWordsText != null) popupWordsText.text = "Words: " + words;

        friendsPopup.SetActive(true);
    }

    public void CloseFriendsPopup()
    {
        friendsPopup.SetActive(false);
    }

    private void RemoveFriend()
    {
        if (string.IsNullOrEmpty(selectedFriendId))
        {
            return;
        }

        db.Collection("Friends").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("friends"))
                {
                    List<string> friendsList = snapshot.GetValue<List<string>>("friends");

                    if (friendsList.Contains(selectedFriendId))
                    {
                        friendsList.Remove(selectedFriendId);

                        db.Collection("Friends").Document(currentUserId).UpdateAsync("friends", friendsList).ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                LoadFriendsList();
                                CloseFriendsPopup();
                            }
                        });
                    }
                }
            }
        });
    }

    public void CloseFriendsPanel()
    {
        FriendListUI.SetActive(false);
    }

    private string FormatUsername(string username)
    {
        if (username.Contains("@"))
        {
            return username.Split('@')[0];
        }
        return username;
    }
}
