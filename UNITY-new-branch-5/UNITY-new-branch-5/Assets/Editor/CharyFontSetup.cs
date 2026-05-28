#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates a TMP SDF asset from Assets/fonts/chary___.ttf for objective UI.
/// Menu: Tools / Setup Chary Font
/// </summary>
public static class CharyFontSetup
{
    const string FontAssetPath = "Assets/fonts/Chary SDF.asset";
    const string SourceFontPath = "Assets/fonts/chary___.ttf";

    [MenuItem("Tools/Setup Chary Font")]
    public static void Setup()
    {
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"Missing source font at {SourceFontPath}");
            return;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (fontAsset == null)
        {
            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("Chary font SDF is ready at Assets/fonts/Chary SDF.asset");
    }
}
#endif
