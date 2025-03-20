using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BlockedCardController : MonoBehaviour
{
    public TMP_Text usernameText;

    public void Setup(string username)
    {
        if (usernameText != null)
            usernameText.text = username;
    }
}
