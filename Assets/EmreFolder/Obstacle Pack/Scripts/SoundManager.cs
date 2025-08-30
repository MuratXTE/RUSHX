using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;
    
    [Header("Math Gate Sounds")]
    [Tooltip("Sound for addition and multiplication operations")]
    public AudioClip positiveGateSound;
    
    [Tooltip("Sound for subtraction and division operations")]
    public AudioClip negativeGateSound;
    
    [Header("Army Sounds")]
    [Tooltip("Sound when a soldier dies")]
    public AudioClip soldierDeathSound;
    
    [Tooltip("Sound when combat starts")]
    public AudioClip combatStartSound;
    
    [Tooltip("Sound during combat attacks")]
    public AudioClip combatAttackSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float gateVolume = 0.7f;
    
    [Range(0f, 1f)]
    public float deathVolume = 0.5f;
    
    // Singleton instance
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
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
    }
    public void PlayPositiveGateSound()
    {
        if (positiveGateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(positiveGateSound, gateVolume);
        }
    }
    public void PlayNegativeGateSound()
    {
        if (negativeGateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(negativeGateSound, gateVolume);
        }
    }
    public void PlaySoldierDeathSound()
    {
        if (soldierDeathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(soldierDeathSound, deathVolume);
        }
    }
    public void PlayCombatStartSound()
    {
        if (combatStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(combatStartSound, gateVolume);
        }
    }
    public void PlayCombatAttackSound()
    {
        if (combatAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(combatAttackSound, deathVolume);
        }
    }
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    public void SetMasterVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}
