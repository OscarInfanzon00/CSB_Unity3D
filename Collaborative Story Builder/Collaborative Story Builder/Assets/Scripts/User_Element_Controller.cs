using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class User_Element_Controller : MonoBehaviour
{
    public TMP_Text usernameText; 
    public Button addFriendButton;
    public Button reportButton;
    public Button blockButton;

    void Awake()
    {
        
    }

    public void Setup(string username, string userID, UserList_Manager userManager)
    {
        usernameText.text = username; 

        addFriendButton.onClick.RemoveAllListeners(); 
        addFriendButton.onClick.AddListener(() =>
        {
            userManager.AddFriend(userID);
        });

        reportButton.onClick.RemoveAllListeners();
        reportButton.onClick.AddListener(() =>
        {
            userManager.reportManager.OpenReportPopup(userID);
        });

        blockButton.onClick.RemoveAllListeners();
        blockButton.onClick.AddListener(() =>
        {
            userManager.blockManager.OnBlockButtonPressed(userID);
        });
    }
}
