using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;

public class InvitationNotification : MonoBehaviour
{
    public GameObject invitationPanel;
    public TMP_Text invitationText;
    public Button acceptButton;
    public Button rejectButton;

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private string currentUserId;
    private string inviterId;

    private ListenerRegistration inviteListener; 

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            currentUserId = auth.CurrentUser.UserId;
            StartListeningForInvites(); 
        }

        acceptButton.onClick.AddListener(AcceptInvitation);
        rejectButton.onClick.AddListener(RejectInvitation);

        
        invitationPanel.SetActive(false);
    }

    private void StartListeningForInvites()
    {
        
        inviteListener = db.Collection("UserInvitations").Document(currentUserId)
            .Listen(snapshot =>
            {
                if (snapshot.Exists)
                {
                    inviterId = snapshot.GetValue<string>("inviterId");

                    
                    db.Collection("Users").Document(inviterId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompletedSuccessfully)
                        {
                            DocumentSnapshot userSnapshot = task.Result;
                            string inviterName = userSnapshot.ContainsField("username") ? userSnapshot.GetValue<string>("username") : "Unknown User";

                            ShowInvitation(inviterId, inviterName);
                        }
                    });
                }
                else
                {
                    invitationPanel.SetActive(false); 
                }
            });
    }

    private void ShowInvitation(string inviterId, string inviterName)
    {
        invitationText.text = $"You have been invited by {inviterName}";
        invitationPanel.SetActive(true);
    }

    private void AcceptInvitation()
    {
        db.Collection("UserInvitations").Document(currentUserId).DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Invitation accepted!");
                invitationPanel.SetActive(false);
            }
        });
    }

    private void RejectInvitation()
    {
        db.Collection("UserInvitations").Document(currentUserId).DeleteAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("Invitation rejected.");
                invitationPanel.SetActive(false);
            }
        });
    }

    private void OnDestroy()
    {
        
        if (inviteListener != null)
        {
            inviteListener.Stop();
        }
    }
}
