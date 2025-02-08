using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class Main_MenuActivity : MonoBehaviour
{
    public Button openProfileButton;
    public Button openMultiplayerMenuButton;
    public Text username;
    public Button btnTutorial;
    public Animator tutorial;

    public Slider lvlSlider;
    public Text lvlText;

    void Start()
    {
        openProfileButton.onClick.AddListener(openProfile);
        openMultiplayerMenuButton.onClick.AddListener(openMultiplayer);
        btnTutorial.onClick.AddListener(playTutorial);

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
        SceneManager.LoadScene("LobbyActivity");
    }

    private void openProfile()
    {
        SceneManager.LoadScene("Profile");
    }
}
