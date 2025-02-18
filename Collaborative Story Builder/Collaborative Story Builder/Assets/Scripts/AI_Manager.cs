using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Text;

//Hey guys, this is the global static class for the project, it makes use of DeekSeep Free API. 
//For all the AI related features, you should use it and call the main method for your User Story.
//The way you should do it is: 
/*
        StartCoroutine(AI_Manager.GetChatCompletion("THE QUESTION OR REQUEST GOES HERE", response =>
        {
            Debug.Log("AI Response: " + THIS IS THE RESULT, USE IT IN FOR YOUR PURPOSE);
        }));
*/
//You have to use a coroutine because AI is not an instant thing and you have to wait for the answer, have it in consideration for your development.
//Everything inside the coroutine will wait for the answer from the AI to be executed.
//Let me know if you need help. Oscar

public static class AI_Manager
{
    private const string API_URL = "https://openrouter.ai/api/v1";
    private const string API_KEY = "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";

    public static IEnumerator GetChatCompletion(string prompt, Action<string> callback)
    {
        string jsonData = "{\"model\": \"google/gemini-2.0-flash-exp:free\", \"messages\": [{\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "/chat/completions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + API_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(jsonResponse);

                if (response != null && response.choices != null && response.choices.Length > 0 && response.choices[0].message != null)
                {
                    callback?.Invoke(response.choices[0].message.content);
                }
                else
                {
                    Debug.LogError("Response failed :(");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }
}

[System.Serializable]
public class OpenAIResponse
{
    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public Message message;
}

[System.Serializable]
public class Message
{
    public string content;
}
