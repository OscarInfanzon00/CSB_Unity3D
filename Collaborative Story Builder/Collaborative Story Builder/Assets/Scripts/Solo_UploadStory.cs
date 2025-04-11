using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Solo_UploadStory : MonoBehaviour
{
    private UserData userData;
    public TMP_InputField storyInputField;
    public TMP_InputField bannedWordInputField;
    public Transform bannedWordsListContent;
    public GameObject bannedWordItemPrefab;
    public TMP_InputField requiredWordInputField;
    public Transform requiredWordsListContent;
    private List<string> currentStoryRequiredWords = new List<string>();
    private List<string> currentStoryBannedWords = new List<string>();
    private FirebaseFirestore db;
    public NotificationManager notificationManager;
    public Button closeButton;
    public AudioSource victory;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        userData = User.GetUser();
        currentStoryBannedWords.Clear();
        UpdateBannedWordsUI();
        closeButton.onClick.AddListener(closeScene);
    }

    public void SaveStory()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            return;
        }

        string userID = user.UserId;
        string storyID = Guid.NewGuid().ToString();

        string storyText = storyInputField.text;
        string foundBannedWord = CheckForBannedWords(storyText);
        if (!string.IsNullOrEmpty(foundBannedWord))
        {
            notificationManager.Notify($"Your story contains a banned word: '{foundBannedWord}'.", 3f);
            return;
        }
        if(!ContainsAllRequiredWords(storyText))
        {
            return;
        }
        int wordCount = CountWords(storyText);
        List<string> storyTexts = new List<string>
    {
        storyInputField.text
    };

        List<string> usersID = new List<string> { userID };
        List<string> usersUsernames = new List<string> { userData.Username };

        Dictionary<string, object> storyData = new Dictionary<string, object>
    {
        { "storyID", storyID },
        { "storyTexts", storyTexts },
        { "timestamp", Firebase.Firestore.Timestamp.GetCurrentTimestamp() },
        { "usernames", usersUsernames },
        { "users", usersID },
        { "wordCount", wordCount }
    };

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                notificationManager.Notify($"Your story was successfully uploaded!\nWord count: {wordCount}", 3f);
                Debug.Log("Story saved successfully with ID: " + storyID);
                if (victory != null) {
                    victory.Play();
                }
                LevelSystem.AddXP(50);
                AddWordsToUser(user.UserId, wordCount);
            }
            else
            {
                Debug.LogError("Error saving story: " + task.Exception);
            }
        });
    }
    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public void AddWordsToUser(string userId, int newWords)
    {
        DocumentReference userRef = db.Collection("Users").Document(userId);

        userRef.UpdateAsync("words", FieldValue.Increment(newWords)).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log($"Successfully added {newWords} words to user {userId}.");
            }
            else
            {
                Debug.LogError("Failed to update words: " + task.Exception);
            }
        });
    }
     private string CheckForBannedWords(string text)
    {
        foreach (string word in currentStoryBannedWords)
        {
            if (text.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                return word;
            }
        }
        return null;
    }

    public void AddBannedWord()
    {
        string bannedWord = bannedWordInputField.text.Trim().ToLower();

        if (string.IsNullOrEmpty(bannedWord))
        {
            notificationManager.Notify("Please enter a word to ban.", 2f);
            return;
        }

        if (!currentStoryBannedWords.Contains(bannedWord))
        {
            currentStoryBannedWords.Add(bannedWord);
            notificationManager.Notify($"'{bannedWord}' has been added to the banned list.", 2f);
            bannedWordInputField.text = "";
            UpdateBannedWordsUI();
        }
        else
        {
            notificationManager.Notify($"'{bannedWord}' is already in the banned list.", 2f);
        }
    }

    public void RemoveBannedWord(string word)
    {
        if (currentStoryBannedWords.Contains(word))
        {
            currentStoryBannedWords.Remove(word);
            notificationManager.Notify($"'{word}' has been removed from the banned list.", 2f);
            UpdateBannedWordsUI();
        }
    }

    private void UpdateBannedWordsUI()
    {
        foreach (Transform child in bannedWordsListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string word in currentStoryBannedWords)
        {
            GameObject item = Instantiate(bannedWordItemPrefab, bannedWordsListContent);
            item.GetComponentInChildren<TMP_Text>().text = word;
            item.GetComponentInChildren<Button>().onClick.AddListener(() => RemoveBannedWord(word));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(bannedWordsListContent.GetComponent<RectTransform>());

    }

    public void ClearBannedWordsForNewStory()
    {
        currentStoryBannedWords.Clear();
        UpdateBannedWordsUI();
    }
    public void AddRequiredWord() {
        string newWord = requiredWordInputField.text.Trim().ToLower();
        Debug.Log("Attempting to add required word: " + newWord);
        if (string.IsNullOrEmpty(newWord))
        {
            notificationManager.Notify("Please enter a required word.", 3f);
            return;
        }

        if (currentStoryRequiredWords.Contains(newWord))
        {
            notificationManager.Notify("Word is already in the required list.", 3f);
            return;
        }

        currentStoryRequiredWords.Add(newWord);
        requiredWordInputField.text = "";
        UpdateRequiredWordsUI();
    }
    public void RemoveRequiredWord(string word)
    {
        currentStoryRequiredWords.Remove(word);
        UpdateRequiredWordsUI();
    }
    private void  UpdateRequiredWordsUI()
    {
        foreach (Transform child in requiredWordsListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string word in currentStoryRequiredWords)
        {
            GameObject item = Instantiate(bannedWordItemPrefab, requiredWordsListContent);
            item.GetComponentInChildren<TMP_Text>().text = word;
            item.GetComponentInChildren<Button>().onClick.AddListener(() => RemoveRequiredWord(word));
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(requiredWordsListContent.GetComponent<RectTransform>());
    }
    private bool ContainsAllRequiredWords(string storyText) {
        foreach (string word in currentStoryRequiredWords)
        {
            if (!storyText.ToLower().Contains(word))
            {
                notificationManager.Notify($"Your story must include the word: '{word}'", 3f);
                return false;
            }
        }
        return true;
    }


    public void clearText()
    {
        storyInputField.text = " ";
    }

    void Update()
    {

    }

    public void closeScene()
    {
        SceneManager.LoadScene("Main_Menu");
    }
}

