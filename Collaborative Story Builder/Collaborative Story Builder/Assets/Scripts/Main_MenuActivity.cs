using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class Main_MenuActivity : MonoBehaviour
{

    public GameObject MainMenuPanel, ProfilePanel, LobbyPanel, TutorialPanel1, TutorialPanel2, TutorialPanel3,
    TutorialPanel4, TutorialPanel5, TutorialPanel6, onboardingMessage;
    public Button openProfileButton;
    public Button openMultiplayerMenuButton;
    public TextMeshProUGUI username;
    public Button btnTutorial;
    
    public GameObject AiGenButtonsPanel;
    public Button AiGenMenuBtn;

    public Slider lvlSlider;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI XPtext;

    public Button openTestingRoomButton;

    private UserData user;

    void Start()
    {
        openProfileButton.onClick.AddListener(openProfile);
        openMultiplayerMenuButton.onClick.AddListener(openMultiplayer);
        btnTutorial.onClick.AddListener(playTutorial);
        openTestingRoomButton.onClick.AddListener(openTestingRoom);
        AiGenMenuBtn.onClick.AddListener(openAiGenMenu);

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
        AiGenButtonsPanel.SetActive(false);
        
        createOnboardingMessage();
    }

    private void openAiGenMenu()
    {
        AiGenButtonsPanel.SetActive(!AiGenButtonsPanel.activeSelf);
    }


    private void updateLVL(){
        if (user.UserLevel!=0)
        {
            XPtext.text = "XP "+ PlayerPrefs.GetInt("XP", 0);
            lvlText.text = "LVL: "+ user.UserLevel;
        }else{
            XPtext.text = "XP "+ 0;
            lvlText.text = "LVL: Newbie";
        }
        lvlSlider.value = PlayerPrefs.GetInt("XP", 0);
        lvlSlider.maxValue = LevelSystem.GetXPForNextLevel();
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

    public void createOnboardingMessage(){
        if(PlayerPrefs.GetInt("login")==1){
            onboardingMessage.SetActive(true);
            Invoke("closeOnboardingMessage", 5f);
            PlayerPrefs.SetInt("login", 0);
            PlayerPrefs.Save();
        }
    }

    public void closeOnboardingMessage(){
        onboardingMessage.SetActive(false);
    }
}
