using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class BossBackgroundController : MonoBehaviour
{
    public static BossBackgroundController Instance { get; private set; }

    [Header("Sprites")]
    public Sprite[] idleSprites;    // Boss bg 1 - 17
    public Sprite[] damageSprites;  // Boss Damaged 1 - 12
    public Sprite[] deathSprites;   // Boss bg 18 - 20 (will stay at 20)
    public Sprite[] regenSprites;   // Boss bg 20 - 22 (then back to idle loop)

    [Header("Audio")]
    public AudioClip bossIdleClip;           // BOSS IDLE.mp3
    public AudioClip bossDamageAndDiedClip;  // BOSS DAMAGED.mp3

    [Header("Settings")]
    public float frameRate = 0.1f; // Speed of animation in seconds per frame

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Coroutine activeAnimCoroutine;
    
    public enum AnimState { Idle, Damaging, Dying, Dead, Regenerating }
    public AnimState currentState = AnimState.Idle;
    private bool bossDefeated = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set up AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            PlayIdle(); // Start sprite animation, but NOT audio yet
        }
    }

    /// <summary>Call this when the boss camera activates to start boss idle music.</summary>
    public void StartBossMusic()
    {
        PlayAudio(bossIdleClip);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

#if UNITY_EDITOR
    private double lastEditorUpdateTime;
    private int editModeFrameIndex = 0;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            lastEditorUpdateTime = UnityEditor.EditorApplication.timeSinceStartup;
            UnityEditor.EditorApplication.update += EditorUpdate;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.update -= EditorUpdate;
        }
    }

    private void EditorUpdate()
    {
        if (Application.isPlaying) return;

        double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
        if (currentTime - lastEditorUpdateTime >= frameRate)
        {
            lastEditorUpdateTime = currentTime;
            AnimateNextFrameEditMode();
        }
    }

    private void AnimateNextFrameEditMode()
    {
        Sprite[] sprites = null;
        switch (currentState)
        {
            case AnimState.Idle: sprites = idleSprites; break;
            case AnimState.Damaging: sprites = damageSprites; break;
            case AnimState.Dying: sprites = deathSprites; break;
            case AnimState.Regenerating: sprites = regenSprites; break;
        }

        if (sprites != null && sprites.Length > 0)
        {
            editModeFrameIndex = (editModeFrameIndex + 1) % sprites.Length;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprites[editModeFrameIndex];
                UnityEditor.EditorUtility.SetDirty(spriteRenderer);
            }
        }
    }
#endif

    public void PlayIdle()
    {
        if (currentState == AnimState.Dying || currentState == AnimState.Dead || currentState == AnimState.Regenerating)
            return;

        StopCurrentAnimation();
        currentState = AnimState.Idle;
        if (Application.isPlaying)
        {
            activeAnimCoroutine = StartCoroutine(LoopAnimation(idleSprites));
        }
    }

    public void PlayDamageAnimation()
    {
        // Damage can interrupt Idle, but shouldn't break Death or Regenerating transitions
        if (currentState == AnimState.Dying || currentState == AnimState.Dead || currentState == AnimState.Regenerating)
            return;

        StopCurrentAnimation();
        currentState = AnimState.Damaging;
        activeAnimCoroutine = StartCoroutine(PlayOnceAndThen(damageSprites, () => {
            currentState = AnimState.Idle;
            PlayIdle();
        }));
    }

    public void PlayDeathAnimation()
    {
        StopCurrentAnimation();
        currentState = AnimState.Dying;
        PlayAudio(bossDamageAndDiedClip);
        activeAnimCoroutine = StartCoroutine(PlayOnceAndThen(deathSprites, () => {
            currentState = AnimState.Dead;
            // Hold on the last frame of death animation (Boss bg 20)
            if (deathSprites != null && deathSprites.Length > 0 && spriteRenderer != null)
            {
                spriteRenderer.sprite = deathSprites[deathSprites.Length - 1];
            }
        }));
    }

    public void PlayRegenAnimation()
    {
        if (bossDefeated) return; // Don't regen music if boss is fully dead
        StopCurrentAnimation();
        currentState = AnimState.Regenerating;
        PlayAudio(bossIdleClip); // Switch back to idle music as tentacles respawn
        activeAnimCoroutine = StartCoroutine(PlayOnceAndThen(regenSprites, () => {
            currentState = AnimState.Idle;
            PlayIdle();
        }));
    }

    /// <summary>Call this when all crystals are destroyed - keeps damage track playing permanently.</summary>
    public void PlayVictoryAudio()
    {
        bossDefeated = true;
        PlayAudio(bossDamageAndDiedClip);
    }

    private void PlayAudio(AudioClip clip)
    {
        if (!Application.isPlaying || audioSource == null || clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return; // Already playing this clip
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void StopCurrentAnimation()
    {
        if (activeAnimCoroutine != null)
        {
            StopCoroutine(activeAnimCoroutine);
            activeAnimCoroutine = null;
        }
    }

    private IEnumerator LoopAnimation(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) yield break;
        int index = 0;
        while (true)
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = sprites[index];
            index = (index + 1) % sprites.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    private IEnumerator PlayOnceAndThen(Sprite[] sprites, System.Action onComplete)
    {
        if (sprites == null || sprites.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        foreach (Sprite sprite in sprites)
        {
            if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;
            yield return new WaitForSeconds(frameRate);
        }

        onComplete?.Invoke();
    }
}
