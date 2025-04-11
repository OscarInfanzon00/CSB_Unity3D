using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollToTopController : MonoBehaviour
{
    public ScrollRect scrollRect;            // Drag your Scroll View here
    public Button scrollToTopButton;         // Drag the "Scroll to Top" button here

    private float bottomThreshold = 0.01f; // How close to bottom before showing button
    private float topThreshold = 1.01f;

    void Start()
    {
        scrollToTopButton.onClick.AddListener(ScrollToTop);
    }

    void Update()
    {
        float scrollPosition = scrollRect.verticalNormalizedPosition;

        // Show the button if we're close to the bottom
        if (scrollPosition <= bottomThreshold || scrollPosition >= topThreshold)
        {
            scrollToTopButton.gameObject.SetActive(true);
        }
        else
        {
            scrollToTopButton.gameObject.SetActive(false);
        }
    }

    public void ScrollToTop()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        StartCoroutine(SmoothScrollToTop());
    }

    private IEnumerator SmoothScrollToTop()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        float start = scrollRect.verticalNormalizedPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, 1f, elapsed / duration);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = 1f;
    }
}
