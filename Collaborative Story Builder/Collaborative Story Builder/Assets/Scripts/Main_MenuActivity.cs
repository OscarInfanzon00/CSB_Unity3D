using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
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

    void Start()
    {
        openProfileButton.onClick.AddListener(openProfile);
        openMultiplayerMenuButton.onClick.AddListener(openMultiplayer);
        btnTutorial.onClick.AddListener(playTutorial);

        if (PlayerPrefs.HasKey("SavedUsername"))
        {
            username.text = PlayerPrefs.GetString("SavedUsername");
        }
        else if (PlayerPrefs.HasKey("SavedEmail"))
        {
            username.text = PlayerPrefs.GetString("SavedEmail");
        }

        updateLVL();
    }

    private void updateLVL(){
        if (PlayerPrefs.HasKey("lvl"))
        {
            lvlSlider.value = PlayerPrefs.GetInt("lvl");
            lvlText.text = "LVL: "+PlayerPrefs.GetInt("lvl");
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
        //SceneManager.LoadScene("LobbyActivity");
        ProfilePanel.SetActive(false);
        MainMenuPanel.SetActive(false);
        LobbyPanel.SetActive(true);
    }

    private void openProfile()
    {
        //SceneManager.LoadScene("Profile");
        ProfilePanel.SetActive(true);
        MainMenuPanel.SetActive(false);
        LobbyPanel.SetActive(false);
    }
}
