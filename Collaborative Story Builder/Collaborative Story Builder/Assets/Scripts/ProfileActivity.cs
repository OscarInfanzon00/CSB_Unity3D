using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;

public class ProfileActivity : MonoBehaviour
{
    public GameObject MainMenuPanel, ProfilePanel, FriendsPanel;
    public TextMeshProUGUI usernameMainMenuText;
    public Button closeButton;
    public TMP_InputField email;
    public TMP_InputField username;
    public Button saveButton;
    public Button logoutButton;
    public Button profilePicButton;
    public Button friedListButton; 
    public Image profilePic;
    public Slider lvlSlider;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI txtWordCounter;
    public NotificationManager notification;
    public int maxImageSize = 512;
    private string persistentImageName = "profilePic.png";

    private UserData user;

    public TextMeshProUGUI XPtext;

    private FirebaseFirestore dbReference;

    void Start()
    {
        saveButton.onClick.AddListener(saveUsername);
        logoutButton.onClick.AddListener(logout);
        closeButton.onClick.AddListener(closeProfile);
        profilePicButton.onClick.AddListener(OnProfilePicButtonClicked);
        friedListButton.onClick.AddListener(OpenFriendsList); 

        user = User.GetUser();

        if (user.Email!="defaultEmail")
        {
            email.text = user.Email;
            username.text = user.Email;
        }
        if (user.Username!="defaultUser")
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

    private void updateLVL(){
        if (user.UserLevel!=0)
        {
            XPtext.text = "XP "+ PlayerPrefs.GetInt("XP", 0);
            lvlText.text = "LVL: "+ user.UserLevel;
        }else{
            XPtext.text = "XP "+ 0;
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
}
