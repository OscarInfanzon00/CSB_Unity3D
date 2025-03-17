using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class AchievementClass : MonoBehaviour
{
    public bool Locked = true;
    public string Name, Description, ID, DateOccured;
    public GameObject InfoPanel;
    public TextMeshProUGUI NameTMP, DescriptionTMP;
    private EventSystem eventSystem;
    private GraphicRaycaster graphicRaycaster;
    public GameObject Background, Image;
    void Start()
    {
        // Find the EventSystem in the scene
        eventSystem = FindFirstObjectByType<EventSystem>();

        // Find the GraphicRaycaster on the Canvas
        graphicRaycaster = FindFirstObjectByType<GraphicRaycaster>();
    }
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (GetTouchedUIObject(touch.position) == gameObject)
                {
                    ShowInfo();
                }
            }
            if (touch.phase == TouchPhase.Ended)
            {
                InfoPanel.SetActive(false);
            }
        }
    }

    private GameObject GetTouchedUIObject(Vector2 screenPosition)
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(eventData, results);

        if (results.Count > 0)
        {
            return results[0].gameObject; // Return the first UI object hit
        }
        return null;
    }

    public void ShowInfo()
    {
        InfoPanel.SetActive(true);
        if (!Locked)
        {
            NameTMP.SetText(Name);
            DescriptionTMP.SetText(Description);
        }
        else
        {
            NameTMP.SetText("???");
            DescriptionTMP.SetText("Locked");
        }
    }
    public void UnlockAchievement()
    {
        Locked = false;
        Background.GetComponent<Image>().color = Color.white;
        Image.GetComponent<Image>().color = Color.white;
    }
}
