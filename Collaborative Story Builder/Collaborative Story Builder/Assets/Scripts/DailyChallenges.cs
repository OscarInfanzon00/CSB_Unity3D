using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;



public class PanelController : MonoBehaviour
{

    public List<string> dailyChallenges = new List<string> { 
        "Log-in today for the first time",
        "Participate in a story", 
        "Add a friend",
        "Write a story in the testing room" };


    public GameObject panel;

    public TMP_Text challengeText1;
    public TMP_Text challengeText2;
    public TMP_Text challengeText3;


    void Start()
    {
        AssignChallenges();

    }

    public void TogglePanel()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    void AssignChallenges()
        {
            List<string> shuffledChallenges = new List<string>(dailyChallenges);
            for (int i = 0; i < shuffledChallenges.Count; i++)
            {
                int randomIndex = Random.Range(i, shuffledChallenges.Count);
                string temp = shuffledChallenges[i];
                shuffledChallenges[i] = shuffledChallenges[randomIndex];
                shuffledChallenges[randomIndex] = temp;
            }

            challengeText1.text = shuffledChallenges[0];
            challengeText2.text = shuffledChallenges[1];
            challengeText3.text = shuffledChallenges[2];
    }


}
