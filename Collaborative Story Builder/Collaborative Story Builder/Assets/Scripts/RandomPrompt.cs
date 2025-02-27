using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Compilation;


public class RandomPrompt : MonoBehaviour {
    public Button generateButton, closeButton;
    public TMP_Text outputText;
    public GameObject promptPopup, AigenMenu;

    private void Start()
    {
        if (generateButton != null) {
            generateButton.onClick.AddListener(GeneratePrompt);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(closePrompt);
        }
    }
    public void GeneratePrompt()
    {
        promptPopup.SetActive(true);
        AigenMenu.SetActive(!AigenMenu.activeSelf);
        StartCoroutine(AI_Manager.GetChatCompletion("Give me a short sentence for a story idea I could use to write.", response =>
        {
            Debug.Log("AI Response: " + response);
            if (outputText != null) {
                outputText.text = response;
            }
        }));
    }

    public void closePrompt()
    {
        promptPopup.SetActive(false);
    }
    
}