using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Murat;

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
    
    [Header("UI References")]
    [Tooltip("Game Won UI object to enable when boss is defeated")]
    public GameObject gameWonUI;
    
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
        if (soldier == null) return;
        
        // Kill any existing animations on this soldier first
        soldier.DOKill();
        DOTween.Kill(soldier);
        
        // Subtle breathing/pulsing animation for boss soldiers
        Vector3 originalScale = soldier.localScale;
        string animationId = "boss_idle_" + soldier.GetInstanceID(); // Use unique string ID instead of Transform
        
        soldier.DOScale(originalScale * 1.05f, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(animationId) // Use string ID instead of Transform reference
            .SetTarget(soldier); // Set target for additional safety
    }
    
    // Override the HandleCombat method to add victory check
    protected override IEnumerator HandleCombat()
    {
        // Call the base combat handling first
        yield return StartCoroutine(base.HandleCombat());
        
        // After combat, check if this was a victory against the boss
        CheckForVictoryOrDefeat();
    }
    
    void CheckForVictoryOrDefeat()
    {
        if (hasTriggeredVictory) return; // Already handled
        
        // Check if the boss army is defeated and player army still has units
        if (GetArmySize() == 0 && playerArmy != null && playerArmy.GetArmySize() > 0)
        {
            Debug.Log("Boss defeated! Starting victory sequence...");
            StartCoroutine(HandleVictory());
        }
        // Check if player army is defeated
        else if (playerArmy != null && playerArmy.GetArmySize() == 0)
        {
            Debug.Log("Player defeated by boss! Starting defeat sequence...");
            StartCoroutine(HandleDefeat());
        }
    }
    
    IEnumerator HandleDefeat()
    {
        if (hasTriggeredVictory) yield break; // Prevent multiple triggers
        
        hasTriggeredVictory = true; // Use same flag to prevent multiple defeat triggers
        
        Debug.Log("💀 PLAYER DEFEATED BY BOSS! Defeat sequence starting!");
        
        // Set defeat flag to prevent any further combat (access protected field from base class)
        if (this is EnemyArmy enemyArmy)
        {
            // We need to access the isPlayerDefeated field, but it's private in base class
            // So we'll disable the component instead
        }
        
        // Stop player movement permanently
        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoMove(false); // Stop auto movement
            playerController.SetCombatState(true); // Disable all input
        }
        
        // Kill any DOTween animations on the player before disabling
        if (playerArmy.player != null)
        {
            playerArmy.player.DOKill(); // Kill all tweens on the player transform
            DOTween.Kill(playerArmy.player); // Kill tweens with this transform as ID
            Debug.Log("DOTween animations killed for player");
        }
        
        // Kill all boss-specific DOTween animations before disabling components
        KillAllBossDOTweenAnimations();
        Debug.Log("All boss fight DOTween animations killed before defeat sequence");
        
        // Disable ArmyManager component to prevent any further army operations
        if (playerArmy != null)
        {
            playerArmy.enabled = false;
            Debug.Log("ArmyManager disabled to prevent further operations");
        }
        
        // Clear the player's army
        if (playerArmy != null)
        {
            playerArmy.ClearArmy();
        }
        
        // Show Game Lost UI
        if (gameLostUI != null)
        {
            gameLostUI.SetActive(true);
            Debug.Log("Game Lost UI enabled!");
        }
        else
        {
            Debug.LogWarning("Game Lost UI not assigned in BossFight component!");
        }
        
        // Disable player components and GameObject after DOTween cleanup
        if (playerController != null)
        {
            playerController.enabled = false;
           //playerController.gameObject.SetActive(false);
        }
        
        // Disable this boss fight component as well to stop all operations
        this.enabled = false;
        
        Debug.Log("Boss defeat sequence completed! All combat systems disabled.");
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
        
        // Calculate and add points based on remaining army size
        int remainingArmySize = playerArmy.GetArmySize();
        int pointsToAdd = remainingArmySize; // Points = number of soldiers left (including player)
        
        // Add points to Puan using BellekYonetim
        BellekYonetim bellekYonetim = new BellekYonetim();
        int currentPoints = bellekYonetim.VeriOku_i("Puan");
        int newPoints = currentPoints + pointsToAdd;
        bellekYonetim.VeriKaydet_int("Puan", newPoints);
        
        Debug.Log($"Boss victory reward: +{pointsToAdd} points! Total: {newPoints}");
        
        // Show Game Won UI
        if (gameWonUI != null)
        {
            gameWonUI.SetActive(true);
            Debug.Log("Game Won UI enabled!");
        }
        else
        {
            Debug.LogWarning("Game Won UI not assigned in BossFight component!");
        }
        
        // Kill all boss animations after victory to prevent issues
        KillAllBossDOTweenAnimations();
        Debug.Log("All boss animations killed after victory sequence");
        
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
        
        // Clean up any remaining boss soldier animations before victory
        foreach (Transform enemySoldier in enemySoldiers)
        {
            if (enemySoldier != null)
            {
                enemySoldier.DOKill();
                DOTween.Kill(enemySoldier);
            }
        }
        
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
    
    // Override RemoveEnemySoldier to properly clean up boss animations
    public new void RemoveEnemySoldier(Transform soldier)
    {
        if (enemySoldiers.Contains(soldier))
        {
            // Kill any DOTween animations on this soldier first
            if (soldier != null)
            {
                soldier.DOKill(); // Kill all tweens on this transform
                DOTween.Kill(soldier); // Kill tweens with this transform as ID
            }
            
            enemySoldiers.Remove(soldier);
            
            // Animate death
            if (soldier != null)
            {
                soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    if (soldier != null)
                        Destroy(soldier.gameObject);
                });
            }
        }
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
    
    void OnDestroy()
    {
        // Kill all DOTween animations when this boss fight is destroyed
        KillAllBossDOTweenAnimations();
        Debug.Log("BossFight: All DOTween animations cleaned up on destroy");
    }
    
    // Method to kill all DOTween animations specific to boss fight
    public void KillAllBossDOTweenAnimations()
    {
        Debug.Log("KillAllBossDOTweenAnimations: Starting boss animation cleanup");
        
        // Kill base enemy army animations first
        KillAllEnemyDOTweenAnimations();
        
        // Kill any additional boss-specific animations
        foreach (Transform soldier in enemySoldiers)
        {
            if (soldier != null)
            {
                // Kill boss idle breathing animations that use soldier as ID
                DOTween.Kill(soldier);
                soldier.DOKill();
                
                // Also kill any scale animations or other tweens
                DOTween.Kill(soldier, false); // Kill all tweens targeting this transform
                
                Debug.Log($"Killed DOTween animations for boss soldier: {soldier.name}");
            }
        }
        
        // Kill any animations on the boss fight transform itself
        if (transform != null)
        {
            transform.DOKill();
            DOTween.Kill(transform);
        }
        
        // Kill any victory particle effects or other boss-specific elements
        if (victoryParticleEffect != null)
        {
            DOTween.Kill(victoryParticleEffect.transform);
        }
        
        Debug.Log("BossFight: All boss-specific DOTween animations killed");
    }
}
