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

public class ProfileActivity : MonoBehaviour
{
    public GameObject MainMenuPanel, ProfilePanel, FriendsPanel, BlockedUsersPanel;
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
    public Button blockedListButton;
    public Image profilePic;
    FirebaseFirestore db;
    public Slider lvlSlider;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI txtWordCounter;
    public NotificationManager notification;
    public int maxImageSize = 512;
    private string persistentImageName = "profilePic.png";
    private string storyID;

    private UserData user;

    public TextMeshProUGUI XPtext;

    private FirebaseFirestore dbReference;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        saveButton.onClick.AddListener(saveUsername);
        logoutButton.onClick.AddListener(logout);
        closeButton.onClick.AddListener(closeProfile);
        profilePicButton.onClick.AddListener(OnProfilePicButtonClicked);
        friedListButton.onClick.AddListener(OpenFriendsList);
        blockedListButton.onClick.AddListener(OpenBlockedUsersList);
        user = User.GetUser();

        if (user.Email != "defaultEmail")
        {
            email.text = user.Email;
            username.text = user.Email;
        }
        if (user.Username != "defaultUser")
        {
            username.text = user.Username;
        }

        string savedPath = Path.Combine(Application.persistentDataPath, persistentImageName);
        if (File.Exists(savedPath))
        {
            LoadProfileImage(savedPath);
        }
        else
        {
            Debug.Log("No profile image found at: " + savedPath);
        }

        updateLVL();

        dbReference = FirebaseFirestore.DefaultInstance;
        GetUserWords(user.UserID);

        loadStoryByID();
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

    private void OpenBlockedUsersList()
    {
        BlockedUsersPanel.SetActive(true);
    }
    private void OnProfilePicButtonClicked()
    {
        PickImage();
    }

    private void PickImage()
    {
        NativeGallery.GetImageFromGallery((string imagePath) =>
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                Debug.Log("Image selected: " + imagePath);
                CopyImageToPersistentPath(imagePath);
            }
            else
            {
                Debug.Log("No image was selected.");
            }
        }, "Select a PNG image", "image/png");
    }

    private void CopyImageToPersistentPath(string sourcePath)
    {
        try
        {
            string destPath = Path.Combine(Application.persistentDataPath, persistentImageName);

            byte[] imageData = File.ReadAllBytes(sourcePath);
            File.WriteAllBytes(destPath, imageData);

            Debug.Log("Copied image to persistent path: " + destPath);
            PlayerPrefs.SetString("ProfilePicPath", destPath);
            PlayerPrefs.Save();
            LoadProfileImage(destPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error copying image: " + ex.Message);
        }
    }

    private void LoadProfileImage(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogWarning("File does not exist at path: " + imagePath);
            return;
        }

        Texture2D texture = NativeGallery.LoadImageAtPath(imagePath, maxImageSize);
        if (texture != null)
        {
            Sprite newSprite = Sprite.Create(texture,
                                             new Rect(0, 0, texture.width, texture.height),
                                             new Vector2(0.5f, 0.5f));
            profilePic.sprite = newSprite;
            Debug.Log("Profile image loaded successfully from: " + imagePath);
        }
        else
        {
            Debug.LogWarning("Failed to load texture from: " + imagePath);
        }
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
                        {
                            Debug.LogWarning("No storyID found in document");
                            continue;
                        }

                        string storyID = (string)data["storyID"];
                        Debug.Log($"Found storyID: {storyID}");

                        // Check if the user is in the 'users' array
                        if (!data.ContainsKey("users") || !(data["users"] is List<object> userList))
                        {
                            Debug.Log($"Skipping story {storyID} - No users list found.");
                            continue;
                        }

                        List<string> owners = userList.Select(user => user.ToString()).ToList();
                        if (!owners.Contains(currentUserID))
                        {
                            Debug.Log($"Skipping story {storyID} - Current user is not an owner.");
                            continue;
                        }

                        List<string> storyTexts = new List<string>();
                        if (data.ContainsKey("storyTexts"))
                        {
                            foreach (var item in (List<object>)data["storyTexts"])
                            {
                                storyTexts.Add(item.ToString());
                            }
                        }

                        List<string> usernames = new List<string>();
                        if (data.ContainsKey("usernames"))
                        {
                            foreach (var item in (List<object>)data["usernames"])
                            {
                                usernames.Add(item.ToString());
                            }
                        }

                        DateTime timestamp = DateTime.MinValue;
                        if (data.ContainsKey("timestamp") && data["timestamp"] is Firebase.Firestore.Timestamp firestoreTimestamp)
                        {
                            timestamp = firestoreTimestamp.ToDateTime();
                        }

                        string previewText = storyTexts.Count > 0 ? storyTexts[0] : "No preview available";
                        Story newStory = new Story(storyID, previewText, storyTexts, usernames, timestamp);

                        matchedStories.Add(newStory);
                    }

                    // Sort stories by timestamp (latest first)
                    matchedStories.Sort((s1, s2) => s2.timestamp.CompareTo(s1.timestamp));

                    // Display matched stories
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
