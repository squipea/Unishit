using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Bottom-left heart HUD with crack (frames 1-6) and explode (frames 7-17) animations.
/// </summary>
public class PlayerHeartsUI : MonoBehaviour
{
    public static PlayerHeartsUI Instance { get; private set; }

    [Header("Layout")]
    public Vector2 heartSize = new Vector2(72f, 72f);
    public float heartSpacingX = 14f;
    public Vector2 anchoredOffset = new Vector2(28f, 28f);

    [Header("Animation")]
    public float frameDuration = 0.07f;

    const string ResourcesFolder = "Hearts";
    const string EditorHeartFolder = "Assets/Sprites (animation)/Heart";

    private Canvas canvas;
    private RectTransform container;
    private readonly List<Image> heartImages = new List<Image>();
    private readonly List<Coroutine> heartCoroutines = new List<Coroutine>();

    private static Sprite[] heartSprites;
    private int damagePerHeart = 2;
    private int lastSyncedHealth = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static PlayerHeartsUI EnsureExists()
    {
        if (Instance != null)
            return Instance;

        var existing = FindAnyObjectByType<PlayerHeartsUI>();
        if (existing != null)
            return existing;

        var go = new GameObject("PlayerHeartsUI");
        Instance = go.AddComponent<PlayerHeartsUI>();
        return Instance;
    }

    static Sprite GetHeartSprite(int frameIndex)
    {
        EnsureSpritesLoaded();
        if (heartSprites == null || frameIndex < 1 || frameIndex > heartSprites.Length)
            return null;
        return heartSprites[frameIndex - 1];
    }

    static void EnsureSpritesLoaded()
    {
        if (heartSprites != null && heartSprites[0] != null)
            return;

        heartSprites = new Sprite[17];
        for (int i = 1; i <= 17; i++)
        {
            string resourceName = $"{ResourcesFolder}/Heart explode {i}";
            Sprite sprite = Resources.Load<Sprite>(resourceName);

#if UNITY_EDITOR
            if (sprite == null)
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{EditorHeartFolder}/Heart explode {i}.png");
#endif
            heartSprites[i - 1] = sprite;
        }
    }

    Canvas EnsureCanvas()
    {
        if (canvas != null)
            return canvas;

        GameObject canvasGo = new GameObject("HeartsCanvas");
        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        var rt = canvasGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        return canvas;
    }

    RectTransform EnsureContainer()
    {
        EnsureCanvas();
        if (container != null)
            return container;

        GameObject containerGo = new GameObject("HeartsContainer");
        containerGo.transform.SetParent(canvas.transform, false);

        container = containerGo.AddComponent<RectTransform>();
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.zero;
        container.pivot = Vector2.zero;
        container.anchoredPosition = anchoredOffset;
        container.sizeDelta = new Vector2(1f, heartSize.y);

        return container;
    }

    public void Initialize(int heartCount, int damagePerHeartValue = 2)
    {
        damagePerHeart = Mathf.Max(1, damagePerHeartValue);
        EnsureContainer();
        EnsureSpritesLoaded();

        if (heartImages.Count == heartCount)
            return;

        ClearHearts();

        Sprite fullSprite = GetHeartSprite(1);
        for (int i = 0; i < heartCount; i++)
        {
            var heartGo = new GameObject($"Heart_{i}");
            heartGo.transform.SetParent(container, false);

            var rt = heartGo.AddComponent<RectTransform>();
            rt.sizeDelta = heartSize;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(i * (heartSize.x + heartSpacingX), 0f);

            var img = heartGo.AddComponent<Image>();
            img.sprite = fullSprite;
            img.preserveAspect = true;
            img.color = Color.white;
            img.enabled = true;

            heartImages.Add(img);
            heartCoroutines.Add(null);
        }
    }

    void ClearHearts()
    {
        for (int i = 0; i < heartCoroutines.Count; i++)
        {
            if (heartCoroutines[i] != null)
                StopCoroutine(heartCoroutines[i]);
        }
        heartCoroutines.Clear();

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
                Destroy(heartImages[i].gameObject);
        }
        heartImages.Clear();
        lastSyncedHealth = -1;
    }

    public void OnHealthChanged(int previousHealth, int currentHealth, int heartCount, int damagePerHeartValue)
    {
        damagePerHeart = Mathf.Max(1, damagePerHeartValue);
        Initialize(heartCount, damagePerHeart);

        if (lastSyncedHealth < 0)
            lastSyncedHealth = currentHealth;

        int oldHp = lastSyncedHealth;
        lastSyncedHealth = currentHealth;

        if (currentHealth >= oldHp)
        {
            RefreshStaticDisplay(currentHealth, heartCount);
            return;
        }

        for (int i = 0; i < heartCount; i++)
        {
            int prevInHeart = GetHpInHeart(oldHp, i);
            int currInHeart = GetHpInHeart(currentHealth, i);
            if (currInHeart >= prevInHeart)
                continue;

            if (currInHeart == 0)
                PlayHeartAnimation(i, 7, 17, false);
            else if (currInHeart < prevInHeart)
                PlayHeartAnimation(i, 1, 6, true);
        }
    }

    public void RefreshStaticDisplay(int currentHealth, int heartCount)
    {
        Initialize(heartCount, damagePerHeart);
        lastSyncedHealth = currentHealth;

        for (int i = 0; i < heartCount && i < heartImages.Count; i++)
        {
            if (heartCoroutines[i] != null)
            {
                StopCoroutine(heartCoroutines[i]);
                heartCoroutines[i] = null;
            }

            int hpInHeart = GetHpInHeart(currentHealth, i);
            ApplyStaticHeartVisual(heartImages[i], hpInHeart);
        }
    }

    void ApplyStaticHeartVisual(Image img, int hpInHeart)
    {
        if (img == null)
            return;

        if (hpInHeart >= damagePerHeart)
        {
            img.enabled = true;
            img.color = Color.white;
            img.sprite = GetHeartSprite(1);
        }
        else if (hpInHeart > 0)
        {
            img.enabled = true;
            img.color = Color.white;
            img.sprite = GetHeartSprite(6);
        }
        else
        {
            img.enabled = false;
        }
    }

    int GetHpInHeart(int totalHealth, int heartIndex)
    {
        int hpStart = heartIndex * damagePerHeart;
        return Mathf.Clamp(totalHealth - hpStart, 0, damagePerHeart);
    }

    void PlayHeartAnimation(int heartIndex, int firstFrame, int lastFrame, bool holdLastFrame)
    {
        if (heartIndex < 0 || heartIndex >= heartImages.Count)
            return;

        if (heartCoroutines[heartIndex] != null)
            StopCoroutine(heartCoroutines[heartIndex]);

        heartCoroutines[heartIndex] = StartCoroutine(AnimateHeartFrames(heartIndex, firstFrame, lastFrame, holdLastFrame));
    }

    IEnumerator AnimateHeartFrames(int heartIndex, int firstFrame, int lastFrame, bool holdLastFrame)
    {
        Image img = heartImages[heartIndex];
        if (img == null)
            yield break;

        img.enabled = true;
        img.color = Color.white;

        for (int frame = firstFrame; frame <= lastFrame; frame++)
        {
            Sprite sprite = GetHeartSprite(frame);
            if (sprite != null)
                img.sprite = sprite;

            yield return new WaitForSeconds(frameDuration);
        }

        if (holdLastFrame)
        {
            Sprite cracked = GetHeartSprite(6);
            if (cracked != null)
                img.sprite = cracked;
        }
        else
        {
            img.enabled = false;
        }

        heartCoroutines[heartIndex] = null;
    }

    IEnumerator HeartPop()
    {
        float t = 0f;
        Vector3 baseScale = container.localScale;
        container.localScale = baseScale * 1.06f;
        while (t < 0.1f)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - (t / 0.1f);
            container.localScale = baseScale * (1f + 0.06f * k);
            yield return null;
        }
        container.localScale = baseScale;
        popCoroutine = null;
    }

    Coroutine popCoroutine;

    public void PlayDamagePop()
    {
        if (container == null)
            return;
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);
        popCoroutine = StartCoroutine(HeartPop());
    }
}
