using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mixer;
    public Toggle muteToggle;

    private void Start()
    {
        // Load saved state
        if (PlayerPrefs.HasKey("Muted"))
        {
            bool isMuted = PlayerPrefs.GetInt("Muted") == 1;
            muteToggle.isOn = isMuted;
            SetMute(isMuted);
        }

        muteToggle.onValueChanged.AddListener(SetMute);
    }

    public void SetMute(bool isMuted)
    {
        mixer.SetFloat("MasterVolume", isMuted ? -80f : 0f); // -80 dB is effectively mute
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
