using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;

public class FriendsListManager : MonoBehaviour
{
    public GameObject FriendListUI;
    public Transform contentArea;
    public GameObject FriendCardPrefab;
    public TMP_Text friendsCounterText;
    public Button closeButton;
    public Button refreshButton;
    public TMP_InputField searchField;

    public GameObject friendsPopup;
    public TMP_Text popupUsernameText;
    public TMP_Text popupLevelText;
    public TMP_Text popupWordsText;
    public UnityEngine.UI.Image popupAvatarImage; // 🆕 Avatar image in popup
    public Button closeFriendPopupBtn;
    public Button removeFriendButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    private string selectedFriendId;

    // 🆕 Store all friend data including avatar URL
    private List<(string friendId, string username, int level, int words, string avatarUrl)> allFriends = new();

    // 🆕 Default avatar fallback
    private const string defaultAvatarUrl = "https://as2.ftcdn.net/jpg/03/31/69/91/1000_F_331699188_lRpvqxO5QRtwOM05gR50ImaaJgBx68vi.jpg";

    void Start()
    {
        closeFriendPopupBtn.onClick.AddListener(CloseFriendsPopup);
        closeButton.onClick.AddListener(CloseFriendsPanel);
        removeFriendButton.onClick.AddListener(RemoveFriend);
        refreshButton.onClick.AddListener(LoadFriendsList);
        searchField.onValueChanged.AddListener(FilterFriendsList);

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
        allFriends.Clear();

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
                    string avatarUrl = friendSnapshot.ContainsField("avatarUrl") ? friendSnapshot.GetValue<string>("avatarUrl") : defaultAvatarUrl;

                    username = FormatUsername(username);

                    allFriends.Add((friendId, username, level, words, avatarUrl));
                    FilterFriendsList(searchField.text);
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
            if (friend.username.ToLower().Contains(searchLower))
            {
                CreateFriendCard(friend.friendId, friend.username, friend.level, friend.words, friend.avatarUrl);
                count++;
            }
        }

        Debug.Log($"Filtered Friends: {count} matching '{searchText}'");
    }

    private void CreateFriendCard(string friendId, string username, int level, int words, string avatarUrl)
    {
        if (FriendCardPrefab == null) return;

        GameObject newFriendCard = Instantiate(FriendCardPrefab, contentArea);
        if (newFriendCard == null) return;

        FriendCardController friendController = newFriendCard.GetComponent<FriendCardController>();
        if (friendController != null)
        {
            friendController.Setup(username, level, friendId, words, avatarUrl, this);
        }
    }

    private void UpdateFriendsCounter(int count)
    {
        if (friendsCounterText != null)
        {
            friendsCounterText.text = $"Friends: {count}";
        }
    }

    // 🆕 Now receives avatarUrl
    public void OpenFriendsPopup(string friendId, string username, int level, int words, string avatarUrl)
    {
        selectedFriendId = friendId;
        username = FormatUsername(username);

        if (popupUsernameText != null) popupUsernameText.text = username;
        if (popupLevelText != null) popupLevelText.text = "Level: " + level;
        if (popupWordsText != null) popupWordsText.text = "Words: " + words;

        if (!string.IsNullOrEmpty(avatarUrl) && popupAvatarImage != null)
        {
            StartCoroutine(LoadPopupImageFromURL(avatarUrl));
        }

        friendsPopup.SetActive(true);
    }

    private IEnumerator LoadPopupImageFromURL(string url)
    {
        using (WWW www = new WWW(url))
        {
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                Texture2D texture = www.texture;
                Sprite sprite = Sprite.Create(texture,
                                              new Rect(0, 0, texture.width, texture.height),
                                              new Vector2(0.5f, 0.5f));
                popupAvatarImage.sprite = sprite;
                popupAvatarImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning("Failed to load popup avatar: " + www.error);
            }
        }
    }

    public void CloseFriendsPopup()
    {
        friendsPopup.SetActive(false);
    }

    private void RemoveFriend()
    {
        if (string.IsNullOrEmpty(selectedFriendId)) return;

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
