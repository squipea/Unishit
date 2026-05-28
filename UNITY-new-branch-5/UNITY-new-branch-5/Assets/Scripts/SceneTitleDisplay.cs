using UnityEngine;
using TMPro;
using System.Collections;

public class SceneTitleDisplay : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    
    [Header("Timing Settings")]
    public float fadeInDuration = 1.0f;    // Time to fade from 0 to 1 alpha
    public float displayDuration = 2.5f;   // Time to stay fully visible
    public float fadeOutDuration = 1.5f;   // Time to fade from 1 to 0 alpha
    public float delayBeforeStart = 0.5f;  // Delay after screen fade-in starts

    void Start()
    {
        if (titleText == null) titleText = GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            // Start transparent
            Color c = titleText.color;
            c.a = 0f;
            titleText.color = c;
            
            StartCoroutine(DisplayRoutine());
        }
    }

    IEnumerator DisplayRoutine()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        Color c = titleText.color;
        float elapsed = 0f;

        // Fade In
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            titleText.color = c;
            yield return null;
        }
        c.a = 1f;
        titleText.color = c;

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade Out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            titleText.color = c;
            yield return null;
        }

        c.a = 0f;
        titleText.color = c;
        gameObject.SetActive(false);
    }
}