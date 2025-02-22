using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class User_Element_Controller : MonoBehaviour
{
    public TMP_Text usernameText;  // Reference to username text
    public Button addFriendButton;
    public Button reportButton;
    public Button blockButton;

    // Function to setup UI elements properly
    public void Setup(string username, string userID, UserList_Manager userManager)
    {
        usernameText.text = username; // Set the username text

        // Add Friend Button
        addFriendButton.onClick.RemoveAllListeners();  // Ensure no duplicate listeners
        addFriendButton.onClick.AddListener(() =>
        {
            userManager.AddFriend(userID);
        });

        // Report Button (Optional)
        reportButton.onClick.RemoveAllListeners();
        reportButton.onClick.AddListener(() =>
        {
            userManager.reportManager.OpenReportPopup(userID);
        });

        // Block Button (Optional)
        blockButton.onClick.RemoveAllListeners();
        blockButton.onClick.AddListener(() =>
        {
            userManager.blockManager.OnBlockButtonPressed(userID);
        });
    }
}
