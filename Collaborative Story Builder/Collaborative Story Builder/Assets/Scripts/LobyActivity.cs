using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Threading.Tasks;
public class LobbyActivity : MonoBehaviour
{
    public GameObject LobbyActivityUI;
    public GameObject MainMenuActivity;
    public Button closeMenuButton1;
    public Button closeMenuButton2;
    public GameObject createJoinMenu;
    public GameObject createLobbyMenu;
    public Button quickRejoinButton;
    public Button btnCreateLobby;
    public Button joinToLobbyButton;
    public Button startGameButton;
    public Button joinRandomButton;

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
    private UserData user;

    public NotificationManager notificationManager;

    void Start()
    {
        closeMenuButton1.onClick.AddListener(closeMenu);
        closeMenuButton2.onClick.AddListener(closeMenu);
        btnCreateLobby.onClick.AddListener(CreateLobby);
        joinToLobbyButton.onClick.AddListener(JoinToLobby);
        startGameButton.onClick.AddListener(StartGame);

        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        user = User.GetUser();

        if (quickRejoinButton != null)
        {
            quickRejoinButton.onClick.AddListener(QuickRejoin);
        }
        if (joinRandomButton != null)
        {
            joinRandomButton.onClick.AddListener(JoinRandomRoom);
        }
        if (PlayerPrefs.HasKey("LastRoomID"))
        {
            currentRoomID = PlayerPrefs.GetString("LastRoomID");
        }

        if (auth.CurrentUser != null)
        {
            currentUserID = auth.CurrentUser.UserId;

            if (user.Username != "defaultUser")
            {
                currentUsername = user.Username;
            }
            else if (user.Email != "defaultEmail")
            {
                currentUsername = user.Email;
            }
        }
        PlayerPrefs.SetInt("isRoomCreator", 0);
    }

    private void closeMenu()
    {
        MainMenuActivity.SetActive(true);
        LobbyActivityUI.SetActive(false);
    }

    private void QuickRejoin()
    {
        if (string.IsNullOrEmpty(currentRoomID))
        {
            SetLogText("No previous room found.");
            return;
        }

        SetLogText("Attempting to rejoin room: " + currentRoomID);

        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                SetLogText("Room found! Rejoining...");
                EnterRoom(currentRoomID);
            }
            else
            {
                SetLogText("Room not found or no longer available.");
            }
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                SetLogText("Internal error occurred while rejoining room.");
            }
        });
    }


    private void CreateLobby()
    {
        SetLogText("Creating Lobby...");
        isRoomCreator = true;
        currentRoomID = GenerateRoomCode();
        SetLogText("Generating Room Code...");

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
    private void JoinRandomRoom()
    {
        db.Collection("Rooms").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if(task.IsCompletedSuccessfully)
            {
                List<string> roomIDs = new List<string>();
                foreach (var document in task.Result.Documents)
                {
                    string creatorID = document.GetValue<string>("creatorID");
                    if (!string.IsNullOrEmpty(creatorID) && creatorID != currentUserID)
                    {
                        roomIDs.Add(document.Id);
                    }
                }
                if (roomIDs.Count == 0)
                {
                    SetLogText("No available rooms found.");
                    return;
                }
                System.Random random = new System.Random();

                string randomRoomID = roomIDs[random.Next(roomIDs.Count)];
                SetLogText("Random room selected: " + randomRoomID);
                EnterRoom(randomRoomID);
            }
            else
            {
                SetLogText("Error fetching rooms: " + task.Exception);
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

    public void JoinRoomForFriend(string roomID){
        EnterRoom(roomID);
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
                            {
                                SetLogText("Joined room successfully.");
                                PlayerPrefs.SetString("room", currentRoomID);
                            }
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

        PlayerPrefs.SetInt("isRoomCreator", 1);
        DocumentReference roomRef = db.Collection("Rooms").Document(currentRoomID);
        roomRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                List<Dictionary<string, object>> players = task.Result.GetValue<List<Dictionary<string, object>>>("players");
                if (players == null || players.Count == 0)
                {
                    SetLogText("No players in the room to start the game.");
                    return;
                }
                // For example, set the first player as the first turn.
                string firstPlayerID = players[0]["userID"].ToString();

                // Update the room with additional game data
                Dictionary<string, object> updateData = new Dictionary<string, object>
                {
                {"isGameStarted", true},
                {"currentTurn", firstPlayerID},
                {"roundNumber", 1}
                };

                roomRef.UpdateAsync(updateData).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompletedSuccessfully)
                    {
                        SetLogText("Game started!");
                    }
                    else
                    {
                        SetLogText("Error starting game: " + updateTask.Exception);
                    }
                });
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

    public void CopyRoomID()
    {
        GUIUtility.systemCopyBuffer = "Hey, I created a room in CSB. Wanna play? Room ID: " + currentRoomID;
        Debug.Log("Room ID copied to clipboard: " + currentRoomID);
        notificationManager.Notify("Room ID copied to clipboard.");
    }
}