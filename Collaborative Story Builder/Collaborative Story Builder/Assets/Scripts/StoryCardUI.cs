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
    public GameObject friendTag;
    public GameObject summarizePanel;
    public TextMeshProUGUI summaryText;
    private Story storyData;
    public string storyID;
    public NotificationManager notification = null;


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

    public void activateFriends()
    {
        friendTag.SetActive(true);
    }

    public void summarizeStory()
    {
        if (summarizePanel.activeSelf)
        {
            summarizePanel.SetActive(false);
        }
        else
        {
            summarizePanel.SetActive(true);

            string combinedStoryText = string.Join("\n\n", storyData.storyTexts);

            StartCoroutine(AI_Manager.GetChatCompletion("Summarize this story a few words, what is it about?: " + combinedStoryText, response =>
            {
                summaryText.text = response.ToString();
            }));
        }
    }

    public void saveBookmark()
    {
        if(notification==null){
            notification = GameObject.Find("Notification").GetComponent<NotificationManager>();
        }
        string bookMarkList = PlayerPrefs.GetString("SavedBookMarkList");

        string finalBookList;

        if (bookMarkList.Length > 0)
        {
            finalBookList = bookMarkList + "," + storyID;
        }
        else
        {
            finalBookList = storyID;
        }

        PlayerPrefs.SetString("SavedBookMarkList", finalBookList);
        Debug.Log("Story bookmarked! "+ storyID);
        notification.Notify("Story bookmarked!");
    }

}
