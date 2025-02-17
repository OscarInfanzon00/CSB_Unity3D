using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
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

    void Start()
    {
        openUserListButton.onClick.AddListener(openUserList);
        closeButton.onClick.AddListener(closeMenu);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                db = FirebaseFirestore.DefaultInstance;
                GetTotalUsers();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    private void closeMenu()
    {
        UserListUI.SetActive(false); 
    }

    private void openUserList()
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

        db.Collection("Users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    Dictionary<string, object> data = doc.ToDictionary();

                    if (data.ContainsKey("username") && data.ContainsKey("userID"))
                    {
                        string username = data["username"].ToString();
                        string userID = data["userID"].ToString();
                        CreateUserCard(userID, username);
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load users: " + task.Exception);
            }
        });
    }

   private void CreateUserCard(string userID, string username)
    {
        GameObject newUserCard = Instantiate(UserElementPrefab, contentArea); 
        User_Element_Controller userController = newUserCard.GetComponent<User_Element_Controller>();

        if (userController != null)
        {
            userController.usernameText.text = username; 
            
            userController.blockButton.onClick.AddListener(() =>
            {
                if (blockManager != null) {
                    blockManager.OnBlockButtonPressed(userID);
                }
                else {
                    Debug.LogError("BlockManager reference is missing.");
                }
            });

            userController.reportButton.onClick.AddListener(() => 
            {
                if (reportManager != null) {
                    reportManager.OpenReportPopup(userID);
                }
                else {
                    Debug.LogError("ReportManager reference is missing.");
                }
            });
        }

    }
}
