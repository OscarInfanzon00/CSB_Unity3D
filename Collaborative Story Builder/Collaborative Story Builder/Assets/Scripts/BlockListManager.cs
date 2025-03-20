using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class BlockListManager : MonoBehaviour
{
    public GameObject BlockListUI;
    public Transform contentArea;
    public GameObject BlockCardPrefab;
    public TMP_Text blockedCounterText;
    public Button closeButton;
    public Button refreshButton;
    public TMP_InputField searchField;

    public GameObject blockedPopup;
    public TMP_Text popupUsernameText;
    public Button closeBlockedPopupBtn;
    public Button unblockButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    private string selectedBlockedId;

    private List<(string userId, string username)> allBlockedUsers = new();

    void Start()
    {
        closeBlockedPopupBtn.onClick.AddListener(CloseBlockedPopup);
        closeButton.onClick.AddListener(CloseBlockListPanel);
        unblockButton.onClick.AddListener(UnblockUser);
        refreshButton.onClick.AddListener(LoadBlockedList);
        searchField.onValueChanged.AddListener(FilterBlockedList);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    currentUserId = auth.CurrentUser.UserId;
                    OpenBlockedList();
                }
            }
        });
    }

    public void OpenBlockedList()
    {
        BlockListUI.SetActive(true);
        LoadBlockedList();
    }

    private void LoadBlockedList()
    {
        allBlockedUsers.Clear();

        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        db.Collection("Blocked").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (!snapshot.Exists || !snapshot.ContainsField("blockedPeople"))
                {
                    UpdateBlockedCounter(0);
                    return;
                }

                List<string> blockedList = snapshot.GetValue<List<string>>("blockedPeople");
                UpdateBlockedCounter(blockedList.Count);

                foreach (string blockedId in blockedList)
                {
                    FetchBlockedUserDetails(blockedId);
                }
            }
            else
            {
                UpdateBlockedCounter(0);
            }
        });
    }

    private void FetchBlockedUserDetails(string blockedId)
    {
        db.Collection("Users").Document(blockedId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot blockedSnapshot = task.Result;

                if (blockedSnapshot.Exists)
                {
                    string username = blockedSnapshot.ContainsField("username") 
                        ? blockedSnapshot.GetValue<string>("username")
                        : "Unknown User";
                    username = FormatUsername(username);
                    allBlockedUsers.Add((blockedId, username));
                    FilterBlockedList(searchField.text);
                }
            }
        });
    }

    private void FilterBlockedList(string searchText)
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        string searchLower = searchText.ToLower();

        int count = 0;
        foreach (var blockedUser in allBlockedUsers)
        {
            if (blockedUser.username.ToLower().Contains(searchLower))
            {
                CreateBlockedCard(blockedUser.userId, blockedUser.username);
                count++;
            }
        }

        Debug.Log($"Filtered Blocked Users: {count} matching '{searchText}'");
    }

    private void CreateBlockedCard(string userId, string username)
    {
        if (BlockCardPrefab == null)
        {
            return;
        }

        GameObject newBlockCard = Instantiate(BlockCardPrefab, contentArea);
        if (newBlockCard == null)
        {
            return;
        }

        BlockedCardController blockController = newBlockCard.GetComponent<BlockedCardController>();
        if (blockController != null)
        {
            blockController.Setup(username);
        }
        

        Button blockedCardButton = newBlockCard.GetComponent<Button>();
        if (blockedCardButton != null)
        {
            blockedCardButton.onClick.AddListener(() =>
            {
                OpenBlockedPopup(userId, username);
            });
        }
    }

    private void UpdateBlockedCounter(int count)
    {
        if (blockedCounterText != null)
        {
            blockedCounterText.text = $"Blocked Users: {count}";
        }
    }

    public void OpenBlockedPopup(string userId, string username)
    {
        selectedBlockedId = userId;
        username = FormatUsername(username);
        if (popupUsernameText != null) popupUsernameText.text = username;

        blockedPopup.SetActive(true);
    }

    public void CloseBlockedPopup()
    {
        blockedPopup.SetActive(false);
    }

    private void UnblockUser()
    {
        if (string.IsNullOrEmpty(selectedBlockedId))
        {
            return;
        }

        db.Collection("Blocked").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists && snapshot.ContainsField("blockedPeople"))
                {
                    List<string> blockedList = snapshot.GetValue<List<string>>("blockedPeople");

                    if (blockedList.Contains(selectedBlockedId))
                    {
                        blockedList.Remove(selectedBlockedId);

                        db.Collection("Blocked").Document(currentUserId).UpdateAsync("blockedPeople", blockedList).ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                LoadBlockedList();
                                CloseBlockedPopup();
                            }
                        });
                    }
                }
            }
        });
    }

    public void CloseBlockListPanel()
    {
        BlockListUI.SetActive(false);
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

