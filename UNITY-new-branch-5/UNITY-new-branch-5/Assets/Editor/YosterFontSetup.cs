#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures the Yoster TMP font asset exists and is the project default.
/// Menu: Tools / Setup Yoster Font
/// </summary>
public static class YosterFontSetup
{
    const string FontAssetPath = "Assets/fonts/Yoster SDF.asset";
    const string SourceFontPath = "Assets/fonts/yoster.ttf";
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("Tools/Setup Yoster Font")]
    public static void Setup()
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);

        if (sourceFont == null)
        {
            Debug.LogError($"Missing source font at {SourceFontPath}");
            return;
        }

        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        }

        TMP_Settings.defaultFontAsset = fontAsset;

        TMP_Settings settingsAsset = AssetDatabase.LoadAssetAtPath<TMP_Settings>(TmpSettingsPath);
        if (settingsAsset != null)
        {
            EditorUtility.SetDirty(settingsAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("Yoster font is ready and set as TMP default.");
    }
}
#endif
