using Firebase.Firestore;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;



public class ReportManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private UserData user;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = User.GetUser();
        report_Player("TEST", "TEST");
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void report_Player(string userID, string reason)
    {
        //You have to add the userID coming from the input of the method into a new space in the list blocked people.
        Dictionary<string, object> reportedPeople = new Dictionary<string, object>
        {
            { "userID", user.UserID},
            { "reportedPeople", new List<Dictionary<string, object>>()},
            { "reason", new List<Dictionary<string, object>>()}
        };

        DocumentReference reportedPeopleRefer = db.Collection("Reported").Document();
        reportedPeopleRefer.SetAsync(reportedPeople).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("it worked");
            }
            else
            {
                Debug.Log("it did not work");
            }
        });
    }
}

