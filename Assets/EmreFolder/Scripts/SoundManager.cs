using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Math Gate Sounds")]
    public AudioClip positiveGateSound;   // Addition, multiplication
    public AudioClip negativeGateSound;   // Subtraction, division

    [Header("Army Sounds")]
    public AudioClip soldierDeathSound;
    public AudioClip combatStartSound;
    public AudioClip combatAttackSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float gateVolume = 0.7f;
    [Range(0f, 1f)] public float deathVolume = 0.5f;

    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public void PlayPositiveGateSound()
    {
        if (positiveGateSound != null)
            audioSource.PlayOneShot(positiveGateSound, gateVolume);
    }

    public void PlayNegativeGateSound()
    {
        if (negativeGateSound != null)
            audioSource.PlayOneShot(negativeGateSound, gateVolume);
    }

    public void PlaySoldierDeathSound()
    {
        if (soldierDeathSound != null)
            audioSource.PlayOneShot(soldierDeathSound, deathVolume);
    }

    public void PlayCombatStartSound()
    {
        if (combatStartSound != null)
            audioSource.PlayOneShot(combatStartSound, gateVolume);
    }

    public void PlayCombatAttackSound()
    {
        if (combatAttackSound != null)
            audioSource.PlayOneShot(combatAttackSound, deathVolume);
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }

    public void SetMasterVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}
