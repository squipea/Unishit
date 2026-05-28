using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

/// <summary>
/// Camera shake via Cinemachine impulse (works with CinemachineBrain).
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetupImpulseSource();
        EnsureImpulseListeners();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureImpulseListeners();
    }

    public static CameraShake EnsureExists()
    {
        if (Instance != null)
            return Instance;

        var existing = FindAnyObjectByType<CameraShake>();
        if (existing != null)
            return existing;

        var go = new GameObject("CameraShake");
        return go.AddComponent<CameraShake>();
    }

    void SetupImpulseSource()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();

        impulseSource.ImpulseDefinition.ImpulseChannel = 1;
        impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulseSource.ImpulseDefinition.ImpulseDuration = 0.18f;
        impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulseSource.DefaultVelocity = Vector3.right;
    }

    void EnsureImpulseListeners()
    {
        var cameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var camera in cameras)
        {
            if (camera == null)
                continue;

            if (camera.GetComponent<CinemachineImpulseListener>() == null)
                camera.gameObject.AddComponent<CinemachineImpulseListener>();
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (impulseSource == null)
            return;

        EnsureImpulseListeners();

        impulseSource.ImpulseDefinition.ImpulseDuration = Mathf.Max(0.05f, duration);

        Vector2 dir = Random.insideUnitCircle;
        if (dir.sqrMagnitude < 0.001f)
            dir = Vector2.right;
        dir.Normalize();

        Vector3 velocity = new Vector3(dir.x, dir.y, 0f) * magnitude;
        impulseSource.GenerateImpulseWithVelocity(velocity);
    }
}
