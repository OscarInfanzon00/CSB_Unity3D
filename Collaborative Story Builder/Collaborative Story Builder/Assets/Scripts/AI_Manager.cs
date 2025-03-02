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

#region Gemini Response Classes
[Serializable]
public class Response
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public Content content;
}

[Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}
#endregion

public static class AI_Manager
{
    private static string apiKey = "AIzaSyA_5K_6hP7RpZ8Ivfmpn-GcqJQJOqSZo9s";
    private static string apiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

    public static IEnumerator GetChatCompletion(string prompt, Action<string> callback)
    {
        string url = $"{apiEndpoint}?key={apiKey}";
        string jsonData = "{\"contents\": [{\"parts\": [{\"text\": \"" + EscapeJson(prompt) + "\"}]}]}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                callback?.Invoke(ParseGeminiResponse(jsonResponse));
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                callback?.Invoke(null);
            }
        }
    }

    private static string ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            Response response = JsonUtility.FromJson<Response>(jsonResponse);
            if (response.candidates != null && response.candidates.Length > 0 &&
                response.candidates[0].content != null &&
                response.candidates[0].content.parts != null &&
                response.candidates[0].content.parts.Length > 0)
            {
                return response.candidates[0].content.parts[0].text;
            }
            return "No response.";
        }
        catch (Exception ex)
        {
            Debug.LogError("Parsing error: " + ex.Message);
            return "Error parsing response.";
        }
    }

    private static string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
