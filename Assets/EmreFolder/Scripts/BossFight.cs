using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BossFight : EnemyArmy
{
    [Header("Boss Fight Settings")]
    [Tooltip("Special effects for boss victory")]
    public GameObject victoryParticleEffect;
    
    [Tooltip("Duration of victory celebration")]
    public float victoryCelebrationDuration = 3f;
    
    [Tooltip("Scale factor for victory bounce animation")]
    public float victoryBounceScale = 1.3f;
    
    [Tooltip("Duration of individual victory animation")]
    public float victoryAnimationDuration = 0.8f;
    
    [Tooltip("Should the level/game end after victory?")]
    public bool endLevelAfterVictory = false;
    
    [Header("Boss Visual Settings")]
    [Tooltip("Special color for boss army")]
    public Color bossColor = Color.magenta;
    
    [Tooltip("Scale multiplier for boss soldiers")]
    public float bossSoldierScale = 1.2f;
    
    [Tooltip("Special material for boss soldiers")]
    public Material bossMaterial;
    
    [Header("Victory Sound Effects")]
    [Tooltip("Victory sound to play when boss is defeated")]
    public AudioClip victorySoundClip;
    
    private bool hasTriggeredVictory = false;
    
    void Start()
    {
        // Override the enemy color with boss color
        enemyColor = bossColor;
        
        // Replicate the parent Start() logic since we can't call base.Start()
        if (spawnOnStart)
        {
            SpawnEnemyArmy();
        }
        
        // Find player army in the scene
        playerArmy = FindFirstObjectByType<ArmyManager>();
        
        // Update army display (replicate UpdateArmyDisplay functionality)
        if (armyCountText != null)
        {
            armyCountText.text = GetArmySize().ToString();
        }
        
        // Apply boss-specific modifications to existing soldiers
        ApplyBossModifications();
    }
    
    void ApplyBossModifications()
    {
        // Access the protected enemySoldiers field directly
        foreach (Transform soldier in enemySoldiers)
        {
            if (soldier != null)
            {
                // Apply boss scale
                soldier.localScale = Vector3.one * bossSoldierScale;
                
                // Apply boss material if available
                if (bossMaterial != null)
                {
                    Renderer renderer = soldier.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = bossMaterial;
                    }
                }
                else
                {
                    // Apply boss color
                    Renderer renderer = soldier.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = bossColor;
                    }
                }
                
                // Add a subtle glow effect or breathing animation to make them feel more "boss-like"
                StartBossIdleAnimation(soldier);
            }
        }
        
        Debug.Log("Boss modifications applied to army!");
    }
    
    void StartBossIdleAnimation(Transform soldier)
    {
        // Subtle breathing/pulsing animation for boss soldiers
        Vector3 originalScale = soldier.localScale;
        soldier.DOScale(originalScale * 1.05f, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    // Override the HandleCombat method to add victory check
    protected override IEnumerator HandleCombat()
    {
        // Call the base combat handling first
        yield return StartCoroutine(base.HandleCombat());
        
        // After combat, check if this was a victory against the boss
        CheckForVictory();
    }
    
    void CheckForVictory()
    {
        // Check if the boss army is defeated and player army still has units
        if (!hasTriggeredVictory && GetArmySize() == 0 && playerArmy != null && playerArmy.GetArmySize() > 0)
        {
            Debug.Log("Boss defeated! Starting victory sequence...");
            StartCoroutine(HandleVictory());
        }
    }
    
    IEnumerator HandleVictory()
    {
        if (hasTriggeredVictory) yield break;
        
        hasTriggeredVictory = true;
        
        Debug.Log("🎉 BOSS DEFEATED! Victory sequence starting!");
        
        // Stop player movement permanently
        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoMove(false); // Stop auto movement
            playerController.SetCombatState(true); // Disable all input
        }
        
        // Play victory sound
        PlayVictorySound();
        
        // Spawn victory particle effects at boss location
        if (victoryParticleEffect != null)
        {
            GameObject victoryEffect = Instantiate(victoryParticleEffect, transform.position, Quaternion.identity);
            Destroy(victoryEffect, victoryCelebrationDuration);
        }
        
        // Get all army soldiers for victory animation
        List<Transform> victorySoldiers = playerArmy.GetAvailableSoldiers();
        
        // Add the player to the victory celebration
        if (playerArmy.player != null)
        {
            victorySoldiers.Add(playerArmy.player);
        }
        
        // Play victory animations for all soldiers
        yield return StartCoroutine(PlayVictoryAnimations(victorySoldiers));
        
        // Optional: End level or trigger next sequence
        if (endLevelAfterVictory)
        {
            yield return new WaitForSeconds(1f);
            HandleLevelComplete();
        }
        
        Debug.Log("Victory sequence completed!");
    }
    
    IEnumerator PlayVictoryAnimations(List<Transform> soldiers)
    {
        Debug.Log($"Playing victory animations for army!");
        
        // Use the army manager's built-in victory animation
        if (playerArmy != null)
        {
            playerArmy.PlayVictoryAnimation(victoryAnimationDuration, victoryBounceScale);
        }
        
        // Wait for the animations to complete
        yield return new WaitForSeconds(victoryCelebrationDuration);
        
        Debug.Log("All victory animations completed!");
    }
    
    void PlayVictorySound()
    {
        if (SoundManager.Instance != null)
        {
            if (victorySoundClip != null)
            {
                // Play custom victory sound if available
                SoundManager.Instance.PlaySound(victorySoundClip, 1f);
            }
            else
            {
                // Use existing positive sound as fallback
                SoundManager.Instance.PlayPositiveGateSound();
            }
        }
    }
    
    void HandleLevelComplete()
    {
        Debug.Log("Level complete! Implement level transition logic here.");
        
        // You can implement level completion logic here such as:
        // - Show victory screen
        // - Load next level
        // - Save progress
        // - Show rewards/stats
        
        // Example: Load next scene (uncomment if you want to use it)
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NextLevel");
    }
    
    // Override SpawnEnemyArmy to apply boss modifications to new soldiers
    protected override void SpawnEnemyArmy()
    {
        // Call base spawn method
        base.SpawnEnemyArmy();
        
        // Apply boss modifications to newly spawned soldiers
        ApplyBossModifications();
    }
    
    // Public method to manually trigger victory (for testing)
    [ContextMenu("Force Boss Victory")]
    public void ForceVictory()
    {
        if (!hasTriggeredVictory)
        {
            StartCoroutine(HandleVictory());
        }
    }
    
    // Method to check if this boss fight has been completed
    public bool IsVictoryAchieved()
    {
        return hasTriggeredVictory;
    }
}
