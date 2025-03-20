using UnityEngine;
using TMPro;

public class FriendCardController : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text levelText;

    public void Setup(string username, int level)
    {
        if (usernameText != null)
            usernameText.text = username;

        if (levelText != null)
            levelText.text = "Level: " + level;
    }
}
