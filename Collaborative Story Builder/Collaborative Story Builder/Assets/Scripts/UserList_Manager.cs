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
    public BlockManager blockManager;
    public ReportManager reportManager;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;

    void Start()
    {
        openUserListButton.onClick.AddListener(OpenUserList);
        closeButton.onClick.AddListener(CloseMenu);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                if (auth.CurrentUser != null)
                {
                    currentUserId = auth.CurrentUser.UserId;  // Get logged-in user ID
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
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("=== Requesting Users from Firestore ===");

        db.Collection("Users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                int totalDocuments = task.Result.Count;
                Debug.Log($"Firestore returned {totalDocuments} user documents."); // Log how many documents were retrieved

                int count = 0; // Track number of users processed

                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    try
                    {
                        Dictionary<string, object> data = doc.ToDictionary();
                        Debug.Log($"Processing Document ID: {doc.Id}"); // Log each document's ID before reading data

                        if (data.ContainsKey("username") && data.ContainsKey("userID"))
                        {
                            string username = data["username"].ToString();
                            string userID = data["userID"].ToString();

                            Debug.Log($"User {count + 1}: Username = {username}, ID = {userID}");

                            if (userID != currentUserId) // Don't show yourself in the list
                            {
                                count++;
                                CreateUserCard(userID, username);
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

                Debug.Log($"Total Users Retrieved and Processed from Firestore: {count}");
            }
            else
            {
                Debug.LogError($"Failed to load users from Firestore: {task.Exception}");
            }
        });
    }




    private void CreateUserCard(string userID, string username)
    {
        Debug.Log($"Creating user card for: {username} (ID: {userID})");

        // Ensure UserElementPrefab is assigned
        if (UserElementPrefab == null)
        {
            Debug.LogError("UserElementPrefab is not assigned in the Inspector.");
            return;
        }

        GameObject newUserCard = Instantiate(UserElementPrefab, contentArea);
        if (newUserCard == null)
        {
            Debug.LogError("Failed to instantiate UserElementPrefab.");
            return;
        }

        User_Element_Controller userController = newUserCard.GetComponent<User_Element_Controller>();
        if (userController == null)
        {
            Debug.LogError("User_Element_Controller script is missing on the instantiated prefab.");
            return;
        }

        // Ensure the usernameText field is assigned
        if (userController.usernameText == null)
        {
            Debug.LogError("usernameText is not assigned in User_Element_Controller. Check the prefab setup.");
            return;
        }

        // Set username
        userController.usernameText.text = username;

        // Ensure block button is assigned before adding event listener
        if (userController.blockButton != null)
        {
            userController.blockButton.onClick.RemoveAllListeners(); // Prevent duplicate listeners
            userController.blockButton.onClick.AddListener(() =>
            {
                if (blockManager != null)
                {
                    blockManager.OnBlockButtonPressed(userID);
                }
                else
                {
                    Debug.LogError("BlockManager reference is missing.");
                }
            });
        }
        else
        {
            Debug.LogError("Block button is not assigned in User_Element_Controller.");
        }

        // Ensure report button is assigned before adding event listener
        if (userController.reportButton != null)
        {
            userController.reportButton.onClick.RemoveAllListeners(); // Prevent duplicate listeners
            userController.reportButton.onClick.AddListener(() =>
            {
                if (reportManager != null)
                {
                    reportManager.OpenReportPopup(userID);
                }
                else
                {
                    Debug.LogError("ReportManager reference is missing.");
                }
            });
        }
        else
        {
            Debug.LogError("Report button is not assigned in User_Element_Controller.");
        }

        // Ensure add friend button is assigned before adding event listener
        if (userController.addFriendButton != null)
        {
            userController.addFriendButton.onClick.RemoveAllListeners(); // Prevent duplicate listeners
            userController.addFriendButton.onClick.AddListener(() =>
            {
                AddFriend(userID);
            });
        }
        else
        {
            Debug.LogError("Add Friend button is not assigned in User_Element_Controller.");
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
                List<string> friendsList = new List<string>();

                if (snapshot.Exists && snapshot.ContainsField("friends"))
                {
                    friendsList = snapshot.GetValue<List<string>>("friends");
                }

                if (!friendsList.Contains(friendId))
                {
                    friendsList.Add(friendId);
                    docRef.SetAsync(new Dictionary<string, object> { { "friends", friendsList } }, SetOptions.MergeAll)
                        .ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                            {
                                Debug.Log("Friend added successfully!");
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

