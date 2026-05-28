using UnityEngine;
using TMPro;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectiveHUD : MonoBehaviour
{
    public static ObjectiveHUD Instance { get; private set; }
    public TextMeshProUGUI objectiveText;

    [Header("Typography")]
    public float objectiveFontSize = 28f;

    const string CharyFontResourcePath = "Fonts/chary___";
    const string CharyFontAssetPath = "Assets/fonts/chary___.ttf";
    const string CharySdfAssetPath = "Assets/fonts/Chary SDF.asset";

    static TMP_FontAsset cachedCharyFont;

    private Dictionary<string, int> initialCounts = new Dictionary<string, int>();
    private bool allObjectivesMet = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ApplyObjectiveTypography();

        if (ShouldWaitForForestActivation())
        {
            if (objectiveText != null)
                objectiveText.text = "";
            return;
        }

        InitializeObjectives();
        UpdateUI();
    }

    static bool ShouldWaitForForestActivation()
    {
        return LevelManager.Instance != null
            && LevelManager.Instance.CurrentObjectiveMode == LevelManager.ObjectiveMode.Forest
            && !LevelManager.Instance.ObjectivesActive;
    }

    public void OnObjectivesActivated()
    {
        InitializeObjectives();
        UpdateUI();
    }

    void ApplyObjectiveTypography()
    {
        if (objectiveText == null)
            return;

        TMP_FontAsset font = GetCharyFont();
        if (font != null)
        {
            objectiveText.font = font;
            objectiveText.fontSharedMaterial = font.material;
        }

        objectiveText.fontSize = objectiveFontSize;
        objectiveText.enableAutoSizing = false;
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

    public void InitializeObjectives()
    {
        initialCounts.Clear();
        
        int soldiers = CountActive<SoldierAI>();
        if (soldiers > 0) initialCounts["Soldiers"] = soldiers;

        int crystals = CountActive<CrystalEnemyAI>();
        if (crystals > 0) initialCounts["Crystals"] = crystals;

        int tentacles = CountActive<TentacleBossAI>();
        if (tentacles > 0) initialCounts["Tentacles"] = tentacles;

        int owls = CountActive<OwlAI>();
        if (owls > 0) initialCounts["Owls"] = owls;

        int bears = 0;
        foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
        {
            if (enemy.enabled && enemy.name.ToLower().Contains("bear")) bears++;
        }
        if (bears > 0) initialCounts["Bears"] = bears;
    }

    int CountActive<T>() where T : MonoBehaviour
    {
        int count = 0;
        foreach (var obj in Object.FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            if (obj.enabled) count++;
        }
        return count;
    }

    public void UpdateUI()
    {
        if (objectiveText == null) return;

        if (ShouldWaitForForestActivation())
        {
            objectiveText.text = "";
            return;
        }

        if (LevelManager.Instance != null && LevelManager.Instance.CurrentObjectiveMode == LevelManager.ObjectiveMode.PressButtons)
        {
            int pressed = LevelManager.Instance.PressedButtons;
            int required = LevelManager.Instance.RequiredButtons;
            bool complete = pressed >= required;
            objectiveText.text = $"Press the buttons ({pressed}/{required}){(complete ? " <color=green>✓</color>" : "")}\n";
            return;
        }

        string display = "";

        if (initialCounts.ContainsKey("Soldiers"))
        {
            int current = CountActive<SoldierAI>();
            int killed = initialCounts["Soldiers"] - current;
            display += $"Eliminate the soldier ({killed}/{initialCounts["Soldiers"]}){(current <= 0 ? " <color=green>✓</color>" : "")}\n";
        }

        if (CrystalBossController.Instance != null)
        {
            display += CrystalBossController.Instance.GetObjectiveStatusLine() + "\n";
        }

        if (initialCounts.ContainsKey("Crystals"))
        {
            int current = CountActive<CrystalEnemyAI>();
            int destroyed = initialCounts["Crystals"] - current;
            display += $"Destroy the crystal core ({destroyed}/{initialCounts["Crystals"]}){(current <= 0 ? " <color=green>✓</color>" : "")}\n";
        }

        if (initialCounts.ContainsKey("Tentacles"))
        {
            int current = CountActive<TentacleBossAI>();
            display += $"Tentacles remaining: {current}{(current <= 0 ? " <color=green>✓</color>" : "")}\n";
        }

        if (initialCounts.ContainsKey("Owls"))
        {
            int current = CountActive<OwlAI>();
            int killed = initialCounts["Owls"] - current;
            display += $"Eliminate the owl ({killed}/{initialCounts["Owls"]}){(current <= 0 ? " <color=green>✓</color>" : "")}\n";
        }

        if (initialCounts.ContainsKey("Bears"))
        {
            int current = 0;
            foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            {
                if (enemy.enabled && enemy.name.ToLower().Contains("bear")) current++;
            }
            int killed = initialCounts["Bears"] - current;
            display += $"Eliminate the bear ({killed}/{initialCounts["Bears"]}){(current <= 0 ? " <color=green>✓</color>" : "")}\n";
        }

        objectiveText.text = display;
    }
}
