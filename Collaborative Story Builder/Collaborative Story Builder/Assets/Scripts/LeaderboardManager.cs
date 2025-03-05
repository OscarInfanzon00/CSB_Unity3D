using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using Microsoft.Unity.VisualStudio.Editor;

public class LeaderboardManager : MonoBehaviour
{
    private FirebaseFirestore db;
    public GameObject leaderboardElement;
    public Transform contentHolder;
    public GameObject leaderboard;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadLeaderboard();
    }

    void LoadLeaderboard()
    {
        db.Collection("Users").OrderByDescending("words").Limit(10).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                int rank = 1;
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    string username = doc.ContainsField("username") ? doc.GetValue<string>("username") : "Unknown";
                    int words = doc.ContainsField("words") ? doc.GetValue<int>("words") : 0;

                    GameObject newElement = Instantiate(leaderboardElement, contentHolder);
                    newElement.GetComponent<LeaderboardItem>().username.text = rank.ToString() + ". " + username;
                    newElement.GetComponent<LeaderboardItem>().words.text = words.ToString();
                    rank++;
                }
            }
            else
            {
                Debug.LogError("Failed to load leaderboard: " + task.Exception);
            }
        });
    }

    public void activateOrdeactivate(){
        if(leaderboard.active)
            leaderboard.SetActive(false);
        else leaderboard.SetActive(true);
    }
}
