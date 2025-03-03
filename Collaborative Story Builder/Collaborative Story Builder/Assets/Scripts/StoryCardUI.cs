using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryCardUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text authorText;
    public Button openDetailsButton;
    public GameObject StoryViewerUI;
    private Story storyData;
    public string storyID;

    public void SetStoryInfo(Story story)
    {
        storyData = story;
        titleText.text = story.storyID;
        authorText.text = "Authors: " + string.Join(", ", story.users);
        openDetailsButton.onClick.AddListener(OnStoryClick);
    }

    public void OnStoryClick()
    {
        StoryViewerUI.SetActive(true);
        StoryViewerUI.GetComponent<StoryDetailsUI>().ShowStoryDetails(storyData, storyID);
    }
}
