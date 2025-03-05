using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class LevelSystem
{
    private static UserData user;
    private static int XP;
    public static int Level { get; private set; }

    public static int GetXPForNextLevel()
    {
        return User.GetUser().UserLevel * 100;
    }

    public static void AddXP(int amount)
    {
        Debug.Log("Trying to add " + amount + "XP");
        XP = PlayerPrefs.GetInt("XP", 0);
        Level = User.GetUser().UserLevel;

        XP += amount; // Add XP

        if (XP >= GetXPForNextLevel()) // Level Up Logic
        {
            XP -= GetXPForNextLevel();
            Level++;

            ShowLevelUpWidget();

            PlayerPrefs.SetInt("lvl", Level);
            PlayerPrefs.Save();

            User.GetUser().UserLevel = Level;
            User.UpdateUserInfo(User.GetUser().UserID, userLevel: Level);
        }
        else
        {
            ShowXPWidget(amount);
        }

        PlayerPrefs.SetInt("XP", XP);
        PlayerPrefs.Save();
    }

    private static void ShowXPWidget(int XPGained)
    {
        GameObject widgetPrefab = Resources.Load<GameObject>("LevelUpWidget");
        if (widgetPrefab != null)
        {
            if (widgetPrefab != null)
            {
                GameObject widgetInstance = GameObject.Instantiate(widgetPrefab);

                // Find the Canvas in the scene
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    widgetInstance.transform.SetParent(canvas.transform, false); // Attach to UI Canvas
                }
                else
                {
                    Debug.LogWarning("No Canvas found in the scene!");
                    return;
                }

                // Reset position to center
                RectTransform rect = widgetInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero; // Center in UI
                }

                TextMeshProUGUI text = widgetInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"CONGRATS! JUST GAINED {XPGained} XP.";
                    GameObject.Destroy(widgetInstance, 5f);
                }
            }
            else
            {
                Debug.LogWarning("LevelUpWidget prefab not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("LevelUpWidget prefab not found in Resources!");
        }
    }


    private static void ShowLevelUpWidget()
    {
        GameObject widgetPrefab = Resources.Load<GameObject>("LevelUpWidget");
        if (widgetPrefab != null)
        {
            if (widgetPrefab != null)
            {
                GameObject widgetInstance = GameObject.Instantiate(widgetPrefab);

                // Find the Canvas in the scene
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    widgetInstance.transform.SetParent(canvas.transform, false); // Attach to UI Canvas
                }
                else
                {
                    Debug.LogWarning("No Canvas found in the scene!");
                    return;
                }

                // Reset position to center
                RectTransform rect = widgetInstance.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero; // Center in UI
                }

                TextMeshProUGUI text = widgetInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"LEVEL UP! Now Level {Level}";
                    GameObject.Destroy(widgetInstance, 5f);
                }
            }
            else
            {
                Debug.LogWarning("LevelUpWidget prefab not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("LevelUpWidget prefab not found in Resources!");
        }
    }
}
