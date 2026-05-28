using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attaches a world-space health bar above any enemy.
/// Creates its own Canvas, Slider, and Images at runtime — no prefab required.
/// Call SetHealth(current, max) to update the bar.
/// </summary>
public class EnemyWorldHealthBar : MonoBehaviour
{
    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 2.5f, 0f);

    [Header("Size")]
    public float barWidth = 1.6f;
    public float barHeight = 0.22f;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
    public Color fillColor = new Color(0.2f, 0.85f, 0.25f, 1f);
    public Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    private Canvas canvas;
    private Slider slider;
    private Image fillImage;
    private RectTransform canvasRect;

    void Awake()
    {
        CreateHealthBar();
    }

    void LateUpdate()
    {
        if (canvas == null) return;

        // Keep the health bar above the enemy and always upright
        canvas.transform.position = transform.position + offset;
        canvas.transform.rotation = Quaternion.identity;

        // Compensate for parent scale flips so bar doesn't mirror
        Vector3 parentScale = transform.localScale;
        float sign = parentScale.x < 0 ? -1f : 1f;
        canvasRect.localScale = new Vector3(sign * Mathf.Abs(canvasRect.localScale.x),
                                             Mathf.Abs(canvasRect.localScale.y),
                                             1f);
    }

    public void SetHealth(int current, int max)
    {
        if (slider == null) return;

        slider.maxValue = max;
        slider.value = current;

        if (fillImage != null)
        {
            float t = (max > 0) ? (float)current / max : 0f;
            fillImage.color = Color.Lerp(lowHealthColor, fillColor, t);
        }
    }

    public void Hide()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(true);
    }

    void CreateHealthBar()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("WorldHealthBarCanvas");
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = offset;
        canvasGO.transform.localRotation = Quaternion.identity;

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(barWidth, barHeight);
        canvasRect.localScale = Vector3.one;

        // --- Slider ---
        GameObject sliderGO = new GameObject("HealthSlider");
        sliderGO.transform.SetParent(canvasGO.transform, false);

        slider = sliderGO.AddComponent<Slider>();
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        slider.direction = Slider.Direction.LeftToRight;

        RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = Vector2.zero;
        sliderRect.anchoredPosition = Vector2.zero;

        // --- Background ---
        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(sliderGO.transform, false);
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = backgroundColor;

        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        // --- Fill Area ---
        GameObject fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(sliderGO.transform, false);

        RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;

        // --- Fill ---
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        fillImage = fillGO.AddComponent<Image>();
        fillImage.color = fillColor;

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        // Wire up slider references
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        // Defaults
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;
    }

    void OnDestroy()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
    }
}
