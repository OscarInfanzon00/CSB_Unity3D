using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Linq;
using TMPro;

public class StoryDeletionManager : MonoBehaviour
{
    // References to UI elements
    public GameObject selectionModeButton;
    public GameObject cancelSelectionButton;
    public GameObject confirmDeleteButton;
    public GameObject deletePopupPanel;
    public Transform selectedStoriesContainer;
    public Transform storyListContainer;
    public GameObject noSelectedStoriesText;
    public Button closePopupButton;
    public Button finalDeleteButton;
    public GameObject storyCardPrefab;

    // References to existing components
    private StoryManager storyManager;
    private CurrentUserStoryManager crntUserStoryManager;
    private FirebaseFirestore db;
    public NotificationManager notification;

    // State tracking
    private bool selectionModeActive = false;
    private List<string> selectedStoryIDs = new List<string>();
    private List<StoryCardUI> selectedStoryCards = new List<StoryCardUI>();

    void Start()
    {
        storyManager = GetComponent<StoryManager>();
        crntUserStoryManager = GetComponent<CurrentUserStoryManager>();
        db = FirebaseFirestore.DefaultInstance;

        // Initialize notification manager if not set in inspector
        if (notification == null)
        {
            notification = GameObject.Find("Notification").GetComponent<NotificationManager>();
        }

        // Set up button listeners
        selectionModeButton.GetComponent<Button>().onClick.AddListener(ToggleSelectionMode);
        cancelSelectionButton.GetComponent<Button>().onClick.AddListener(ExitSelectionMode);
        confirmDeleteButton.GetComponent<Button>().onClick.AddListener(ShowDeleteConfirmation);
        closePopupButton.onClick.AddListener(HideDeleteConfirmation);
        finalDeleteButton.onClick.AddListener(DeleteSelectedStories);

        // Initialize UI state
        cancelSelectionButton.SetActive(false);
        confirmDeleteButton.SetActive(false);
        deletePopupPanel.SetActive(false);
    }

    public void ToggleSelectionMode()
    {
        selectionModeActive = true;

        // Update UI elements
        selectionModeButton.SetActive(false);
        cancelSelectionButton.SetActive(true);
        confirmDeleteButton.SetActive(true);

        // Update story cards to selection mode
        foreach (Transform child in storyListContainer)
        {
            StoryCardUI cardUI = child.GetComponent<StoryCardUI>();
            if (cardUI != null)
            {
                // Disable normal click behavior
                cardUI.openDetailsButton.onClick.RemoveAllListeners();

                // Add selection behavior
                cardUI.openDetailsButton.onClick.AddListener(() => ToggleStorySelection(cardUI));

                // Show selection indicator
                cardUI.EnableSelectionMode(true);
            }
        }
    }

    public void ExitSelectionMode()
    {
        selectionModeActive = false;

        // Update UI elements
        selectionModeButton.SetActive(true);
        cancelSelectionButton.SetActive(false);
        confirmDeleteButton.SetActive(false);

        // Reset story cards to normal mode
        foreach (Transform child in storyListContainer)
        {
            StoryCardUI cardUI = child.GetComponent<StoryCardUI>();
            if (cardUI != null)
            {
                // Reset to normal click behavior
                cardUI.openDetailsButton.onClick.RemoveAllListeners();
                cardUI.openDetailsButton.onClick.AddListener(cardUI.OnStoryClick);

                // Hide selection indicator
                cardUI.EnableSelectionMode(false);
                cardUI.SetSelected(false);
            }
        }

        // Clear selection
        selectedStoryIDs.Clear();
        selectedStoryCards.Clear();
    }

    private void ToggleStorySelection(StoryCardUI cardUI)
    {
        if (selectedStoryIDs.Contains(cardUI.storyID))
        {
            // Deselect story
            selectedStoryIDs.Remove(cardUI.storyID);
            selectedStoryCards.Remove(cardUI);
            cardUI.SetSelected(false);
        }
        else
        {
            // Select story
            selectedStoryIDs.Add(cardUI.storyID);
            selectedStoryCards.Add(cardUI);
            cardUI.SetSelected(true);
        }

        // Update the delete button state
        confirmDeleteButton.GetComponent<Button>().interactable = selectedStoryIDs.Count > 0;
    }

    private void ShowDeleteConfirmation()
    {
        if (selectedStoryIDs.Count == 0)
        {
            notification.Notify("No stories selected for deletion.");
            return;
        }

        deletePopupPanel.SetActive(true);

        // Clear previous entries in selected stories container
        foreach (Transform child in selectedStoriesContainer)
        {
            Destroy(child.gameObject);
        }

        // Show selected stories or message if none selected
        if (selectedStoryIDs.Count > 0)
        {
            noSelectedStoriesText.SetActive(false);

            foreach (StoryCardUI cardUI in selectedStoryCards)
            {
                if (storyCardPrefab != null)
                {
                    GameObject cardCopy = Instantiate(storyCardPrefab, selectedStoriesContainer);
                    StoryCardUI copyCardUI = cardCopy.GetComponent<StoryCardUI>();

                    copyCardUI.SetStoryInfo(cardUI.GetStoryData());
                    RectTransform rectTransform = cardCopy.GetComponent<RectTransform>();
                    rectTransform.localScale = Vector3.one;
                    copyCardUI.EnableSelectionMode(false);
                    copyCardUI.openDetailsButton.onClick.RemoveAllListeners();
                }
            }
        }
        else
        {
            noSelectedStoriesText.SetActive(true);
        }
    }

    private void HideDeleteConfirmation()
    {
        deletePopupPanel.SetActive(false);
    }

    private void DeleteSelectedStories()
    {
        int totalToDelete = selectedStoryIDs.Count;
        int deletedCount = 0;

        UpdateLocalDataModel();

        foreach (string storyID in selectedStoryIDs)
        {
            // Fixed Firebase query to properly handle the query results
            Query query = db.Collection("Stories").WhereEqualTo("storyID", storyID);
            query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    QuerySnapshot querySnapshot = task.Result;

                    if (querySnapshot.Count > 0)
                    {
                        // Get the first document that matches our query
                        DocumentSnapshot document = querySnapshot.Documents.FirstOrDefault();

                        if (document != null)
                        {
                            document.Reference.DeleteAsync().ContinueWithOnMainThread(deleteTask =>
                            {
                                deletedCount++;

                                if (deleteTask.IsCompletedSuccessfully)
                                {
                                    Debug.Log("Successfully deleted story: " + storyID);

                                    // If all stories deleted, finish up
                                    if (deletedCount == totalToDelete)
                                    {
                                        FinishDeletion(totalToDelete);
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Error deleting story: " + deleteTask.Exception);

                                    // If all deletion attempts completed
                                    if (deletedCount == totalToDelete)
                                    {
                                        notification.Notify("Error deleting some stories. The UI has been updated.");
                                        HideDeleteConfirmation();
                                        ExitSelectionMode();
                                    }
                                }
                            });
                        }
                        else
                        {
                            Debug.LogWarning("No document found with storyID: " + storyID);
                            deletedCount++;
                            CheckIfAllDeletionsCompleted(deletedCount, totalToDelete);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No document found with storyID: " + storyID);
                        deletedCount++;
                        CheckIfAllDeletionsCompleted(deletedCount, totalToDelete);
                    }
                }
                else
                {
                    Debug.LogError("Error finding story to delete: " + task.Exception);
                    deletedCount++;
                    CheckIfAllDeletionsCompleted(deletedCount, totalToDelete);
                }
            });
        }
    }
    private void UpdateLocalDataModel()
    {
        // Update StoryManager internal data model
        if (storyManager != null)
        {
            foreach (string storyID in selectedStoryIDs)
            {
                storyManager.RemoveStoryByID(storyID);
            }
        }

        // Remove the UI elements
        foreach (StoryCardUI card in selectedStoryCards)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
    }

    private void CheckIfAllDeletionsCompleted(int deletedCount, int totalToDelete)
    {
        // If all deletion attempts completed
        if (deletedCount == totalToDelete)
        {
            FinishDeletion(totalToDelete);
        }
    }

    private void FinishDeletion(int totalDeleted)
    {
        notification.Notify($"Successfully deleted {totalDeleted} stories!");
        HideDeleteConfirmation();
        ExitSelectionMode();

        // Refresh CurrentUserStoryManager's UI if available
        if (crntUserStoryManager != null)
        {
            crntUserStoryManager.LoadUserStories();
        }
    }
}