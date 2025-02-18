using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndTurnScript : MonoBehaviour
{

    public TMP_InputField userInputField;  
    public GameObject popupPanel;             

    public void OnEndTurnButtonClicked()
    {
        string userInput = userInputField.text;
        
        if (!string.IsNullOrEmpty(userInput)) 
        {
            popupPanel.SetActive(true); 
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
    
}



