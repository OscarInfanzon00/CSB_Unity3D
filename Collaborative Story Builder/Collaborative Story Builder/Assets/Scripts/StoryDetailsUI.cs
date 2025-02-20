using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryDetailsUI : MonoBehaviour
{
    public Button closeButton;
    public static StoryDetailsUI Instance;
    public TMP_Text storyContent;
    public TMP_Text storyAuthors;
    public TMP_Text storyDate;
    public GameObject StoryViewerUI;
    public string storyID;

    private void Awake()
    {
        Instance = this;
        closeButton.onClick.AddListener(CloseDetails);
    }

    public void ShowStoryDetails(Story story, string storyID)
    {
        storyContent.text = string.Join("\n\n", story.storyTexts);
        storyAuthors.text = "Authors: " + string.Join(", ", story.users);
        storyDate.text = "Date: " + story.timestamp.ToString("MM/dd/yyyy HH:mm");

        this.storyID = storyID;
    }

    public void CloseDetails()
    {
        StoryViewerUI.SetActive(false);
    }
}
