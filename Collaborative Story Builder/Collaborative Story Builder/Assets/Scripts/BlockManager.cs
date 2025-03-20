using Firebase.Firestore;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;



public class BlockManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private UserData user;
    private NotificationManager notification;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = User.GetUser();
        notification = GameObject.Find("Notification").GetComponent<NotificationManager>();
    }

    // Update is called once per frame
    public void OnBlockButtonPressed(string userID)
    {
        Debug.Log("Blocking user: " + userID);
        block_Player(userID);
    }


    private void block_Player(string userID)
    {
        DocumentReference blockedPeopleRef = db.Collection("Blocked").Document(user.UserID);

        blockedPeopleRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                // If the document exists, update the blockedPeople list
                blockedPeopleRef.UpdateAsync("blockedPeople", FieldValue.ArrayUnion(userID))
                    .ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsCompletedSuccessfully)
                        {
                            Debug.Log("User successfully blocked.");
                            notification.Notify("User blocked!", 3f);
                        }
                        else
                        {
                            Debug.LogError("Failed to block user: " + updateTask.Exception);
                        }
                    });
            }
            else
            {
                // If the document does not exist, create it with the first blocked user
                Dictionary<string, object> blockedData = new Dictionary<string, object>
                {
                    { "blockedPeople", new List<string> { userID } }
                };

                blockedPeopleRef.SetAsync(blockedData).ContinueWithOnMainThread(setTask =>
                {
                    if (setTask.IsCompletedSuccessfully)
                    {
                        Debug.Log("Blocked list created, user successfully blocked.");
                    }
                    else
                    {
                        Debug.LogError("Failed to create block list: " + setTask.Exception);
                    }
                });
            }
        });
    }
}
