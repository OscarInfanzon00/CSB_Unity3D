using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;


public class FriendCardController : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text levelText;
    public Button inviteButton;
    public Button cardClickButton;// Clickable card to open popup

    private string friendId;
    private string username;
    private int level;
    private int words;
    private string avatarUrl;      // 🆕 Store avatar URL
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    public UnityEngine.UI.Image avatarImage;

    private FriendsListManager manager;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            currentUserId = auth.CurrentUser.UserId;
        }

        if (inviteButton != null)
        {
            inviteButton.onClick.AddListener(SendInvite);
        }

        if (cardClickButton != null)
        {
            cardClickButton.onClick.AddListener(OpenPopup);
        }
    }

    // 🆕 Updated Setup to receive avatarUrl
    public void Setup(string username, int level, string friendId, int words, string avatarUrl, FriendsListManager manager)
    {
        this.username = username;
        this.level = level;
        this.friendId = friendId;
        this.words = words;
        this.avatarUrl = avatarUrl;
        this.manager = manager;

        if (usernameText != null)
            usernameText.text = username;

        if (levelText != null)
            levelText.text = "Level: " + level;

        if (!string.IsNullOrEmpty(avatarUrl) && avatarImage != null)
        {
            StartCoroutine(LoadImageFromURL(avatarUrl));
        }
    }

    private void OpenPopup()
    {
        if (manager != null)
        {
            manager.OpenFriendsPopup(friendId, username, level, words, avatarUrl); // 🆕 Pass avatar to popup
        }
    }

    private void SendInvite()
    {
        if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendId))
        {
            Debug.LogError("User ID or Friend ID is missing.");
            return;
        }

        db.Collection("UserInvitations").Document(friendId).SetAsync(new
        {
            inviterId = currentUserId,
            inviteeId = friendId,
            timestamp = Timestamp.GetCurrentTimestamp()
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"Invitation sent to {friendId}");
            }
            else
            {
                Debug.LogError("Failed to send invitation.");
            }
        });
    }

    // 🆕 Load avatar from URL
    private IEnumerator LoadImageFromURL(string url)
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
                avatarImage.sprite = sprite;
                avatarImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning("Failed to load avatar image: " + www.error);
            }
        }
    }
}
