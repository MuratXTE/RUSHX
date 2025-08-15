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
    
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float gateVolume = 0.7f;
    
    [Range(0f, 1f)]
    public float deathVolume = 0.5f;
    
    // Singleton instance
    public static SoundManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton pattern
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
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }
    
    /// <summary>
    /// Play sound for positive math operations (addition, multiplication)
    /// </summary>
    public void PlayPositiveGateSound()
    {
        if (positiveGateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(positiveGateSound, gateVolume);
        }
    }
    
    /// <summary>
    /// Play sound for negative math operations (subtraction, division)
    /// </summary>
    public void PlayNegativeGateSound()
    {
        if (negativeGateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(negativeGateSound, gateVolume);
        }
    }
    
    /// <summary>
    /// Play sound when a soldier dies
    /// </summary>
    public void PlaySoldierDeathSound()
    {
        if (soldierDeathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(soldierDeathSound, deathVolume);
        }
    }
    
    /// <summary>
    /// Play a custom sound with specified volume
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    /// <summary>
    /// Set master volume for all sounds
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}
