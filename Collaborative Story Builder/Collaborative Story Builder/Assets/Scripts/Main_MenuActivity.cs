using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.U2D.Aseprite;
public class Main_MenuActivity : MonoBehaviour
{

    public GameObject MainMenuPanel, ProfilePanel, LobbyPanel;
    public Button openProfileButton;
    public Button openMultiplayerMenuButton;
    public TextMeshProUGUI username;
    public Button btnTutorial;
    public Animator tutorial;

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


        tutorial.enabled = false;

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
        tutorial.enabled = true;
        tutorial.Play("Tutorial", 0, 0f);
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

    
    public void openTestingRoom()
    {
        SceneManager.LoadScene("Testing_Room");
        
    }

    
}
