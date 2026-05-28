using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen red vignette flash when the player takes damage.
/// </summary>
public class DamageScreenEffect : MonoBehaviour
{
    public static DamageScreenEffect Instance { get; private set; }

    [Header("Flash")]
    public float maxAlpha = 0.42f;
    public float fadeInDuration = 0.05f;
    public float fadeOutDuration = 0.28f;

    static readonly Color VignetteColor = new Color(0.82f, 0.06f, 0.05f, 1f);

    Image vignetteImage;
    Coroutine flashCoroutine;
    Texture2D vignetteTexture;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    public static DamageScreenEffect EnsureExists()
    {
        if (Instance != null)
            return Instance;

        var existing = FindAnyObjectByType<DamageScreenEffect>();
        if (existing != null)
            return existing;

        var go = new GameObject("DamageScreenEffect");
        return go.AddComponent<DamageScreenEffect>();
    }

    void BuildOverlay()
    {
        var canvasGo = new GameObject("DamageVignetteCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var imageGo = new GameObject("Vignette");
        imageGo.transform.SetParent(canvasGo.transform, false);

        var rt = imageGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        vignetteImage = imageGo.AddComponent<Image>();
        vignetteImage.raycastTarget = false;
        vignetteImage.sprite = CreateVignetteSprite();
        vignetteImage.color = new Color(VignetteColor.r, VignetteColor.g, VignetteColor.b, 0f);
        vignetteImage.enabled = true;
    }

    Sprite CreateVignetteSprite()
    {
        const int size = 256;
        vignetteTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        vignetteTexture.wrapMode = TextureWrapMode.Clamp;
        vignetteTexture.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float invHalf = 1f / center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) * invHalf;
                float dy = (y - center) * invHalf;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((dist - 0.25f) / 0.75f);
                alpha = alpha * alpha;
                vignetteTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        vignetteTexture.Apply();
        return Sprite.Create(
            vignetteTexture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    public void PlayDamageFlash(float intensity = 1f)
    {
        if (vignetteImage == null)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine(intensity));
    }

    IEnumerator FlashRoutine(float intensity)
    {
        float peak = maxAlpha * Mathf.Clamp(intensity, 0.35f, 1.25f);
        float t = 0f;

        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, peak, t / fadeInDuration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(peak);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(peak, 0f, t / fadeOutDuration);
            SetAlpha(a);
            yield return null;
        }

        SetAlpha(0f);
        flashCoroutine = null;
    }

    void SetAlpha(float alpha)
    {
        if (vignetteImage == null)
            return;

        vignetteImage.color = new Color(VignetteColor.r, VignetteColor.g, VignetteColor.b, alpha);
    }

    void OnDestroy()
    {
        if (vignetteTexture != null)
            Destroy(vignetteTexture);

        if (Instance == this)
            Instance = null;
    }
}
