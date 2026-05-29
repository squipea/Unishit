using UnityEngine;
using TMPro;

public class CrystalBossController : MonoBehaviour
{
    public static CrystalBossController Instance { get; private set; }

    public enum BossPhase { Protected, Vulnerable, Victory }

    [Header("Phase Timing")]
    public float vulnerableWindowDuration = 5f;
    public int tentaclesPerWave = 5;

    [Header("References")]
    public TentacleManager tentacleManager;
    public TextMeshProUGUI phaseTimerText;

    public BossPhase CurrentPhase { get; private set; } = BossPhase.Protected;
    public float VulnerableTimeRemaining { get; private set; }

    public bool IsCrystalShielded => CurrentPhase == BossPhase.Protected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tentacleManager == null)
            tentacleManager = FindFirstObjectByType<TentacleManager>();
    }

    void Start()
    {
        if (phaseTimerText == null)
        {
            ObjectiveHUD hud = FindFirstObjectByType<ObjectiveHUD>();
            if (hud != null)
                phaseTimerText = hud.objectiveText;
        }

        EnterProtectedPhase(false, true);
        UpdateTimerUI();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (CurrentPhase == BossPhase.Victory)
            return;

        if (CurrentPhase == BossPhase.Vulnerable)
        {
            VulnerableTimeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (VulnerableTimeRemaining <= 0f)
                EndVulnerableWindow();
        }

        if (CountAliveCrystals() <= 0)
            TriggerVictory();
    }

    public void NotifyTentacleDefeated()
    {
        if (CurrentPhase != BossPhase.Protected)
            return;

        if (CountAliveTentacles() <= 0)
            BeginVulnerableWindow();
    }

    public void NotifyCrystalDestroyed()
    {
        if (CountAliveCrystals() <= 0)
            TriggerVictory();
    }

    void BeginVulnerableWindow()
    {
        CurrentPhase = BossPhase.Vulnerable;
        VulnerableTimeRemaining = vulnerableWindowDuration;
        SetAllCrystalShields(false);
        UpdateTimerUI();

        if (BossBackgroundController.Instance != null)
        {
            BossBackgroundController.Instance.PlayDeathAnimation();
        }

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    void EndVulnerableWindow()
    {
        if (CountAliveCrystals() <= 0)
        {
            TriggerVictory();
            return;
        }

        EnterProtectedPhase(true, false);
    }

    void EnterProtectedPhase(bool spawnWave, bool fillMissingOnStart)
    {
        CurrentPhase = BossPhase.Protected;
        VulnerableTimeRemaining = 0f;
        SetAllCrystalShields(true);
        UpdateTimerUI();

        if (BossBackgroundController.Instance != null && spawnWave)
        {
            BossBackgroundController.Instance.PlayRegenAnimation();
        }

        if (tentacleManager == null)
            return;

        if (spawnWave)
            tentacleManager.SpawnWave(tentaclesPerWave);
        else if (fillMissingOnStart)
            tentacleManager.EnsureInitialWave(tentaclesPerWave);

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    void SetAllCrystalShields(bool active)
    {
        CrystalEnemyAI[] crystals = FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None);
        foreach (CrystalEnemyAI crystal in crystals)
        {
            if (crystal != null && crystal.enabled)
                crystal.SetShieldActive(active);
        }
    }

    void TriggerVictory()
    {
        if (CurrentPhase == BossPhase.Victory)
            return;

        CurrentPhase = BossPhase.Victory;
        VulnerableTimeRemaining = 0f;
        UpdateTimerUI();

        // Keep BOSS DAMAGE AND DIED.mp3 playing permanently
        if (BossBackgroundController.Instance != null)
            BossBackgroundController.Instance.PlayVictoryAudio();

        if (BossCameraController.Instance != null)
            BossCameraController.Instance.ExitBossMode();

        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.CompleteCurrentState();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Credits");
    }

    int CountAliveTentacles()
    {
        int count = 0;
        TentacleBossAI[] tentacles = FindObjectsByType<TentacleBossAI>(FindObjectsSortMode.None);
        foreach (TentacleBossAI tentacle in tentacles)
        {
            if (tentacle != null && tentacle.enabled)
                count++;
        }
        return count;
    }

    int CountAliveCrystals()
    {
        int count = 0;
        CrystalEnemyAI[] crystals = FindObjectsByType<CrystalEnemyAI>(FindObjectsSortMode.None);
        foreach (CrystalEnemyAI crystal in crystals)
        {
            if (crystal != null && crystal.enabled)
                count++;
        }
        return count;
    }

    void UpdateTimerUI()
    {
        if (ObjectiveHUD.Instance != null)
            ObjectiveHUD.Instance.UpdateUI();
    }

    public string GetObjectiveStatusLine()
    {
        if (CurrentPhase == BossPhase.Victory)
            return "<color=green>Boss defeated!</color>";

        if (CurrentPhase == BossPhase.Vulnerable)
            return $"Destroy the crystal! ({Mathf.CeilToInt(VulnerableTimeRemaining)}s)";

        return "Kill all tentacles to expose the crystal";
    }
}
