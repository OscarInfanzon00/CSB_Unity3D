using UnityEngine;
using UnityEngine.UI;

public class FriendUIManager : MonoBehaviour
{
    public GameObject popupPanel; 
    public Text nameText;
    public Text statusText;
    public Image profileImage;
    public Button closeButton; 

    private void Start()
    {
        popupPanel.SetActive(false); 

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideFriendInfo);
        }
    }

    public void ShowFriendInfo(string friendName, string friendStatus, Sprite friendImage)
    {
        nameText.text = friendName;
        statusText.text = friendStatus;
        profileImage.sprite = friendImage;

        popupPanel.SetActive(true);
    }

    public void HideFriendInfo()
    {
        popupPanel.SetActive(false);
    }
}
