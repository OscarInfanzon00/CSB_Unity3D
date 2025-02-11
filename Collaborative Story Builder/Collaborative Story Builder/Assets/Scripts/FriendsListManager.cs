using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CloseButtonHandler : MonoBehaviour
{
    public Button CloseButton;
    public Button friendPanel;
    public GameObject FriendPopupPanel, MainMenuPanel, FriendsPanel;

    void Start()
    {
        CloseButton.onClick.AddListener(CloseScene);
        friendPanel.onClick.AddListener(ShowFriendInfo);
    }

    private void CloseScene()
    {
        MainMenuPanel.SetActive(true);
        FriendsPanel.SetActive(false);
    }

    private void ShowFriendInfo()
    {
        FriendPopupPanel.gameObject.SetActive(true);
    }
}
