using System;
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance;

    public string CurrentAvatarUrl { get; private set; }

    public event Action<string> OnAvatarUrlChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetAvatarUrl(string url)
    {
        if (url != CurrentAvatarUrl)
        {
            CurrentAvatarUrl = url;
            OnAvatarUrlChanged?.Invoke(url);
        }
    }
}
