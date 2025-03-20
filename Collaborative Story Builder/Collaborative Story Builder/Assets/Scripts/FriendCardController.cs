using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;



public class FriendCardController : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text levelText;
    public Button inviteButton; // 🆕 Reference to the invite button

    private string friendId;
    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;

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
    }

    public void Setup(string username, int level, string friendId)
    {
        this.friendId = friendId;
        if (usernameText != null)
            usernameText.text = username;

        if (levelText != null)
            levelText.text = "Level: " + level;
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
}
