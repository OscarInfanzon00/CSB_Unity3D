using UnityEngine;
using Firebase.Firestore;
using System;
using System.Collections.Generic;

public class ReportSystem : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public void ReportUser(string reportedUserId, string reporterUserId, string reason, string additionalDetails)
    {
        // Create report data
        Dictionary<string, object> report = new Dictionary<string, object>
        {
            { "reportedUserId", reportedUserId },
            { "reporterUserId", reporterUserId },
            { "reason", reason },
            { "additionalDetails", additionalDetails },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        // Save to Firestore
        db.Collection("reports").AddAsync(report).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Report submitted successfully.");
            }
            else
            {
                Debug.LogError("Failed to submit report: " + task.Exception);
            }
        });
    }
}

