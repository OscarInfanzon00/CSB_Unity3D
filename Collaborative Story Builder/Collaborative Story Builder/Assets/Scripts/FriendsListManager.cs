using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CloseButtonHandler : MonoBehaviour
{
    public Button CloseButton;

    void Start()
    {
        CloseButton.onClick.AddListener(CloseScene);
    }

    private void CloseScene()
    {
        SceneManager.LoadScene("Profile");
    }
}
