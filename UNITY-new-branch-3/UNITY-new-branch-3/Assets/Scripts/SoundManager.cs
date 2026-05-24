using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource runSource;

    [Header("Player Clips")]
    public AudioClip background;
    public AudioClip attack1;
    public AudioClip attack2;
    public AudioClip attack3;
    public AudioClip run;
    public AudioClip dash;
    public AudioClip jump;
    public AudioClip ouch;
    public AudioClip playerDeath;

    [Header("Bear Clips")]
    public AudioClip growl;
    public AudioClip bearAttack;

    [Header("Owl Clips")]
    public AudioClip owlHover;
    public AudioClip owlAttack;


    public void PlayMusic()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayRunSFX(AudioClip clip)
    {
        if (!runSource.isPlaying)
        {
            runSource.loop = true;
            runSource.clip = clip;
            runSource.Play();
        }
    
    }

    public void StopRunSFX()
    {
        runSource.Stop();
    }
}
