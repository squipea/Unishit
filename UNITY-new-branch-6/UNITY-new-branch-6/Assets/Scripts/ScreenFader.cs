using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    public Image fadeImage;
    
    [Header("Default Timing")]
    public float defaultFadeInDuration = 1.5f;
    public float defaultFadeOutDuration = 1.5f;
    public bool fadeInOnStart = true;

    private void Awake()
    {
        // Simple per-scene singleton
        Instance = this;

        if (fadeImage == null) fadeImage = GetComponent<Image>();
        
        // Ensure the image covers the screen and is ready
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
            // Ensure it's black if we want to fade in
            if (fadeInOnStart)
            {
                Color c = fadeImage.color;
                c.a = 1f;
                fadeImage.color = c;
                fadeImage.gameObject.SetActive(true);
            }
            else
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
                fadeImage.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (fadeInOnStart)
        {
            // Start by fading in the screen (Black -> Clear)
            StartFadeIn();
        }
    }

    public void StartFadeIn(float duration = -1f)
    {
        StopAllCoroutines();
        if (duration < 0) duration = defaultFadeInDuration;
        StartCoroutine(FadeRoutine(1f, 0f, duration));
    }

    public void StartFadeOut(float duration = -1f)
    {
        StopAllCoroutines();
        if (duration < 0) duration = defaultFadeOutDuration;
        StartCoroutine(FadeRoutine(0f, 1f, duration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;

        if (endAlpha <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}