using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Firebase.Auth;
using System;

public class MultiplayerStoryManager : MonoBehaviour
{
    public TMP_InputField contributionInput;
    public Button submitContributionButton;
    public TextMeshProUGUI storyDisplay;
    public TextMeshProUGUI turnIndicator;
    private FirebaseFirestore db;
    private string currentRoomID;
    private string currentUserID;
    private string currentStoryID;
    private ListenerRegistration storyListener;
    private ListenerRegistration roomListener;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        currentRoomID = PlayerPrefs.GetString("room");
        currentUserID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        submitContributionButton.onClick.AddListener(SubmitContribution);

        ListenForRoomUpdates();

        if (IsRoomCreator())
        {
            SaveStory();
        }

        roomListener = null;
    }

    private void ListenForStoryUpdates()
    {
        if (string.IsNullOrEmpty(currentStoryID))
            return;
        DocumentReference storyRef = db.Collection("Stories").Document(currentStoryID);
        storyListener = storyRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                List<string> storyTexts = snapshot.GetValue<List<string>>("storyTexts");
                if (storyTexts != null)
                {
                    // Update the UI text element with the entire story (each line separated by a newline).
                    storyDisplay.text = string.Join("\n", storyTexts);
                }
            }
        });
    }

    private bool IsRoomCreator()
    {
        return PlayerPrefs.GetInt("isRoomCreator", 0) == 1;
    }

    public void SaveStory()
    {
        ListenForStoryUpdates();
        if (!IsRoomCreator())
        {
            Debug.LogError("Only the room owner can create the story.");
            return;
        }

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            return;
        }

        string userID = user.UserId;
        string storyID = Guid.NewGuid().ToString();
        currentStoryID = storyID;
        int wordCount = 0;
        List<string> storyTexts = new List<string> { };

        string username = User.GetUser().Username;
        List<string> usersID = new List<string> { userID };
        List<string> usersUsernames = new List<string> { username };

        Dictionary<string, object> storyData = new Dictionary<string, object>
    {
        { "storyID", storyID },
        { "roomID", currentRoomID },
        { "storyTexts", storyTexts },
        { "timestamp", Firebase.Firestore.Timestamp.GetCurrentTimestamp() },
        { "usernames", usersUsernames },
        { "users", usersID },
        { "wordCount", wordCount },
        { "ownerID", userID }
    };

        DocumentReference storyRef = db.Collection("Stories").Document(storyID);

        storyRef.SetAsync(storyData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Story saved successfully with ID: " + storyID);
                DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
                roomRef.UpdateAsync(new Dictionary<string, object> { { "currentStoryID", storyID } })
                    .ContinueWithOnMainThread(roomTask =>
                    {
                        if (roomTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("Room updated with story ID: " + storyID);
                        }
                        else
                        {
                            Debug.LogError("Error updating room with story ID: " + roomTask.Exception);
                        }
                    });
            }
            else
            {
                Debug.LogError("Error saving story: " + task.Exception);
            }
        });
    }

    public void SubmitContribution()
    {
        if (storyListener == null)
        {
            ListenForStoryUpdates();
        }
        Debug.Log("Try to submit contribution");
        string newContribution = contributionInput.text.Trim();
        if (string.IsNullOrEmpty(newContribution))
            return;

        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("Trying to open room document...");
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                Debug.Log("Room document retrieved successfully.");
                string currentTurn = task.Result.GetValue<string>("currentTurn");
                if (currentUserID != currentTurn)
                {
                    Debug.Log("Not your turn to contribute.");
                    return;
                }
                Debug.Log("It is my turn.");

                List<Dictionary<string, object>> players = task.Result.GetValue<List<Dictionary<string, object>>>("players");
                int currentIndex = players.FindIndex(p => p["userID"].ToString() == currentUserID);
                int nextIndex = (currentIndex + 1) % players.Count;
                string nextTurn = players[nextIndex]["userID"].ToString();

                int roundNumber = task.Result.GetValue<int>("roundNumber");
                if (nextIndex == 0)
                {
                    roundNumber++;
                }

                if (string.IsNullOrEmpty(currentStoryID))
                {
                    if (task.Result.ContainsField("currentStoryID"))
                    {
                        currentStoryID = task.Result.GetValue<string>("currentStoryID");
                        Debug.Log("Retrieved currentStoryID from room: " + currentStoryID);
                    }
                    else
                    {
                        Debug.LogError("No story ID found in room document.");
                        return;
                    }
                }

                DocumentReference storyRef = db.Collection("Stories").Document(currentStoryID);
                storyRef.GetSnapshotAsync().ContinueWithOnMainThread(storyTask =>
                {
                    if (storyTask.IsCompletedSuccessfully && storyTask.Result.Exists)
                    {
                        List<string> storyTexts = storyTask.Result.GetValue<List<string>>("storyTexts") ?? new List<string>();
                        storyTexts.Add(newContribution);

                        int totalWordCount = 0;
                        foreach (string line in storyTexts)
                        {
                            totalWordCount += CountWords(line);
                        }

                        List<string> storyUsers = storyTask.Result.ContainsField("users") ?
                            storyTask.Result.GetValue<List<string>>("users") : new List<string>();
                        List<string> storyUsernames = storyTask.Result.ContainsField("usernames") ?
                            storyTask.Result.GetValue<List<string>>("usernames") : new List<string>();

                        if (!storyUsers.Contains(currentUserID))
                        {
                            storyUsers.Add(currentUserID);
                            string currentUsername = User.GetUser().Username;
                            storyUsernames.Add(currentUsername);
                        }

                        Dictionary<string, object> storyUpdate = new Dictionary<string, object>
                        {
                            { "storyTexts", storyTexts },
                            { "wordCount", totalWordCount },
                            { "users", storyUsers },
                            { "usernames", storyUsernames }
                        };

                        storyRef.UpdateAsync(storyUpdate).ContinueWithOnMainThread(storyUpdateTask =>
                        {
                            if (storyUpdateTask.IsCompletedSuccessfully)
                            {
                                Debug.Log("Story updated successfully.");
                            }
                            else
                            {
                                Debug.LogError("Error updating story: " + storyUpdateTask.Exception);
                            }
                        });

                        Dictionary<string, object> roomUpdate = new Dictionary<string, object>
                        {
                        { "currentTurn", nextTurn },
                        { "roundNumber", roundNumber }
                        };

                        roomRef.UpdateAsync(roomUpdate).ContinueWithOnMainThread(roomUpdateTask =>
                        {
                            if (roomUpdateTask.IsCompletedSuccessfully)
                            {
                                contributionInput.text = "";
                                Debug.Log("Turn updated to: " + nextTurn);
                            }
                            else
                            {
                                Debug.LogError("Error updating room: " + roomUpdateTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("Error retrieving story document.");
                    }
                });
            }
            else
            {
                Debug.LogError("Error retrieving room document.");
            }
        });
    }


    private int CountWords(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        char[] delimiters = new char[] { ' ', '\r', '\n' };
        return text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    void ListenForRoomUpdates()
    {
        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomListener = roomRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                if (string.IsNullOrEmpty(currentStoryID) && snapshot.ContainsField("currentStoryID"))
                {
                    currentStoryID = snapshot.GetValue<string>("currentStoryID");
                    if (storyListener == null)
                    {
                        ListenForStoryUpdates();
                    }
                }

                string currentTurn = snapshot.GetValue<string>("currentTurn");
                int roundNumber = snapshot.GetValue<int>("roundNumber");

                if (currentUserID == currentTurn)
                {
                    turnIndicator.text = "Your Turn (Round " + roundNumber + ")";
                    contributionInput.interactable = true;
                    submitContributionButton.interactable = true;
                }
                else
                {
                    turnIndicator.text = "Waiting for other players...";
                    contributionInput.interactable = false;
                    submitContributionButton.interactable = false;
                }
            }
        });
    }

    void OnDestroy()
    {
        if (roomListener != null)
            roomListener.Stop();
        if (storyListener != null)
            storyListener.Stop();
    }
}
