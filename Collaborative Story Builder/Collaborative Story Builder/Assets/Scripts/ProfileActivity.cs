using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;
using System.Linq;
using Firebase.Auth;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;


public class ProfileActivity : MonoBehaviour
{
    public GameObject MainMenuPanel, ProfilePanel, FriendsPanel;
    public Transform storyListContainer;
    public GameObject storyCardPrefab;
    public GameObject StoryViewerUI;

    public TextMeshProUGUI usernameMainMenuText;
    public Button closeButton;
    public TMP_InputField email;
    public TMP_InputField username;
    public Button saveButton;
    public Button logoutButton;
    public Button profilePicButton;
    public Button friedListButton;
    public UnityEngine.UI.Image profilePic;


    public Slider lvlSlider;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI txtWordCounter;
    public NotificationManager notification;
    public int maxImageSize = 512;
    private string persistentImageName = "profilePic.png";
    private string storyID;

    private UserData user;
    private FirebaseFirestore db;
    private FirebaseFirestore dbReference;
    private string currentUserId;

    public TextMeshProUGUI XPtext;

    // 🆕 Avatar selection panel and buttons
    public GameObject avatarSelectionPanel, CustomFieldPanel;
    public List<Button> defaultAvatarButtons; // Assign 5 buttons in Inspector
    public Button customAvatarButton;
    public TMP_InputField customURLInput;
    public Button confirmCustomButton;


    void Start()
    {

        db = FirebaseFirestore.DefaultInstance;
        dbReference = FirebaseFirestore.DefaultInstance;
        currentUserId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        saveButton.onClick.AddListener(saveUsername);
        logoutButton.onClick.AddListener(logout);
        closeButton.onClick.AddListener(closeProfile);
        profilePicButton.onClick.AddListener(OnProfilePicButtonClicked);
        friedListButton.onClick.AddListener(OpenFriendsList);

        user = User.GetUser();

        if (user.Email != "defaultEmail") email.text = user.Email;
        if (user.Username != "defaultUser") username.text = user.Username;

        // 🆕 Load avatar from Firestore if saved
        if (!string.IsNullOrEmpty(currentUserId))
        {
            db.Collection("Users").Document(currentUserId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result.Exists && task.Result.ContainsField("avatarUrl"))
                {
                    string avatarUrl = task.Result.GetValue<string>("avatarUrl");
                    SelectAvatar(avatarUrl); // Load saved avatar
                }
            });
        }

        // 🆕 Set up default avatar button listeners
        defaultAvatarButtons[0].onClick.AddListener(() => SelectAvatar("https://img.freepik.com/premium-vector/men-icon-trendy-avatar-character-cheerful-happy-people-flat-vector-illustration-round-frame-male-portraits-group-team-adorable-guys-isolated-white-background_275421-282.jpg?w=1060"));
        defaultAvatarButtons[1].onClick.AddListener(() => SelectAvatar("https://img.freepik.com/premium-vector/women-trendy-icon-avatar-character-cheerful-happy-people-flat-vector-illustration-round-frame-female-portraits-group-team-adorable-girl-isolated-white-background_275421-271.jpg"));
        defaultAvatarButtons[2].onClick.AddListener(() => SelectAvatar("https://img.freepik.com/premium-vector/men-icon-trendy-avatar-character-cheerful-happy-people-flat-vector-illustration-round-frame-male-portraits-group-team-adorable-guys-isolated-white-background_275421-280.jpg"));
        defaultAvatarButtons[3].onClick.AddListener(() => SelectAvatar("https://img.freepik.com/premium-vector/men-icon-trendy-avatar-character-cheerful-happy-people-flat-vector-illustration-round-frame-male-portraits-group-team-adorable-guys-isolated-white-background_275421-281.jpg"));
        defaultAvatarButtons[4].onClick.AddListener(() => SelectAvatar("https://img.freepik.com/premium-vector/men-avatars-characters-cheerful-happy-people-flat-vector-illustration-set-male-female-portraits-group-team-adorable-guys-girls-trendy-pack_275421-1300.jpg"));

        //custom URL input
        customAvatarButton.onClick.AddListener(() =>
        {   
            CustomFieldPanel.SetActive(true);
        });

        //Confirm and apply custom avatar
        confirmCustomButton.onClick.AddListener(() =>
        {
            string url = customURLInput.text;
            if (!string.IsNullOrEmpty(url))
            {
                SelectAvatar(url);
            }
        });

        updateLVL();
        GetUserWords(user.UserID);
        loadStoryByID();
    }

    private void OnProfilePicButtonClicked()
    {
        //Show avatar selection panel
        avatarSelectionPanel.SetActive(true);
    }

    private void SelectAvatar(string url)
    {
        StartCoroutine(LoadImageFromURL(url, (sprite) =>
        {
            if (sprite != null)
            {
                profilePic.sprite = sprite;
                SaveAvatarToFirestore(url);
                AvatarManager.Instance?.SetAvatarUrl(url);


                // Hide panel and reset input
                avatarSelectionPanel.SetActive(false);
                CustomFieldPanel.SetActive(false);
                customURLInput.text = "";
            }
            else
            {
                Debug.LogWarning("Invalid avatar URL.");
            }
        }));
    }

    private IEnumerator LoadImageFromURL(string url, Action<Sprite> callback)
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
                callback(sprite);
            }
            else
            {
                Debug.LogError("Error loading image from URL: " + www.error);
                callback(null);
            }
        }
    }

    private void SaveAvatarToFirestore(string url)
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "avatarUrl", url }
        };

        db.Collection("Users").Document(currentUserId).SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Avatar URL saved to Firestore.");
            }
            else
            {
                Debug.LogError("Failed to save avatar URL: " + task.Exception);
            }
        });
        
    }


    private void logout()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene("Login");
    }

    private void saveUsername()
    {
        PlayerPrefs.SetString("SavedUsername", username.text);
        PlayerPrefs.Save();
        usernameMainMenuText.text = username.text;
        notification.Notify("The new username has been saved!", 3f);
    }

    private void closeProfile()
    {
        MainMenuPanel.SetActive(true);
        ProfilePanel.SetActive(false);
    }

    private void OpenFriendsList()
    {
        FriendsPanel.SetActive(true);
    }

    private void updateLVL()
    {
        if (user.UserLevel != 0)
        {
            XPtext.text = "XP " + PlayerPrefs.GetInt("XP", 0);
            lvlText.text = "LVL: " + user.UserLevel;
        }
        else
        {
            XPtext.text = "XP " + 0;
            lvlText.text = "LVL: Newbie";
        }
        lvlSlider.value = PlayerPrefs.GetInt("XP", 0);
        lvlSlider.maxValue = LevelSystem.GetXPForNextLevel();
    }

    public void GetUserWords(string userId)
    {
        dbReference.Collection("Users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot doc = task.Result;
                if (doc.Exists)
                {
                    int words = doc.ContainsField("words") ? doc.GetValue<int>("words") : 0;
                    txtWordCounter.text = "Words counter: " + words.ToString();
                }
                else
                {
                    Debug.Log($"User {userId} not found in Firestore.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch user data: " + task.Exception);
            }
        });
    }

    public void loadStoryByID()
    {
        string currentUserID = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        if (string.IsNullOrEmpty(currentUserID))
        {
            Debug.LogError("User is not authenticated.");
            return;
        }

        db.Collection("Stories").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                try
                {
                    List<Story> matchedStories = new List<Story>();

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        Dictionary<string, object> data = doc.ToDictionary();

                        if (!data.ContainsKey("storyID"))
                            continue;

                        string storyID = (string)data["storyID"];
                        if (!data.ContainsKey("users") || !(data["users"] is List<object> userList))
                            continue;

                        List<string> owners = userList.Select(user => user.ToString()).ToList();
                        if (!owners.Contains(currentUserID))
                            continue;

                        List<string> storyTexts = data.ContainsKey("storyTexts") ?
                            ((List<object>)data["storyTexts"]).Select(o => o.ToString()).ToList() : new List<string>();

                        List<string> usernames = data.ContainsKey("usernames") ?
                            ((List<object>)data["usernames"]).Select(o => o.ToString()).ToList() : new List<string>();

                        DateTime timestamp = DateTime.MinValue;
                        if (data.ContainsKey("timestamp") && data["timestamp"] is Firebase.Firestore.Timestamp ts)
                        {
                            timestamp = ts.ToDateTime();
                        }

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(storyID, previewText, storyTexts, usernames, timestamp);
                        matchedStories.Add(newStory);
                    }

                    matchedStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));
                    foreach (Story story in matchedStories)
                    {
                        CreateStoryCard(story);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to load stories: " + e);
                }
            }
            else
            {
                Debug.LogError("Failed to load stories: " + task.Exception);
            }
        });
    }

    void CreateStoryCard(Story story)
    {
        GameObject newCard = Instantiate(storyCardPrefab, storyListContainer);
        newCard.GetComponent<StoryCardUI>().StoryViewerUI = StoryViewerUI;
        StoryCardUI cardUI = newCard.GetComponent<StoryCardUI>();
        cardUI.SetStoryInfo(story);
        cardUI.storyID = story.storyRealID;
    }
}
