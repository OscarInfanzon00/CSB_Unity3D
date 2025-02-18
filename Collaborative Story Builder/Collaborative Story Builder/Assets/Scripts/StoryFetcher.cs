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

        storyPopup.SetActive(false); // Hide pop-up initially
    }

    public void GetStory()
    {
        // Set placeholder text before fetching the story
        storyText.text = placeholderText;
        storyPopup.SetActive(true); // Show the pop-up while fetching

        // FIX: Removed third argument, only passing prompt and response callback
        StartCoroutine(AI_Manager.GetChatCompletion("Tell me a short fantasy story.", response =>
        {
            if (!string.IsNullOrEmpty(response)) // Check if response is valid
            {
                Debug.Log("AI Response: " + response);
                storyText.text = response;
            }
            else
            {
                Debug.LogError("Empty AI Response.");
                storyText.text = errorText; // Show error message
            }
        }));
    }

    public void CloseStoryPopup()
    {
        storyPopup.SetActive(false);
        storyText.text = placeholderText; // Reset text when closing
    }
}
