using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Applies the project Yoster TMP font to all UI text when a scene loads.
/// </summary>
public static class GameUIFontApplier
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnSceneLoaded()
    {
        SceneManager.sceneLoaded += (_, __) => Apply();
        Apply();
    }

    static void Apply()
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font == null) return;

        foreach (TextMeshProUGUI text in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text.GetComponentInParent<ObjectiveHUD>() != null)
                continue;

            text.font = font;
            text.fontSharedMaterial = font.material;
            text.fontSize = NormalizeSize(text.fontSize);
        }

        foreach (TextMeshPro text in Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            text.font = font;
            text.fontSharedMaterial = font.material;
            text.fontSize = NormalizeSize(text.fontSize);
        }
    }

    static float NormalizeSize(float size)
    {
        if (size <= 12f) return 22f;
        if (size <= 17f) return 26f;
        if (size <= 22f) return 30f;
        if (size <= 26f) return 34f;
        if (size <= 36f) return 40f;
        if (size <= 54f) return 48f;
        if (size <= 64f) return 56f;
        if (size <= 80f) return 60f;
        if (size >= 100f) return 68f;
        return size;
    }
}
