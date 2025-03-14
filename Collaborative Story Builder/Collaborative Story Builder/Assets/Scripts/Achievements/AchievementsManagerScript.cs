using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class AchievementsManagerScript : MonoBehaviour
{
    public List<GameObject> achievementPrefabs;
    public Transform gridParent; 

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
}
