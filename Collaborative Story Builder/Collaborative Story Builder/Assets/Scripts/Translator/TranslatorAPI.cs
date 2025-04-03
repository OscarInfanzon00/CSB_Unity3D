using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using TMPro;
public class TranslatorAPI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI inputField;
    [SerializeField] private TextMeshProUGUI outputText;
    [SerializeField] private Button translateButton;

    public string inputLanguage, outputLanguage;
    private string myMemoryUrl = "https://api.mymemory.translated.net/get";

    void Start()
    {

        translateButton.onClick.AddListener(OnTranslateButtonClicked);
    }
    private void Update()
    {
        if(Settings.inputLangTranslation != inputLanguage)
        {
            inputLanguage = Settings.inputLangTranslation;
        }
        if (Settings.outputLangTranslation != outputLanguage)
        {
            outputLanguage = Settings.outputLangTranslation;
        }
    }
    void OnTranslateButtonClicked()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            StartCoroutine(Translate(inputField.text));
        }
        else
        {
            outputText.text = "no text";
        }
    }

    IEnumerator Translate(string textToTranslate)
    {
        // Construct the URL with escaped text and language pair (en|ru)
        string url = $"{myMemoryUrl}?q={UnityWebRequest.EscapeURL(textToTranslate)}&langpair={inputLanguage}|{outputLanguage}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            Debug.Log("Sending request to: " + url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Response: " + jsonResponse);
                MyMemoryResponse response = JsonUtility.FromJson<MyMemoryResponse>(jsonResponse);
                outputText.text = response.responseData.translatedText;
            }
            else
            {
                Debug.LogError($"Request failed: {request.error} - Response Code: {request.responseCode}");
                Debug.LogError($"Response Text: {request.downloadHandler?.text}");
                outputText.text = $"Error: {request.error}";
            }
        }
    }
}

[System.Serializable]
public class MyMemoryResponse
{
    public ResponseData responseData;
}

[System.Serializable]
public class ResponseData
{
    public string translatedText;
}