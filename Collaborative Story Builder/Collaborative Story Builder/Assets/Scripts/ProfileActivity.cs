using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;

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

    public int maxImageSize = 512;
    private string persistentImageName = "profilePic.png";

    void Start()
    {
        saveButton.onClick.AddListener(saveUsername);
        logoutButton.onClick.AddListener(logout);
        closeButton.onClick.AddListener(closeProfile);
        profilePicButton.onClick.AddListener(OnProfilePicButtonClicked);
        friedListButton.onClick.AddListener(OpenFriendsList); 

        if (PlayerPrefs.HasKey("SavedEmail"))
        {
            email.text = PlayerPrefs.GetString("SavedEmail");
        }
        if (PlayerPrefs.HasKey("SavedUsername"))
        {
            username.text = PlayerPrefs.GetString("SavedUsername");
        }
        else if (PlayerPrefs.HasKey("SavedEmail"))
        {
            username.text = PlayerPrefs.GetString("SavedEmail");
        }

        string savedPath = Path.Combine(Application.persistentDataPath, persistentImageName);
        if (File.Exists(savedPath))
        {
            LoadProfileImage(savedPath);
        }
        else
        {
            Debug.Log("No persistent profile image found at: " + savedPath);
        }

        updateLVL();

        if (PlayerPrefs.HasKey("words"))
        {
            txtWordCounter.text = "Words counter: " + PlayerPrefs.GetInt("words").ToString();
        }
    }

    private void updateLVL()
    {
        if (PlayerPrefs.HasKey("lvl"))
        {
            lvlSlider.value = PlayerPrefs.GetInt("lvl");
            lvlText.text = "LVL: " + PlayerPrefs.GetInt("lvl");
        }
        else
        {
            lvlSlider.value = 0;
            lvlText.text = "LVL: Newbie";
        }
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
    }

    private void closeProfile()
    {
        MainMenuPanel.SetActive(true);
        ProfilePanel.SetActive(false);
    }

    private void OpenFriendsList()
    {
        ProfilePanel.SetActive(false);
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
