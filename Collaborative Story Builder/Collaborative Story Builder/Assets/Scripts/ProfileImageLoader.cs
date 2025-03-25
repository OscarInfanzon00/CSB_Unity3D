using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;


public class ProfileImageLoader : MonoBehaviour
{
    public UnityEngine.UI.Image profilePic;

    void Start()
    {
        AvatarManager.Instance.OnAvatarUrlChanged += UpdateAvatar;

        if (!string.IsNullOrEmpty(AvatarManager.Instance.CurrentAvatarUrl))
        {
            UpdateAvatar(AvatarManager.Instance.CurrentAvatarUrl);
        }
        else
        {
            LoadAvatarFromFirestore(); // First-time load
        }
    }

    private void OnDestroy()
    {
        if (AvatarManager.Instance != null)
            AvatarManager.Instance.OnAvatarUrlChanged -= UpdateAvatar;
    }

    private void LoadAvatarFromFirestore()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        FirebaseFirestore.DefaultInstance.Collection("Users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists && task.Result.ContainsField("avatarUrl"))
            {
                string avatarUrl = task.Result.GetValue<string>("avatarUrl");
                AvatarManager.Instance?.SetAvatarUrl(avatarUrl); // Also triggers all listeners
            }
        });
    }

    private void UpdateAvatar(string url)
    {
        StartCoroutine(LoadImageFromURL(url));
    }

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
                profilePic.sprite = sprite;
                profilePic.preserveAspect = true;
            }
            else
            {
                Debug.LogError("Failed to load image: " + www.error);
            }
        }
    }
}
