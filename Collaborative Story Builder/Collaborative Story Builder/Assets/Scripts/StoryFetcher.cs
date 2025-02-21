using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryFetcher : MonoBehaviour
{
    public Button storyButton;
    public Button closeButton;
    public TMP_Text storyText;
    public GameObject storyPopup;

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

        
        StartCoroutine(AI_Manager.GetChatCompletion("Tell me a short story about any of this topics: fantasy, military, sports, comedy, or romance. Include a Title at the top and no other extra comments, just the story. Please avoid any religious, political or +18 commentary. In simple lenguage, use words that most people can understand and make it less than 500 words", response =>
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
