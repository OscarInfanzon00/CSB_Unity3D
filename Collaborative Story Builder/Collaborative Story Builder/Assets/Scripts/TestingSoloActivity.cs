using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndTurnScript : MonoBehaviour
{

    //public Button buttonEndTurn;


    public TMP_InputField userInputField;  // Input field reference
    public GameObject popupPanel;          // Pop-up panel reference
    public TextMeshProUGUI popupText;      // Text inside pop-up

    // Function to handle the button click
    public void OnEndTurnButtonClicked()
    {
        string userInput = userInputField.text; //gets user input
        
        if (!string.IsNullOrEmpty(userInput)) //ensures that text is entered
        {
            popupText.text = userInput;
            popupPanel.SetActive(true); //this part will reveal the pop-up
        }
    }

    //this function will close the pop-up
    public void ClosePopup()
    {
        popupPanel.SetActive(false); //hides the pop-up
    }


    //public void 

    
}



