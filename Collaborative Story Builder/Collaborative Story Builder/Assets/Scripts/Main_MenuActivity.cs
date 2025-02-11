using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.U2D.Aseprite;
public class Main_MenuActivity : MonoBehaviour
{

    public GameObject MainMenuPanel, ProfilePanel, LobbyPanel, TutorialPanel1, TutorialPanel2, TutorialPanel3,
    TutorialPanel4, TutorialPanel5, TutorialPanel6;
    public Button openProfileButton;
    public Button openMultiplayerMenuButton;
    public TextMeshProUGUI username;
    public Button btnTutorial;

    public Slider lvlSlider;
    public TextMeshProUGUI lvlText;

    public Button openTestingRoomButton;

    private UserData user;

    void Start()
    {
        openProfileButton.onClick.AddListener(openProfile);
        openMultiplayerMenuButton.onClick.AddListener(openMultiplayer);
        btnTutorial.onClick.AddListener(playTutorial);
        openTestingRoomButton.onClick.AddListener(openTestingRoom);


        user = User.GetUser();

        if (user.Username!="defaultUser")
        {
            username.text = user.Username;
        }
        else if (user.Email!="defaultEmail")
        {
            username.text = user.Email;
        }

        updateLVL();
    }

    private void updateLVL(){
        if (user.UserLevel!=0)
        {
            lvlSlider.value = user.UserLevel;
            lvlText.text = "LVL: "+ user.UserLevel;
        }else{
            lvlSlider.value = 0;
            lvlText.text = "LVL: Newbie";
        }
    }

    private void playTutorial()
    {
        if(TutorialPanel1.active){
            TutorialPanel1.SetActive(false);
            TutorialPanel2.SetActive(false);
            TutorialPanel3.SetActive(false);
            TutorialPanel4.SetActive(false);
            TutorialPanel5.SetActive(false);
            TutorialPanel6.SetActive(false);
        }else{
             TutorialPanel1.SetActive(true);
             TutorialPanel2.SetActive(true);
             TutorialPanel3.SetActive(true);
             TutorialPanel4.SetActive(true);
             TutorialPanel5.SetActive(true);
             TutorialPanel6.SetActive(true);
        }
    }

    private void openMultiplayer()
    {
        ProfilePanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        LobbyPanel.SetActive(true);
    }

    private void openProfile()
    {
        ProfilePanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        LobbyPanel.SetActive(false);
    }

    
    public void openTestingRoom()
    {
        SceneManager.LoadScene("Testing_Room");
        
    }

    
}
