using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectiveHUD : MonoBehaviour
{
    public static ObjectiveHUD Instance { get; private set; }

    public TextMeshProUGUI objectiveText;

    [Header("Typography")]
    public float objectiveFontSize = 28f;
    public string bossStageHeader = "";

    [Header("Enemy Objective")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private string enemyObjectiveLabel = "Eliminate the enemies";

    private const string CharyFontResourcePath = "Fonts/chary___";
    private const string CharyFontAssetPath = "Assets/fonts/chary___.ttf";
    private const string CharySdfAssetPath = "Assets/fonts/Chary SDF.asset";

    private static TMP_FontAsset cachedCharyFont;
    private int initialOwls;
    private int initialBears;
    private int initialEnemyCount;
    private bool objectivesInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyObjectiveTypography();

        if (ShouldWaitForForestActivation())
        {
            ClearText();
            return;
        }

        InitializeObjectives();
        UpdateUI();
    }

    private void Update()
    {
        if (!objectivesInitialized) return;
        UpdateUI();
    }

    private static bool ShouldWaitForForestActivation()
    {
        if (LevelManager.Instance != null &&
            LevelManager.Instance.CurrentObjectiveMode == LevelManager.ObjectiveMode.Forest)
        {
            return !LevelManager.Instance.ObjectivesActive;
        }
        return false;
    }

    public void OnObjectivesActivated()
    {
        InitializeObjectives();
        UpdateUI();
    }

    public void InitializeObjectives()
    {
        initialEnemyCount = GetRemainingEnemyCount();

        int currentOwls = 0;
        OwlAI[] owls = FindObjectsByType<OwlAI>(FindObjectsSortMode.None);
        foreach (var owl in owls)
        {
            if (owl != null && owl.enabled && owl.gameObject.activeInHierarchy)
                currentOwls++;
        }
        initialOwls = currentOwls;

        int currentBears = 0;
        EnemyPatrol[] patrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        foreach (var patrol in patrols)
        {
            if (patrol != null && patrol.enabled && patrol.gameObject.activeInHierarchy)
                currentBears++;
        }
        initialBears = currentBears;

        objectivesInitialized = true;
    }

    public void UpdateUI()
    {
        if (objectiveText == null) return;

        if (ShouldWaitForForestActivation())
        {
            ClearText();
            return;
        }

        if (LevelManager.Instance != null &&
            LevelManager.Instance.CurrentObjectiveMode == LevelManager.ObjectiveMode.PressButtons)
        {
            int pressed = LevelManager.Instance.PressedButtons;
            int required = LevelManager.Instance.RequiredButtons;
            bool complete = pressed >= required;

            objectiveText.text = $"Press the buttons ({pressed}/{required}){(complete ? " <color=green>✓</color>" : "")}";
            return;
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "Forest" || sceneName == "City")
        {
            int currentOwls = 0;
            OwlAI[] owls = FindObjectsByType<OwlAI>(FindObjectsSortMode.None);
            foreach (var owl in owls)
            {
                if (owl != null && owl.enabled && owl.gameObject.activeInHierarchy)
                    currentOwls++;
            }

            int currentBears = 0;
            EnemyPatrol[] patrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
            foreach (var patrol in patrols)
            {
                if (patrol != null && patrol.enabled && patrol.gameObject.activeInHierarchy)
                    currentBears++;
            }

            int defeatedOwls = Mathf.Clamp(initialOwls - currentOwls, 0, initialOwls);
            int defeatedBears = Mathf.Clamp(initialBears - currentBears, 0, initialBears);

            string display = "";
            if (initialOwls > 0)
            {
                display += $"Enemy owl ({defeatedOwls}/{initialOwls}){(currentOwls <= 0 ? " <color=green>✓</color>" : "")}";
            }
            if (initialBears > 0)
            {
                if (!string.IsNullOrEmpty(display)) display += "\n";
                display += $"enemy bear ({defeatedBears}/{initialBears}){(currentBears <= 0 ? " <color=green>✓</color>" : "")}";
            }
            objectiveText.text = display;
            return;
        }

        int currentEnemies = GetRemainingEnemyCount();

        if (!objectivesInitialized)
        {
            InitializeObjectives();
        }

        if (initialEnemyCount <= 0)
        {
            objectiveText.text = "";
            return;
        }

        int defeatedEnemies = initialEnemyCount - currentEnemies;
        defeatedEnemies = Mathf.Clamp(defeatedEnemies, 0, initialEnemyCount);

        string displayText = "";

        if (LevelManager.Instance != null && LevelManager.Instance.bossStageActive && !string.IsNullOrWhiteSpace(bossStageHeader))
        {
            displayText += bossStageHeader + "\n";
        }

        displayText += $"{enemyObjectiveLabel} ({defeatedEnemies}/{initialEnemyCount})";

        if (currentEnemies <= 0)
        {
            displayText += " <color=green>✓</color>";
        }

        objectiveText.text = displayText;
    }

    private int GetRemainingEnemyCount()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        int count = 0;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    private void ClearText()
    {
        if (objectiveText != null)
        {
            objectiveText.text = "";
        }
    }

    private void ApplyObjectiveTypography()
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

    private static TMP_FontAsset GetCharyFont()
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
        {
            sourceFont = AssetDatabase.LoadAssetAtPath<Font>(CharyFontAssetPath);
        }
#endif

        if (sourceFont != null)
        {
            cachedCharyFont = TMP_FontAsset.CreateFontAsset(sourceFont);
        }

        return cachedCharyFont;
    }
}