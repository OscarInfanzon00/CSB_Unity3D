using UnityEngine;
using UnityEngine.UI;

public class EndTurnScript : MonoBehaviour
{
    public InputField inputField;
    public GameObject popupPanel;
    public Text popupText;

    // variables above are references to elements in the Pop-up

    // This function will be called when the "End Turn" button is clicked
    public void OnEndTurnButtonClicked()
    {
        //gets the text entered by the user
        string userInput = inputField.text;

        //sets the congratulatory message with the user's input
        popupText.text = "Congratulations! You wrote: " + userInput;

        //Makes pop-up panel pop up (it is invisible by default)
        popupPanel.SetActive(true);
    }
}