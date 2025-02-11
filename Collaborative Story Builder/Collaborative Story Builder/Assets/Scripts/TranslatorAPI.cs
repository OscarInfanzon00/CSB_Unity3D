using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
 

public class TranslatorAPI : MonoBehaviour
{
    private const string apiUrl = "https://libretranslate.com/translate";

    void Start()
    {
        StartCoroutine(TranslateText("Hello, how are you?", "ru"));
    }

    IEnumerator TranslateText(string text, string targetLanguage)
    {
        // Create JSON payload
        string jsonData = "{\"q\": \"" + text + "\", \"source\": \"auto\", \"target\": \"" + targetLanguage + "\", \"format\": \"text\", \"alternatives\": 3, \"api_key\": \"\"}";

        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Send request
            yield return request.SendWebRequest();

            // Handle response
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
