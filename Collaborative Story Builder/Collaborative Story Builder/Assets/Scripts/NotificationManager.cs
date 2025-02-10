using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance; 

    public TextMeshProUGUI notificationText;
    public CanvasGroup canvasGroup; 
    public float fadeDuration = 0.5f; 
    public float displayTime = 3f; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        HideInstant(); 
    }

    public void Notify(string message, float? duration = null)
    {
        if (notificationText == null || canvasGroup == null)
        {
            Debug.LogError("Notification UI elements are not assigned!");
            return;
        }

        notificationText.text = message;
        StopAllCoroutines();
        StartCoroutine(ShowAndHide(duration ?? displayTime));
    }

    private IEnumerator ShowAndHide(float duration)
    {
        yield return StartCoroutine(FadeIn()); 
        yield return new WaitForSeconds(duration); 
        yield return StartCoroutine(FadeOut()); 
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }

    public void HideInstant()
    {
        canvasGroup.alpha = 0; 
    }
}
