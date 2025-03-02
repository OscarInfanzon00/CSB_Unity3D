using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeechManager : MonoBehaviour
{
    public TextMeshProUGUI textContent;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Speak()
    {
        StartCoroutine(PlayTextToSpeech(textContent.text));
    }

    private IEnumerator PlayTextToSpeech(string words)
    {
        if (string.IsNullOrEmpty(words))
            yield break;

        Regex rgx = new Regex("\\s+");
        string formattedText = rgx.Replace(words, "%20");
        string url = "https://translate.google.com/translate_tts?tl=en&q=" + formattedText + "&client=gtx";

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching TTS audio: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
