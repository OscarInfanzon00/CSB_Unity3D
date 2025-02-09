using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
public class LobbyActivity : MonoBehaviour
{
    public GameObject createJoinMenu;
    public GameObject createLobbyMenu;

    public Button btnCreateLobby;
    public Button joinToLobbyButton;
    public Button startGameButton;

    public TMP_InputField roomNumberInput;
    public TextMeshProUGUI textRoomNumber;
    public TextMeshProUGUI txtPlayersNumber;
    public VerticalLayoutGroup listOfPlayersOnLobby;
    public TextMeshProUGUI logText;
    public GameObject playerConnected;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentRoomID;
    private string currentUserID;
    private string currentUsername;
    private bool isRoomCreator = false;
    private ListenerRegistration roomListener;

    void Start()
    {
        
        btnCreateLobby.onClick.AddListener(CreateLobby);
        joinToLobbyButton.onClick.AddListener(JoinToLobby);
        startGameButton.onClick.AddListener(StartGame);

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            currentUserID = auth.CurrentUser.UserId;

            if (PlayerPrefs.HasKey("SavedUsername"))
            {
                currentUsername = PlayerPrefs.GetString("SavedUsername");
            }
            else if (PlayerPrefs.HasKey("SavedEmail"))
            {
                currentUsername = PlayerPrefs.GetString("SavedEmail");
            }
        }
    }

    private void CreateLobby()
    {
        isRoomCreator = true;
        currentRoomID = GenerateRoomCode();

        Dictionary<string, object> roomData = new Dictionary<string, object>
        {
            { "roomID", currentRoomID },
            { "creatorID", currentUserID },
            { "players", new List<Dictionary<string, object>>() },
            { "isGameStarted", false }
        };

        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.SetAsync(roomData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                SetLogText("Room created successfully!");
                EnterRoom(currentRoomID);
            }
            else
            {
                SetLogText("Error creating room: " + task.Exception);
            }
        });
    }

    private void JoinToLobby()
    {
        string roomCode = roomNumberInput.text.Trim();
        if (string.IsNullOrEmpty(roomCode))
        {
            SetLogText("Please enter a valid Room Code.");
            return;
        }

        DocumentReference roomRef = db.Collection("Rooms").Document(roomCode);
        roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                SetLogText("Room found! Joining...");
                EnterRoom(roomCode);
            }
            else
            {
                SetLogText("Room not found.");
            }
        });
    }

    private void EnterRoom(string roomID)
    {
        createJoinMenu.SetActive(false);
        createLobbyMenu.SetActive(true);
        textRoomNumber.text = "Room Code: " + roomID;
        currentRoomID = roomID;

        AddPlayerToRoom();
        ListenForRoomUpdates();
    }

    private void AddPlayerToRoom()
    {
        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "userID", currentUserID },
                    { "username", currentUsername }
                };

                List<Dictionary<string, object>> players = task.Result.GetValue<List<Dictionary<string, object>>>("players");
                if (players == null)
                    players = new List<Dictionary<string, object>>();

                bool alreadyInRoom = players.Exists(p => p["userID"].ToString() == currentUserID);
                if (!alreadyInRoom)
                {
                    players.Add(userData);
                    roomRef.UpdateAsync(new Dictionary<string, object> { { "players", players } })
                        .ContinueWithOnMainThread(updateTask =>
                        {
                            if (updateTask.IsCompletedSuccessfully)
                                SetLogText("Joined room successfully.");
                            else
                                SetLogText("Error joining room: " + updateTask.Exception);
                        });
                }
                else
                {
                    SetLogText("You are already in the room.");
                }
            }
        });
    }

    private void ListenForRoomUpdates()
    {
        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomListener = roomRef.Listen(snapshot =>
        {
            if (snapshot.Exists)
            {
                List<Dictionary<string, object>> players = snapshot.GetValue<List<Dictionary<string, object>>>("players");
                bool isGameStarted = snapshot.GetValue<bool>("isGameStarted");

                UpdatePlayerList(players);
                txtPlayersNumber.text = "Players: " + players.Count;

                if (isGameStarted)
                {
                    LoadGameScene();
                }
            }
        });
    }

    private void UpdatePlayerList(List<Dictionary<string, object>> players)
    {
        foreach (Transform child in listOfPlayersOnLobby.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in players)
        {
            GameObject playerEntry = Instantiate(playerConnected, listOfPlayersOnLobby.transform);
            Text playerText = playerEntry.GetComponent<Text>();
            if (playerText != null)
            {
                playerText.text = player["username"].ToString();
            }
            else
            {
                Debug.LogError("playerConnected prefab does not have a Text component.");
            }
        }
    }

    private void StartGame()
    {
        if (!isRoomCreator)
        {
            SetLogText("Only the room creator can start the game.");
            return;
        }

        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.UpdateAsync("isGameStarted", true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                SetLogText("Game started!");
            }
            else
            {
                SetLogText("Error starting game: " + task.Exception);
            }
        });
    }

    private void LoadGameScene()
    {
        roomListener.Stop();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Story_Contribution");
    }

    private string GenerateRoomCode()
    {
        System.Random random = new System.Random();
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];

        for (int i = 0; i < 6; i++)
        {
            code[i] = characters[random.Next(characters.Length)];
        }

        return new string(code);
    }

    private void SetLogText(string message)
    {
        if (logText != null)
        {
            logText.text = message;
        }
    }
}