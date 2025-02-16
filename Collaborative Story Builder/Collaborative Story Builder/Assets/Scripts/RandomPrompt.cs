using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Compilation;


public class RandomPrompt : MonoBehaviour {
    public Button generateButton;
    public TMP_Text outputText;

    private void Start()
    {
        if (generateButton != null) {
            generateButton.onClick.AddListener(GeneratePrompt);
        }
    }
    public void GeneratePrompt()
    {
        StartCoroutine(AI_Manager.GetChatCompletion("Give me a short sentence for a story idea I could use to write.", response =>
        {
            Debug.Log("AI Response: " + response);
            if (outputText != null) {
                outputText.text = response;
            }
        }));
    }
    
}