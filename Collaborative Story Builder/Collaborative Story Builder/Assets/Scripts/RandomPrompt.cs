using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomPrompt : MonoBehaviour
{
    public Button generateButton, closeButton;
    public TMP_Text outputText;
    public GameObject promptPopup, AigenMenu;
    public NotificationManager notification;

    private void Start()
    {
        if (generateButton != null)
        {
            generateButton.onClick.AddListener(GeneratePrompt);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePrompt);
        }
    }

    public void GeneratePrompt()
    {
        promptPopup.SetActive(true);
        AigenMenu.SetActive(!AigenMenu.activeSelf);

        
        if (outputText != null)
        {
            outputText.text = "Getting your AI-generated prompt ready...";
        }

        StartCoroutine(AI_Manager.GetChatCompletion("Give me a short sentence for a story idea I could use to write about something random every time", response =>
        {
            Debug.Log("AI Response: " + response);
            if (outputText != null)
            {
                outputText.text = response;
                notification.Notify("Your AI prompt was succesfully generated. Are you planning to write about it?", 3f);
            }
        }));
    }

    public void ClosePrompt()
    {
        promptPopup.SetActive(false);

        if (outputText != null)
        {
            outputText.text = "Your AI prompt goes here!";
        }
    }
}
