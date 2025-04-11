using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class MusicManagerScripot : MonoBehaviour
{
    public bool mainMenu;
    public List<AudioClip> MusicLibrary;
    public Sprite muteSoundImage, soundOnImage;
    public AudioSource audioSource;
    public GameObject MuteButton;
    public AudioClip currentSong;
    public Toggle MuteToggle; 
    
    
    void Start()
    {

        MuteToggle.onValueChanged.AddListener(SwitchMusicOnOff);
        
        if (mainMenu)
        {
            currentSong = MusicLibrary[6];
            audioSource.resource = currentSong;
        }
        else
        {
            SetRandomSong();
        }
    }

    public void SwitchMusicOnOff(bool isOn)
    {
        audioSource.mute = !isOn;

        MuteButton.transform.GetChild(0).GetComponent<Image>().sprite =
            isOn ? soundOnImage : muteSoundImage;
    }



    public void SwitchMusicOnOff()
    {
        switch (audioSource.mute)
        {
            case true: //Turn On
                audioSource.mute = false;
                MuteButton.transform.GetChild(0).GetComponent<Image>().sprite = soundOnImage;
                break;
            case false: //Turn Off
                audioSource.mute = true;
                MuteButton.transform.GetChild(0).GetComponent<Image>().sprite = muteSoundImage;
                break;
        }
    }

   

    // Update is called once per frame
    void Update()
    {
        if(Settings.musicVolume != audioSource.volume)
        {
            audioSource.volume = Settings.musicVolume;
        }
        if (audioSource.time >= audioSource.clip.length - 0.1f && !mainMenu)
        {
            SetRandomSong();
        }
    }
    public void SetRandomSong()
    {
        currentSong = MusicLibrary[Random.Range(0, 9)];
        audioSource.resource = currentSong;
        audioSource.Play();
        audioSource.loop = false;
    }
    public void NextSong()
    {
        int i = MusicLibrary.IndexOf(currentSong);
        if (i >= MusicLibrary.Count-1) i = -1; //index start from the beginning of the list
        currentSong = MusicLibrary[i+1];
        audioSource.resource = currentSong;
        audioSource.Play();
        audioSource.loop = false;
    }
}
