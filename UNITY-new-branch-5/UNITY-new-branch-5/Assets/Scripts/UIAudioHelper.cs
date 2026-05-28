using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIAudioHelper : MonoBehaviour
{
    public static UIAudioHelper Instance { get; private set; }

    [SerializeField] AudioClip clickClip;
    [SerializeField] AudioClip menuMusicClip;
    readonly HashSet<Button> wiredButtons = new HashSet<Button>();
    [SerializeField] [Range(0f, 1f)] float musicVolume = 0.45f;
    [SerializeField] [Range(0f, 1f)] float sfxVolume = 0.85f;

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            PlayMenuMusic();
        else
            StopMenuMusic();
    }

    public void PlayMenuMusic()
    {
        if (menuMusicClip == null || musicSource == null)
            return;

        if (musicSource.clip != menuMusicClip)
        {
            musicSource.clip = menuMusicClip;
            musicSource.Play();
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMenuMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayClick()
    {
        if (clickClip != null && sfxSource != null)
            sfxSource.PlayOneShot(clickClip);
    }

    public void Configure(AudioClip music, AudioClip click)
    {
        if (music != null)
            menuMusicClip = music;
        if (click != null)
            clickClip = click;
    }

    public void WireButtonsInChildren()
    {
        WireButtonsUnder(transform);
    }

    public void WireButtonsUnder(Transform root)
    {
        if (root == null)
            return;

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null || !wiredButtons.Add(button))
                continue;

            button.onClick.AddListener(PlayClick);
        }
    }
}
