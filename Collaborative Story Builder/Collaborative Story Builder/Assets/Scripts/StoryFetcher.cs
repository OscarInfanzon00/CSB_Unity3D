using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryFetcher : MonoBehaviour
{
    public Button storyButton;
    public Button closeButton;
    public TMP_Text storyText;
    public GameObject storyPopup, AiGenMenuPopup;

    private string placeholderText = "Fetching your story... Please wait.";
    private string errorText = "Oops! Something went wrong. Try again.";

    private void Start()
    {
        if (storyButton != null)
        {
            storyButton.onClick.AddListener(GetStory);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStoryPopup);
        }

        storyPopup.SetActive(false); 
    }

    public void GetStory()
    {
        
        storyText.text = placeholderText;
        storyPopup.SetActive(true);
        AiGenMenuPopup.SetActive(!AiGenMenuPopup.activeSelf);

        
        StartCoroutine(AI_Manager.GetChatCompletion("Do it different every time. Write a short story (under 600 words) with a clear and engaging title. The story should belong to one of the following genres: history, comedy, or fantasy. If historical, focus on a humorous or intriguing event with entertaining characters. If fantasy, create a magical world with whimsical elements. If comedic, use witty dialogue and amusing situations to bring laughter. The story should be fun, adventurous, and suitable for all audiences, avoiding political, religious, or mature themes.", response =>
        {
            if (!string.IsNullOrEmpty(response)) 
            {
                Debug.Log("AI Response: " + response);
                storyText.text = response;
            }
            else
            {
                Debug.LogError("Empty AI Response.");
                storyText.text = errorText;
            }
        }));
    }

    public void CloseStoryPopup()
    {
        storyPopup.SetActive(false);
        storyText.text = placeholderText; 
    }
}
