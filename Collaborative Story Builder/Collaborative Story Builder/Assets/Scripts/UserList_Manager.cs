using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class UserList_Manager : MonoBehaviour
{
    public GameObject UserListUI;
    public Transform contentArea;
    public GameObject UserElementPrefab;
    public Button openUserListButton, closeButton;
    public TMP_Text totalUsersText;
    public TMP_InputField searchField; 
    public BlockManager blockManager;
    public ReportManager reportManager;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    private List<(string userID, string username)> allUsers = new List<(string, string)>();
    public NotificationManager notification;

    void Start()
    {
        openUserListButton.onClick.AddListener(OpenUserList);
        closeButton.onClick.AddListener(CloseMenu);
        searchField.onValueChanged.AddListener(FilterUserList); 

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    currentUserId = auth.CurrentUser.UserId;
                    Debug.Log("Logged in as: " + currentUserId);
                    GetTotalUsers();
                }
                else
                {
                    Debug.LogError("User is not logged in.");
                }
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        notification = GameObject.Find("Notification").GetComponent<NotificationManager>();
    }

    private void CloseMenu()
    {
        UserListUI.SetActive(false);
    }

    private void OpenUserList()
    {
        UserListUI.SetActive(true);
        LoadUserList();
    }

    private void GetTotalUsers()
    {
        db.Collection("Users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int userCount = task.Result.Count;
                totalUsersText.text = "Total Users: " + userCount;
            }
            else
            {
                totalUsersText.text = "Total Users: 0";
                Debug.LogError("Failed to fetch user count: " + task.Exception);
            }
        });
    }

    private void LoadUserList()
    {
        allUsers.Clear(); 

        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("=== Requesting Users from Firestore ===");

        db.Collection("Users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"Firestore returned {task.Result.Count} user documents.");

                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    try
                    {
                        Dictionary<string, object> data = doc.ToDictionary();

                        if (data.ContainsKey("username") && data.ContainsKey("userID"))
                        {
                            string username = data["username"].ToString();
                            string userID = data["userID"].ToString();

                            if (userID != currentUserId) // Exclude the current user
                            {
                                allUsers.Add((userID, username)); 
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Document {doc.Id} is missing 'username' or 'userID' field.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing document {doc.Id}: {ex.Message}");
                    }
                }

               
                FilterUserList(searchField.text);
            }
            else
            {
                Debug.LogError($"Failed to load users from Firestore: {task.Exception}");
            }
        });
    }

    private void FilterUserList(string searchText)
    {
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        string searchLower = searchText.ToLower();

        int count = 0;
        foreach (var user in allUsers)
        {
            if (user.username.ToLower().Contains(searchLower)) 
            {
                CreateUserCard(user.userID, user.username);
                count++;
            }
        }

        Debug.Log($"Filtered Users: {count} matching '{searchText}'");
    }

    private void CreateUserCard(string userID, string username)
    {
        GameObject newUserCard = Instantiate(UserElementPrefab, contentArea);
        User_Element_Controller userController = newUserCard.GetComponent<User_Element_Controller>();

        if (userController == null || userController.usernameText == null)
        {
            Debug.LogError("User_Element_Controller or usernameText is missing on the instantiated prefab.");
            return;
        }

        userController.usernameText.text = username;

        if (userController.blockButton != null)
        {
            userController.blockButton.onClick.RemoveAllListeners();
            userController.blockButton.onClick.AddListener(() =>
            {
                blockManager?.OnBlockButtonPressed(userID);
            });
        }

        if (userController.reportButton != null)
        {
            userController.reportButton.onClick.RemoveAllListeners();
            userController.reportButton.onClick.AddListener(() =>
            {
                reportManager?.OpenReportPopup(userID);
            });
        }

        if (userController.addFriendButton != null)
        {
            userController.addFriendButton.onClick.RemoveAllListeners();
            db.Collection("Friends").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    DocumentSnapshot snapshot = task.Result;
                    List<string> friendsList = snapshot.Exists && snapshot.ContainsField("friends")
                        ? snapshot.GetValue<List<string>>("friends")
                        : new List<string>();

                    if (friendsList.Contains(userID))
                    {
                        userController.addFriendButton.GetComponentInChildren<TMP_Text>().text = "Already Friends";
                        userController.addFriendButton.interactable = false;
                    }
                    else
                    {
                        userController.addFriendButton.GetComponentInChildren<TMP_Text>().text = "Add Friend";
                        userController.addFriendButton.onClick.AddListener(() =>
                        {
                            AddFriend(userID);
                        });
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve friend list: " + task.Exception);
                }
            });
        }
    }

    public void AddFriend(string friendId)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("Current user ID is null or empty. User might not be logged in.");
            return;
        }

        DocumentReference docRef = db.Collection("Friends").Document(currentUserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;
                List<string> friendsList = snapshot.Exists && snapshot.ContainsField("friends")
                    ? snapshot.GetValue<List<string>>("friends")
                    : new List<string>();

                if (!friendsList.Contains(friendId))
                {
                    friendsList.Add(friendId);
                    docRef.SetAsync(new Dictionary<string, object> { { "friends", friendsList } }, SetOptions.MergeAll)
                        .ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                Debug.Log("Friend added successfully!");
                                notification.Notify("The invitation link has been sent!", 3f);
                            }
                            else
                            {
                                Debug.LogError("Failed to add friend: " + updateTask.Exception);
                            }
                        });
                }
                else
                {
                    Debug.Log("Friend already in the list.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve friend list: " + task.Exception);
            }
        });
    }
}
