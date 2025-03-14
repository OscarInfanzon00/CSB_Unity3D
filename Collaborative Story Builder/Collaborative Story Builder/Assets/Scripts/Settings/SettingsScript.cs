using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class SettingsScript : MonoBehaviour
{
    public GameObject SettingsPanel;
    public Slider musicVolumeSlider;
    public void SetMusicVolume()
    {
        Settings.musicVolume = musicVolumeSlider.value;
    }
    public void SwitchSettingPanel()
    {
        switch (SettingsPanel.activeInHierarchy)
        {
            case true:
                SettingsPanel.SetActive(false);
                break;
            case false:
                SettingsPanel.SetActive(true);
                break;
        }
        
    }
}
