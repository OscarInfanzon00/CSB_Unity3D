using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReportManager : MonoBehaviour
{
    private FirebaseFirestore db;
    private UserData user;
    private string reportedUserID = ""; 

    public GameObject reportPopup; // Assign in Unity (this exists outside the prefab)
    public TMP_Dropdown reasonDropdown;
    public TMP_InputField detailsInputField;
    public Button submitReportButton, closeReportButton;

    private readonly List<string> reportReasons = new List<string>
    {
        "Harassment",
        "Inappropriate Content",
        "Cheating/Exploiting",
        "Spam",
        "Other"
    };

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        user = User.GetUser();
        InitializeDropdown();

        closeReportButton.onClick.AddListener(CloseReportPopup);
        submitReportButton.onClick.AddListener(SubmitReport);
    }

    private void InitializeDropdown()
    {
        if (reasonDropdown != null)
        {
            reasonDropdown.ClearOptions();
            reasonDropdown.AddOptions(reportReasons);
        }
    }

    public void OpenReportPopup(string userID)
    {
        if (string.IsNullOrEmpty(userID))
        {
            Debug.LogError("Invalid user ID for reporting.");
            return;
        }

        reportedUserID = userID;
        reportPopup.SetActive(true); // Show the popup

        reportPopup.transform.SetAsLastSibling();
        Debug.Log($"Report popup opened for user: {userID}");
    }

    private void CloseReportPopup()
    {
        reportedUserID = ""; // Reset the user
        reportPopup.SetActive(false);
    }

    private void SubmitReport()
    {
        if (string.IsNullOrEmpty(reportedUserID))
        {
            Debug.LogError("No user selected for reporting.");
            return;
        }

        string selectedReason = reportReasons[reasonDropdown.value];
        string details = detailsInputField.text;

        DocumentReference reportDocRef = db.Collection("Reports").Document(reportedUserID);
        CollectionReference userReportsRef = reportDocRef.Collection("UserReports");

        Dictionary<string, object> newReport = new Dictionary<string, object>
        {
            { "reportedBy", user.UserID },
            { "reason", selectedReason },
            { "details", details },
            { "timestamp", FieldValue.ServerTimestamp }
        };

        userReportsRef.AddAsync(newReport).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Report successfully submitted.");
                CloseReportPopup();
            }
            else
            {
                Debug.LogError("Failed to submit report: " + task.Exception);
            }
        });
    }
}



