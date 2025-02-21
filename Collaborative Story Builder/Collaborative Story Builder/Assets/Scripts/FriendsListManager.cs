using UnityEngine;
using UnityEngine.UI;

public class FriendsUIManager : MonoBehaviour
{
    public GameObject friendsPanel;     // The panel that contains the friends list
    public GameObject friendsPopup;     // The popup that shows friend details
    public Button closeFriendPanelBtn;  // Button to close friendsPanel
    public Button closeFriendPopupBtn;
    public Button FriendCard;

    void Start()
    {
        // Ensure panels are hidden at start
        FriendCard.onClick.AddListener(OpenFriendsPopup);
        // Add listeners for closing buttons
        closeFriendPanelBtn.onClick.AddListener(CloseFriendsPanel);
        closeFriendPopupBtn.onClick.AddListener(CloseFriendsPopup);
    }

    // Function to open the friends popup
    public void OpenFriendsPopup()
    {
        friendsPopup.SetActive(true);
    }

    // Function to close the friends popup
    public void CloseFriendsPopup()
    {
        friendsPopup.SetActive(false);
    }

    // Function to close the friends panel
    public void CloseFriendsPanel()
    {
        friendsPanel.SetActive(false);
    }
}
