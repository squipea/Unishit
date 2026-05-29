using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keeps cutscene dialogue panel and skip hint sized and positioned correctly.
/// </summary>
public class CutsceneDialogueUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform dialogueBoxRect;
    public RectTransform borderRect;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI skipHintText;

    [Header("Sizing")]
    public float dialogueBoxHeight = 200f;
    public float dialogueBoxBottomOffset = 60f;
    public float borderInset = 12f;
    public float textPadding = 24f;
    public float dialogueFontSize = 32f;
    public float skipHintFontSize = 26f;

    void Awake()
    {
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (dialogueBoxRect != null)
        {
            dialogueBoxRect.localScale = Vector3.one;
            dialogueBoxRect.anchorMin = new Vector2(0.05f, 0f);
            dialogueBoxRect.anchorMax = new Vector2(0.95f, 0f);
            dialogueBoxRect.pivot = new Vector2(0.5f, 0f);
            dialogueBoxRect.anchoredPosition = new Vector2(0f, dialogueBoxBottomOffset);
            dialogueBoxRect.sizeDelta = new Vector2(0f, dialogueBoxHeight);

            Image panelImage = dialogueBoxRect.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.type = Image.Type.Sliced;
                if (panelImage.sprite == null)
                    panelImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
                panelImage.color = new Color(0f, 0f, 0f, 0.88f);
            }
        }

        if (borderRect != null)
        {
            borderRect.localScale = Vector3.one;
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.pivot = new Vector2(0.5f, 0.5f);
            borderRect.anchoredPosition = Vector2.zero;
            borderRect.offsetMin = new Vector2(borderInset, borderInset);
            borderRect.offsetMax = new Vector2(-borderInset, -borderInset);

            Image borderImage = borderRect.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.type = Image.Type.Sliced;
                if (borderImage.sprite == null)
                    borderImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                borderImage.color = new Color(0.72f, 0.72f, 0.72f, 1f);
            }
        }

        if (dialogueText != null)
        {
            RectTransform textRect = dialogueText.rectTransform;
            textRect.localScale = Vector3.one;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.offsetMin = new Vector2(textPadding, textPadding);
            textRect.offsetMax = new Vector2(-textPadding, -textPadding);

            dialogueText.fontSize = dialogueFontSize;
            dialogueText.margin = Vector4.zero;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
            dialogueText.enableWordWrapping = true;
        }

        if (skipHintText != null)
        {
            RectTransform skipRect = skipHintText.rectTransform;
            skipRect.localScale = Vector3.one;
            skipRect.anchorMin = new Vector2(1f, 0f);
            skipRect.anchorMax = new Vector2(1f, 0f);
            skipRect.pivot = new Vector2(1f, 0f);
            skipRect.anchoredPosition = new Vector2(-32f, 20f);
            skipRect.sizeDelta = new Vector2(420f, 44f);

            skipHintText.fontSize = skipHintFontSize;
            skipHintText.color = new Color(1f, 1f, 1f, 0.9f);
            skipHintText.alignment = TextAlignmentOptions.BottomRight;
            skipHintText.textWrappingMode = TextWrappingModes.NoWrap;
            if (string.IsNullOrWhiteSpace(skipHintText.text))
                skipHintText.text = "Press SPACE to skip";
        }
    }
}
