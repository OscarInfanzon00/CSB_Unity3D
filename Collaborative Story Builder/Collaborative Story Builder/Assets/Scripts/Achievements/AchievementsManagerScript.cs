using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class AchievementsManagerScript : MonoBehaviour
{
    private FirebaseFirestore db;
    public List<GameObject> achievementPrefabs;
    public Transform gridParent;
    public GameObject AchievementPanel;
    public int lastAchievementID;
    void Start()
    {
        InstantiateAchievementGrid();
    }
    public void InstantiateAchievementGrid()
    {
        for(int i = 0; i < achievementPrefabs.Count; i++)
        {
            GameObject achievement = Instantiate(achievementPrefabs[i], gridParent);
            //achievement.GetComponentInChildren<TMP_Text>().text = "Achievement " + (i + 1);
        }
    }
    public void SwitchAchievementPanel()
    {
        switch (AchievementPanel.activeInHierarchy)
        {
            case true:
                AchievementPanel.SetActive(false);
                break;
            case false:
                AchievementPanel.SetActive(true);
                break;
        }
    }
    public void ObtainAchievement()
    {
        //switch(lastAchievementID)
    }
    
    public void AddAchievementToDatabase()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogError("No user is currently signed in.");
            return;
        }

        string userID = user.UserId;
        string achievementID = lastAchievementID.ToString(); // achievementID;

        //if (string.IsNullOrEmpty(commentText))
        //{
        //    Debug.LogError("Comment cannot be empty.");
        //    return;
        //}

        DocumentReference storyRef = db.Collection("Users").Document(userID);

        storyRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    List<string> achievements = snapshot.ContainsField("achievements")
                        ? snapshot.GetValue<List<string>>("achievements")
                        : new List<string>();



                    achievements.Add(achievementID);

                    storyRef.UpdateAsync("achievements", achievements).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("Achievements added successfully.");

                        }
                        else
                        {
                            Debug.LogError("Error adding achievements: " + updateTask.Exception);
                        }
                    });
                }
                else
                {
                    Debug.LogError("Story does not exist.");
                }
            }
            else
            {
                Debug.LogError("Error retrieving story: " + task.Exception);
            }
        });
    }
}
