using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class EnemyArmy : MonoBehaviour
{
    [Header("Enemy Army Settings")]
    public GameObject enemySoldierPrefab;
    public int initialSoldierCount = 10;
    public bool spawnOnStart = true;
    
    [Header("Formation Settings")]
    public List<int> layerCounts = new List<int> { 1, 6, 12, 18, 24, 30 };
    public float baseLayerRadius = 1.2f;
    public float radiusMultiplier = 1.5f;
    public float soldierSpacing = 1f;
    
    [Header("Combat Settings")]
    public float combatTriggerDistance = 5f;
    public float soldierMoveSpeed = 3f;
    public float attackAnimationDuration = 1f;
    
    [Header("Visual Settings")]
    public TextMeshPro armyCountText;
    public Color enemyColor = Color.red;
    
    [Header("Effects")]
    public GameObject combatParticleEffect;
    
    protected List<Transform> enemySoldiers = new List<Transform>();
    private bool isInCombat = false;
    protected ArmyManager playerArmy;
    
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemyArmy();
        }
        
        // Find player army in the scene
        playerArmy = FindFirstObjectByType<ArmyManager>();
        
        UpdateArmyDisplay();
    }
    
    void Update()
    {
        if (!isInCombat && playerArmy != null)
        {
            CheckForCombat();
        }
        
        UpdateArmyDisplay();
    }
    
    protected virtual void SpawnEnemyArmy()
    {
        for (int i = 0; i < initialSoldierCount; i++)
        {
            GameObject enemySoldier = Instantiate(enemySoldierPrefab, transform.position, Quaternion.identity);
            enemySoldier.transform.SetParent(transform);
            
            // Set enemy soldier color
            Renderer renderer = enemySoldier.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = enemyColor;
            }
            
            // Add EnemySoldier component
            EnemySoldier soldierScript = enemySoldier.GetComponent<EnemySoldier>();
            if (soldierScript == null)
            {
                soldierScript = enemySoldier.AddComponent<EnemySoldier>();
            }
            soldierScript.enemyArmy = this;
            
            enemySoldiers.Add(enemySoldier.transform);
        }
        
        PositionSoldiersInFormation();
    }
    
    void PositionSoldiersInFormation()
    {
        CleanupNullSoldiers();
        
        if (enemySoldiers.Count == 0) return;
        
        int soldierIndex = 0;
        int currentLayer = 0;
        
        while (soldierIndex < enemySoldiers.Count && currentLayer < layerCounts.Count)
        {
            int soldiersInThisLayer = Mathf.Min(layerCounts[currentLayer], enemySoldiers.Count - soldierIndex);
            
            float radius;
            if (currentLayer == 0)
            {
                float circumference = soldiersInThisLayer * soldierSpacing;
                float calculatedRadius = circumference / (2f * Mathf.PI);
                radius = Mathf.Max(calculatedRadius, baseLayerRadius);
            }
            else
            {
                float circumference = soldiersInThisLayer * soldierSpacing;
                float calculatedRadius = circumference / (2f * Mathf.PI);
                float minimumRadius = baseLayerRadius * (currentLayer * radiusMultiplier);
                radius = Mathf.Max(calculatedRadius, minimumRadius);
            }
            
            for (int i = 0; i < soldiersInThisLayer; i++)
            {
                if (soldierIndex < enemySoldiers.Count && enemySoldiers[soldierIndex] != null)
                {
                    float angle = (360f / soldiersInThisLayer) * i;
                    float angleRad = angle * Mathf.Deg2Rad;
                    float x = Mathf.Sin(angleRad) * radius;
                    float z = Mathf.Cos(angleRad) * radius;
                    
                    Vector3 targetLocalPosition = new Vector3(x, 0f, z);
                    enemySoldiers[soldierIndex].localPosition = targetLocalPosition;
                }
                soldierIndex++;
            }
            currentLayer++;
        }
    }
    
    void CheckForCombat()
    {
        if (playerArmy == null || playerArmy.player == null) return;
        
        float distance = Vector3.Distance(transform.position, playerArmy.player.position);
        
        if (distance <= combatTriggerDistance && GetArmySize() > 0 && playerArmy.GetArmySize() > 0) // Combat can start if both armies have units
        {
            StartCombat();
        }
    }
    
    void StartCombat()
    {
        if (isInCombat) return;
        
        isInCombat = true;
        
        Debug.Log($"Combat started! Player army: {playerArmy.GetArmySize()}, Enemy army: {GetArmySize()}");
        
        // Play combat start sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCombatStartSound();
        }
        
        // Stop player movement and input
        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCombatState(true);
        }
        
        StartCoroutine(HandleCombat());
    }
    
    protected virtual IEnumerator HandleCombat()
    {
        // Get available soldiers and determine if player needs to fight
        List<Transform> playerSoldiers = playerArmy.GetAvailableSoldiers();
        List<Transform> enemySoldiers = GetAvailableSoldiers();
        
        int enemyCount = enemySoldiers.Count;
        int playerSoldierCount = playerSoldiers.Count;
        bool playerNeedsToFight = enemyCount >= playerArmy.GetArmySize(); // Enemy >= total player army (soldiers + player)
        
        Debug.Log($"Combat analysis: Player total army: {playerArmy.GetArmySize()} (soldiers: {playerSoldierCount} +1 player), Enemy: {enemyCount}");
        Debug.Log($"Player needs to fight: {playerNeedsToFight} (enemy {enemyCount} >= player total {playerArmy.GetArmySize()})");
        
        // Determine how many pairs will fight
        int fightingPairs;
        if (playerNeedsToFight)
        {
            // Player joins the fight - total available units = soldiers + player
            fightingPairs = Mathf.Min(playerSoldierCount + 1, enemyCount); // +1 for player
        }
        else
        {
            // Only soldiers fight, player stays safe
            fightingPairs = Mathf.Min(playerSoldierCount, enemyCount);
        }
        
        Debug.Log($"Fighting pairs: {fightingPairs}, Player needs to fight: {playerNeedsToFight}");
        
        if (fightingPairs > 0)
        {
            // Create fighting pairs
            List<CombatPair> combatPairs = new List<CombatPair>();
            
            // First, pair up soldiers
            int soldierPairs = Mathf.Min(playerSoldierCount, fightingPairs);
            for (int i = 0; i < soldierPairs; i++)
            {
                CombatPair pair = new CombatPair
                {
                    playerSoldier = playerSoldiers[i],
                    enemySoldier = enemySoldiers[i],
                    isPlayerInvolved = false
                };
                combatPairs.Add(pair);
            }
            
            // If player needs to fight and there are remaining enemies
            if (playerNeedsToFight && fightingPairs > soldierPairs && soldierPairs < enemyCount)
            {
                CombatPair playerPair = new CombatPair
                {
                    playerSoldier = playerArmy.player, // Player himself fights
                    enemySoldier = enemySoldiers[soldierPairs], // Next available enemy
                    isPlayerInvolved = true
                };
                combatPairs.Add(playerPair);
            }
            
            // Move soldiers to combat positions and fight
            yield return StartCoroutine(ExecuteCombat(combatPairs));
        }
        
        // Combat finished
        isInCombat = false;
        
        // Resume player movement and input
        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCombatState(false);
        }
        
        // Reform remaining soldiers
        playerArmy.ForceReformation();
        PositionSoldiersInFormation();
        
        Debug.Log("Combat finished!");
    }
    
    IEnumerator ExecuteCombat(List<CombatPair> combatPairs)
    {
        // Move soldiers towards each other
        List<Tween> moveTweens = new List<Tween>();
        
        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null && pair.enemySoldier != null)
            {
                Vector3 playerPos = pair.playerSoldier.position;
                Vector3 enemyPos = pair.enemySoldier.position;
                Vector3 midPoint = (playerPos + enemyPos) / 2f;
                
                // Move both soldiers to midpoint
                Tween playerMove = pair.playerSoldier.DOMove(midPoint + Vector3.left * 0.3f, soldierMoveSpeed).SetSpeedBased();
                Tween enemyMove = pair.enemySoldier.DOMove(midPoint + Vector3.right * 0.3f, soldierMoveSpeed).SetSpeedBased();
                
                moveTweens.Add(playerMove);
                moveTweens.Add(enemyMove);
            }
        }
        
        // Wait for all soldiers to actually reach their combat positions
        if (moveTweens.Count > 0)
        {
            // Create a sequence and add all move tweens to it
            Sequence moveSequence = DOTween.Sequence();
            foreach (var tween in moveTweens)
            {
                moveSequence.Join(tween);
            }
            yield return moveSequence.WaitForCompletion();
        }
        
        // Play attack animations and effects
        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null && pair.enemySoldier != null)
            {
                // Trigger attack animations on Animator controllers
                Animator playerAnimator = pair.playerSoldier.GetComponentInChildren<Animator>();
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger("attack");
                }
                
                Animator enemyAnimator = pair.enemySoldier.GetComponentInChildren<Animator>();
                if (enemyAnimator != null)
                {
                    enemyAnimator.SetTrigger("attack");
                }
                
                // Spawn combat particle effect
                if (combatParticleEffect != null)
                {
                    Vector3 effectPos = (pair.playerSoldier.position + pair.enemySoldier.position) / 2f;
                    GameObject effect = Instantiate(combatParticleEffect, effectPos, Quaternion.identity);
                    
                    // Make effect more dramatic if player is involved
                    if (pair.isPlayerInvolved)
                    {
                        effect.transform.localScale *= 1.5f; // Bigger effect for player combat
                    }
                }
            }
        }
        
        // Play combat attack sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCombatAttackSound();
        }
        
        // Wait for attack animation to finish
        yield return new WaitForSeconds(attackAnimationDuration);
        
        // Remove both soldiers from combat
        foreach (var pair in combatPairs)
        {
            if (pair.enemySoldier != null)
            {
                RemoveEnemySoldier(pair.enemySoldier);
            }
            
            if (pair.playerSoldier != null)
            {
                if (pair.isPlayerInvolved)
                {
                    // Player dies - handle game over or respawn logic
                    Debug.Log("Player died in combat!");
                    HandlePlayerDeath();
                }
                else
                {
                    // Regular soldier dies
                    playerArmy.RemoveSoldier(pair.playerSoldier, false); // No particles for combat deaths
                }
            }
        }
    }
    
    public void RemoveEnemySoldier(Transform soldier)
    {
        if (enemySoldiers.Contains(soldier))
        {
            enemySoldiers.Remove(soldier);
            
            // Animate death
            soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                if (soldier != null)
                    Destroy(soldier.gameObject);
            });
        }
    }
    
    void HandlePlayerDeath()
    {
        // Handle player death in combat
        Debug.Log("Player died in combat - implementing game over logic");
        
        // You can implement game over logic here, such as:
        // - Show game over screen
        // - Restart level
        // - Reset player position and clear army
        
        // For now, let's reset player and clear army
        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetPosition();
        }
        
        // Clear the army
        if (playerArmy != null)
        {
            playerArmy.ClearArmy();
        }
        
        // You might want to trigger a game over screen or restart here
    }
    
    List<Transform> GetAvailableSoldiers()
    {
        CleanupNullSoldiers();
        return new List<Transform>(enemySoldiers);
    }
    
    void CleanupNullSoldiers()
    {
        for (int i = enemySoldiers.Count - 1; i >= 0; i--)
        {
            if (enemySoldiers[i] == null)
            {
                enemySoldiers.RemoveAt(i);
            }
        }
    }
    
    public int GetArmySize()
    {
        CleanupNullSoldiers();
        return enemySoldiers.Count;
    }
    
    void UpdateArmyDisplay()
    {
        if (armyCountText != null)
        {
            armyCountText.text = GetArmySize().ToString();
        }
    }
}

[System.Serializable]
public class CombatPair
{
    public Transform playerSoldier;
    public Transform enemySoldier;
    public bool isPlayerInvolved = false; // True if playerSoldier is actually the player character
}
