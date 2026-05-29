using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Runtime game-over panel shown when the player dies.
/// </summary>
public class GameOverModalManager : MonoBehaviour
{
    public static GameOverModalManager Instance { get; private set; }

    [Header("Layout")]
    public Vector2 panelSize = new Vector2(560f, 480f); // Larger modal
    public Vector2 buttonSize = new Vector2(380f, 70f); // Larger buttons
    public float buttonSpacing = 18f;

    [Header("Typography")]
    public float titleFontSize = 56f; // Larger typography
    public float buttonFontSize = 32f;

    const string CharyFontResourcePath = "Fonts/chary___";
    const string CharyFontAssetPath = "Assets/fonts/chary___.ttf";
    const string CharySdfAssetPath = "Assets/fonts/Chary SDF.asset";

    static readonly Color PanelFill = new Color(0.06f, 0.07f, 0.09f, 0.99f); // Sleek deep slate
    static readonly Color OuterBorder = new Color(0.15f, 0.17f, 0.20f, 1f); // Muted charcoal/steel border
    static readonly Color InnerBorder = new Color(0.24f, 0.26f, 0.30f, 0.85f); // Deep slate inner border
    static readonly Color AccentRed = new Color(0.88f, 0.24f, 0.22f, 1f);
    static readonly Color AccentRedDark = new Color(0.45f, 0.1f, 0.1f, 1f);
    static readonly Color TitleColor = new Color(0.95f, 0.96f, 0.98f, 1f); // Premium silver-white
    static readonly Color ButtonNormal = new Color(0.12f, 0.13f, 0.16f, 1f); // Muted dark button body
    static readonly Color DimOverlay = new Color(0.02f, 0.02f, 0.03f, 0.92f); // Deepened background dim

    static TMP_FontAsset cachedCharyFont;

    GameObject rootPanel;
    RectTransform boxTransform;
    bool isVisible;
    Coroutine showAnimCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        UIEventSystemBootstrap.EnsureExists();
        BuildUI();
        HideImmediate();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            HideImmediate();
    }

    public static GameOverModalManager EnsureExists()
    {
        if (Instance != null)
            return Instance;

        var existing = FindAnyObjectByType<GameOverModalManager>();
        if (existing != null)
            return existing;

        var go = new GameObject("GameOverModalManager");
        return go.AddComponent<GameOverModalManager>();
    }

    static TMP_FontAsset GetCharyFont()
    {
        if (cachedCharyFont != null)
            return cachedCharyFont;

#if UNITY_EDITOR
        TMP_FontAsset editorSdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(CharySdfAssetPath);
        if (editorSdf != null)
        {
            cachedCharyFont = editorSdf;
            return cachedCharyFont;
        }
#endif

        Font sourceFont = Resources.Load<Font>(CharyFontResourcePath);
#if UNITY_EDITOR
        if (sourceFont == null)
            sourceFont = AssetDatabase.LoadAssetAtPath<Font>(CharyFontAssetPath);
#endif

        if (sourceFont != null)
            cachedCharyFont = TMP_FontAsset.CreateFontAsset(sourceFont);

        return cachedCharyFont;
    }

    static void ApplyCharyFont(TextMeshProUGUI text, float fontSize, Color color)
    {
        TMP_FontAsset font = GetCharyFont();
        if (font != null)
        {
            text.font = font;
            text.fontSharedMaterial = font.material;
        }

        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.color = color;
    }

    static Sprite GetUiSprite()
    {
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    }

    static Sprite GetUiBackground()
    {
        return Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
    }

    public void Show()
    {
        EnsureExists();
        if (isVisible)
            return;

        isVisible = true;
        if (rootPanel != null)
            rootPanel.SetActive(true);

        Time.timeScale = 0f;
        if (GameManager.Instance != null)
            GameManager.Instance.SetPaused(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (UIAudioHelper.Instance != null && rootPanel != null)
            UIAudioHelper.Instance.WireButtonsUnder(rootPanel.transform);

        if (showAnimCoroutine != null)
            StopCoroutine(showAnimCoroutine);
        showAnimCoroutine = StartCoroutine(PlayShowAnimation());
    }

    public void HideImmediate()
    {
        isVisible = false;
        if (showAnimCoroutine != null)
        {
            StopCoroutine(showAnimCoroutine);
            showAnimCoroutine = null;
        }

        if (boxTransform != null)
            boxTransform.localScale = Vector3.one;

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    IEnumerator PlayShowAnimation()
    {
        if (boxTransform == null)
            yield break;

        float duration = 0.22f;
        float t = 0f;
        Vector3 start = Vector3.one * 0.88f;
        Vector3 end = Vector3.one;
        boxTransform.localScale = start;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            boxTransform.localScale = Vector3.Lerp(start, end, k);
            yield return null;
        }

        boxTransform.localScale = end;
        showAnimCoroutine = null;
    }

    void BuildUI()
    {
        if (rootPanel != null)
            return;

        var canvasGo = new GameObject("GameOverCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        rootPanel = new GameObject("GameOverPanel");
        rootPanel.transform.SetParent(canvasGo.transform, false);

        var panelRt = rootPanel.AddComponent<RectTransform>();
        StretchFull(panelRt);

        CreateImage(rootPanel.transform, "Dim", StretchFull, DimOverlay, null, false);

        var box = new GameObject("Box");
        box.transform.SetParent(rootPanel.transform, false);
        boxTransform = box.AddComponent<RectTransform>();
        boxTransform.anchorMin = new Vector2(0.5f, 0.5f);
        boxTransform.anchorMax = new Vector2(0.5f, 0.5f);
        boxTransform.pivot = new Vector2(0.5f, 0.5f);
        boxTransform.sizeDelta = panelSize;

        var shadow = box.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(4f, -4f);

        CreateFrame(box.transform, "OuterFrame", 0f, OuterBorder);
        CreateSlicedPanel(box.transform, "PanelFill", 6f, PanelFill, GetUiBackground());
        CreateFrame(box.transform, "InnerFrame", 12f, InnerBorder);

        // Red accents removed as requested
        // CreateBar(box.transform, "TopAccent", new Vector2(-panelSize.x * 0.5f + 18f, panelSize.y * 0.5f - 14f), new Vector2(panelSize.x - 36f, 6f), AccentRed);
        // CreateBar(box.transform, "TopAccentShadow", new Vector2(-panelSize.x * 0.5f + 18f, panelSize.y * 0.5f - 20f), new Vector2(panelSize.x - 36f, 2f), AccentRedDark);

        CreateCornerPixels(box.transform, 18f);

        Sprite heartSprite = Resources.Load<Sprite>("Hearts/Heart explode 6");
#if UNITY_EDITOR
        if (heartSprite == null)
            heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites (animation)/Heart/Heart explode 6.png");
#endif
        // Exploding heart icons removed as requested
        /*
        if (heartSprite != null)
        {
            CreateHeartIcon(box.transform, "HeartLeft", new Vector2(-125f, panelSize.y * 0.5f - 52f), heartSprite);
            CreateHeartIcon(box.transform, "HeartRight", new Vector2(125f, panelSize.y * 0.5f - 52f), heartSprite);
        }
        */

        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(box.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -42f);
        titleRt.sizeDelta = new Vector2(panelSize.x - 48f, 60f);

        var titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = "YOU LOSE";
        titleText.alignment = TextAlignmentOptions.Center;
        ApplyCharyFont(titleText, titleFontSize, TitleColor);
        titleText.outlineWidth = 0.18f;
        titleText.outlineColor = AccentRedDark;

        CreateBar(box.transform, "TitleDivider", new Vector2(0f, panelSize.y * 0.5f - 104f), new Vector2(panelSize.x - 56f, 2f),
            new Color(0.55f, 0.42f, 0.28f, 0.7f));

        var subtitleGo = new GameObject("Subtitle");
        subtitleGo.transform.SetParent(box.transform, false);
        var subtitleRt = subtitleGo.AddComponent<RectTransform>();
        subtitleRt.anchorMin = new Vector2(0.5f, 1f);
        subtitleRt.anchorMax = new Vector2(0.5f, 1f);
        subtitleRt.pivot = new Vector2(0.5f, 1f);
        subtitleRt.anchoredPosition = new Vector2(0f, -110f);
        subtitleRt.sizeDelta = new Vector2(panelSize.x - 48f, 32f);
        var subtitleText = subtitleGo.AddComponent<TextMeshProUGUI>();
        subtitleText.text = "- Defeated -";
        subtitleText.alignment = TextAlignmentOptions.Center;
        ApplyCharyFont(subtitleText, 26f, new Color(0.75f, 0.62f, 0.55f, 0.9f));

        float y = -156f;
        CreateButton(box.transform, "Retry", y, OnRetry, new Color(0.32f, 0.62f, 0.38f, 1f));
        y -= buttonSize.y + buttonSpacing;
        CreateButton(box.transform, "Main Menu", y, OnMainMenu, new Color(0.55f, 0.62f, 0.78f, 1f));
        y -= buttonSize.y + buttonSpacing;
        CreateButton(box.transform, "Exit Game", y, OnExitGame, new Color(0.78f, 0.38f, 0.34f, 1f));
    }

    void CreateFrame(Transform parent, string name, float inset, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);

        var img = go.AddComponent<Image>();
        img.sprite = GetUiSprite();
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;
    }

    void CreateSlicedPanel(Transform parent, string name, float inset, Color color, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
    }

    void CreateBar(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    void CreateCornerPixels(Transform parent, float inset)
    {
        Color pixel = new Color(0.12f, 0.13f, 0.15f, 1f); // Dark slate grey corner pixels
        float halfW = panelSize.x * 0.5f;
        float halfH = panelSize.y * 0.5f;
        Vector2 size = new Vector2(8f, 8f);

        CreateBar(parent, "CornerTL", new Vector2(-halfW + inset, halfH - inset), size, pixel);
        CreateBar(parent, "CornerTR", new Vector2(halfW - inset, halfH - inset), size, pixel);
        CreateBar(parent, "CornerBL", new Vector2(-halfW + inset, -halfH + inset), size, pixel);
        CreateBar(parent, "CornerBR", new Vector2(halfW - inset, -halfH + inset), size, pixel);
    }

    void CreateHeartIcon(Transform parent, string name, Vector2 pos, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(28f, 28f);

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.color = new Color(0.9f, 0.35f, 0.35f, 0.85f);
        img.raycastTarget = false;
    }

    Image CreateImage(Transform parent, string name, System.Action<RectTransform> layout, Color color, Sprite sprite, bool raycast)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        layout(rt);

        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }

        return img;
    }

    void CreateButton(Transform parent, string label, float y, UnityEngine.Events.UnityAction onClick, Color accent)
    {
        var go = new GameObject(label.Replace(" ", "") + "Button");
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = buttonSize;

        var bg = go.AddComponent<Image>();
        bg.sprite = GetUiSprite();
        bg.type = Image.Type.Sliced;
        bg.color = ButtonNormal;

        var accentBar = new GameObject("Accent");
        accentBar.transform.SetParent(go.transform, false);
        var accentRt = accentBar.AddComponent<RectTransform>();
        accentRt.anchorMin = new Vector2(0f, 1f);
        accentRt.anchorMax = new Vector2(1f, 1f);
        accentRt.pivot = new Vector2(0.5f, 1f);
        accentRt.anchoredPosition = Vector2.zero;
        accentRt.sizeDelta = new Vector2(-8f, 3f);
        var accentImg = accentBar.AddComponent<Image>();
        accentImg.color = new Color(0.24f, 0.26f, 0.30f, 0.85f); // Sleek unified dark slate accent
        accentImg.raycastTarget = false;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        StretchFull(textRt);
        textRt.offsetMin = new Vector2(8f, 4f);
        textRt.offsetMax = new Vector2(-8f, -6f);

        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        ApplyCharyFont(text, buttonFontSize, new Color(0.92f, 0.92f, 0.94f, 1f));
        text.outlineWidth = 0.12f;
        text.outlineColor = new Color(0f, 0f, 0f, 0.65f);
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameOver = false;
            GameManager.Instance.SetPaused(false);
        }

        HideImmediate();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameOver = false;
            GameManager.Instance.SetPaused(false);
        }

        HideImmediate();

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.QuitToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }

    public void OnExitGame()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameOver = false;
            GameManager.Instance.SetPaused(false);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
