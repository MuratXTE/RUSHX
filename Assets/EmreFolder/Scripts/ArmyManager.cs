using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ArmyManager : MonoBehaviour
{
    [Header("Army Settings")]
    public GameObject soldierPrefab;
    public Transform player;
    
    [Header("Formation Settings")]
    public List<int> layerCounts = new List<int> { 1, 6, 12, 18, 24, 30 }; // More circular layer counts
    public float baseLayerRadius = 1.2f; // Base radius for first layer
    public float radiusMultiplier = 1.5f; // How much each layer expands
    public float soldierSpacing = 1f; // Minimum distance between soldiers
    public float reformDelay = 2f; // Time before reforming after death
    
    [Header("Animation Settings")]
    public float spawnAnimationDuration = 0.5f;
    public float reformAnimationDuration = 0.8f;
    public Ease spawnEase = Ease.OutBack;
    public Ease reformEase = Ease.OutQuart;
    public float spawnDelay = 0.05f; // Delay between each soldier spawn
    
    [Header("Effects")]
    public GameObject soldierDeathParticle; // Particle effect when soldier dies
    
    [Header("UI")]
    public TextMeshProUGUI armyCountText; // UI text to display army count
    
    [Header("Movement")]
    public float baseHeight = 0.5f; // Height offset for soldier prefab pivot (if needed)
    
    private List<Transform> soldiers = new List<Transform>();
    private bool isReforming = false;
    private bool isPlayerAlive = true; // Track if player is alive
    
    void Start()
    {
        
        if (player == null)
            player = transform;
            
        // Start with just the player (no initial army)
        // No need to calculate formation initially since we have no soldiers
    }
    
    void Update()
    {
        // No need for update loop since soldiers are children and move with player automatically
        UpdateArmyCountDisplay();
    }
    
    void UpdateArmyCountDisplay()
    {
        if (armyCountText != null)
        {
            int totalArmySize = GetArmySize();
            armyCountText.text = totalArmySize.ToString();
        }
    }
    
    public void AddSoldiers(int count)
    {
        StartCoroutine(SpawnSoldiersWithAnimation(count));
    }
    
    // Method to add an existing soldier transform to the army (for pickup soldiers)
    public void AddExistingSoldier(Transform soldierTransform)
    {
        if (soldierTransform == null) return;
        
        // Make soldier a child of the player
        soldierTransform.SetParent(player);
        
        // Add to soldiers list
        soldiers.Add(soldierTransform);
        
        // Ensure ArmySoldier component has proper reference
        ArmySoldier soldierScript = soldierTransform.GetComponent<ArmySoldier>();
        if (soldierScript == null)
        {
            soldierScript = soldierTransform.gameObject.AddComponent<ArmySoldier>();
        }
        
        soldierScript.armyManager = this;
        soldierScript.canDie = true;
        
        // Apply items to soldier
        StartCoroutine(ApplyItemsToNewSoldier(soldierScript));
        
        Debug.Log($"Added existing soldier to army. New army size: {soldiers.Count + 1}");
        
        // Trigger reformation after a short delay to position the new soldier
        StartCoroutine(DelayedReformation());
    }
    
    private System.Collections.IEnumerator ApplyItemsToNewSoldier(ArmySoldier soldierScript)
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure all components are ready
        if (soldierScript != null)
        {
            soldierScript.ApplyItemsToSoldier();
        }
    }
    
    private System.Collections.IEnumerator DelayedReformation()
    {
        yield return new WaitForSeconds(0.2f);
        PositionSoldiersInFormation();
    }
    
    private IEnumerator SpawnSoldiersWithAnimation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newSoldier = Instantiate(soldierPrefab, player.position, Quaternion.identity);
            
            // Make soldier a child of the player
            newSoldier.transform.SetParent(player);
            
            // Start with zero scale for spawn animation
            newSoldier.transform.localScale = Vector3.zero;
            
            soldiers.Add(newSoldier.transform);
            
            // Add ArmySoldier component if it doesn't exist and set the reference immediately
            ArmySoldier soldierScript = newSoldier.GetComponent<ArmySoldier>();
            if (soldierScript == null)
            {
                soldierScript = newSoldier.AddComponent<ArmySoldier>();
            }
            
            // Ensure the army manager reference is set
            soldierScript.armyManager = this;
            
            // Apply items to soldier after it's created
            // Small delay to ensure all components are initialized
            DOVirtual.DelayedCall(0.1f, () => {
                if (soldierScript != null)
                {
                    soldierScript.ApplyItemsToSoldier();
                }
            });
            
            // Animate soldier spawn with scale and slight bounce
            newSoldier.transform.DOScale(Vector3.one, spawnAnimationDuration)
                .SetEase(spawnEase)
                .OnComplete(() => {
                    // Add a little bounce on land
                    newSoldier.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 1, 0.5f);
                });
            
            Debug.Log($"Added soldier {i+1}/{count}, armyManager reference set: {(soldierScript.armyManager != null)}");
            
            // Wait before spawning next soldier for staggered effect
            yield return new WaitForSeconds(spawnDelay);
        }
        
        // Position soldiers after all are spawned
        yield return new WaitForSeconds(spawnAnimationDuration);
        PositionSoldiersInFormation();
    }
    
    public void RemoveSoldier(Transform soldier, bool spawnParticles = true)
    {
        if (soldiers.Contains(soldier))
        {
            soldiers.Remove(soldier);
            
            Debug.Log($"Soldier removed. Remaining army size: {soldiers.Count + 1}");
            
            // Kill any DOTween animations on this soldier first
            if (soldier != null)
            {
                soldier.DOKill();
            }
            
            // Play soldier death sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySoldierDeathSound();
            }
            
            // Spawn death particle effect at soldier position only if requested
            if (spawnParticles && soldierDeathParticle != null && soldier != null)
            {
                Vector3 worldPosition = soldier.position;
                GameObject deathEffect = Instantiate(soldierDeathParticle, worldPosition, Quaternion.identity);
                
                // Auto-destroy the particle effect after some time
                Destroy(deathEffect, 3f);
            }
            
            // Animate soldier death with scale down and fade
            if (soldier != null && soldier.gameObject != null)
            {
                // Scale down and fade out
                soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                
                // Destroy after animation
                DOVirtual.DelayedCall(0.3f, () => {
                    if (soldier != null && soldier.gameObject != null)
                        Destroy(soldier.gameObject);
                });
            }
            
            // Start reform timer - always reform when a soldier dies, even if it's the last one
            if (!isReforming)
            {
                Debug.Log("Starting army reformation...");
                StartCoroutine(ReformArmy());
            }
            else
            {
                Debug.Log("Army reformation already in progress, skipping...");
            }
        }
        else
        {
            Debug.Log("Soldier not found in army list!");
        }
    }
    
    public void RemoveRandomSoldiers(int count, bool spawnParticles = false)
    {
        Debug.Log($"Removing {count} random soldiers from army (particles: {spawnParticles})");
        
        int soldiersToRemove = Mathf.Min(count, soldiers.Count);
        
        for (int i = 0; i < soldiersToRemove; i++)
        {
            if (soldiers.Count > 0)
            {
                // Pick a random soldier to remove
                int randomIndex = Random.Range(0, soldiers.Count);
                Transform soldierToRemove = soldiers[randomIndex];
                
                // Remove without triggering reformation (we'll do it once at the end)
                soldiers.RemoveAt(randomIndex);
                
                // Kill any DOTween animations on this soldier first
                if (soldierToRemove != null)
                {
                    soldierToRemove.DOKill();
                }
                
                // Play soldier death sound
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySoldierDeathSound();
                }
                
                // Spawn death particle effect only if requested
                if (spawnParticles && soldierDeathParticle != null && soldierToRemove != null)
                {
                    Vector3 worldPosition = soldierToRemove.position;
                    GameObject deathEffect = Instantiate(soldierDeathParticle, worldPosition, Quaternion.identity);
                    Destroy(deathEffect, 3f);
                }
                
                // Animate soldier death with delay for dramatic effect
                if (soldierToRemove != null && soldierToRemove.gameObject != null)
                {
                    float delay = i * 0.1f; // Stagger the deaths
                    
                    // Capture the soldier reference in a local variable to avoid closure issues
                    Transform currentSoldier = soldierToRemove;
                    DOVirtual.DelayedCall(delay, () => {
                        if (currentSoldier != null)
                        {
                            currentSoldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                            DOVirtual.DelayedCall(0.3f, () => {
                                if (currentSoldier != null && currentSoldier.gameObject != null)
                                    Destroy(currentSoldier.gameObject);
                            });
                        }
                    });
                }
            }
        }
        
        // Reform army after all removals
        if (soldiersToRemove > 0 && !isReforming)
        {
            Debug.Log("Starting army reformation after bulk removal...");
            StartCoroutine(ReformArmy());
        }
    }
    
    private IEnumerator ReformArmy()
    {
        isReforming = true;
        Debug.Log($"Reforming army in {reformDelay} seconds...");
        yield return new WaitForSeconds(reformDelay);
        
        Debug.Log("Repositioning soldiers in formation...");
        PositionSoldiersInFormation();
        isReforming = false;
        Debug.Log("Army reformation complete!");
    }
    
    void PositionSoldiersInFormation()
    {
        // Clean up any null references first
        CleanupNullSoldiers();
        
        // Ensure all soldiers have proper army manager reference
        RefreshArmyManagerReferences();
        
        Debug.Log($"Reorganizing {soldiers.Count} soldiers into new formation...");
        
        if (soldiers.Count == 0)
        {
            Debug.Log("No soldiers to position");
            return;
        }
        
        int soldierIndex = 0;
        int currentLayer = 0;
        
        // Create a sequence for smooth reformation animation
        Sequence reformSequence = DOTween.Sequence();
        
        // Reorganize ALL soldiers into new sphere formation based on current count
        // Start from layer 1 since layer 0 (center) is reserved for the player
        while (soldierIndex < soldiers.Count && currentLayer < layerCounts.Count)
        {
            int soldiersInThisLayer = Mathf.Min(layerCounts[currentLayer], soldiers.Count - soldierIndex);
            
            // Calculate radius for this layer - ensures proper spacing
            // Skip layer 0 (center) since that's where the player is
            float radius;
            if (currentLayer == 0)
            {
                // Skip layer 0, use layer 1 radius for first soldiers
                float circumference = soldiersInThisLayer * soldierSpacing;
                float calculatedRadius = circumference / (2f * Mathf.PI);
                float minimumRadius = baseLayerRadius * 1; // Use layer 1 radius
                radius = Mathf.Max(calculatedRadius, minimumRadius);
            }
            else
            {
                // Calculate radius based on soldier spacing to ensure they don't overlap
                float circumference = soldiersInThisLayer * soldierSpacing;
                float calculatedRadius = circumference / (2f * Mathf.PI);
                
                // Use either calculated radius or minimum based on layer
                float minimumRadius = baseLayerRadius * (currentLayer * radiusMultiplier);
                radius = Mathf.Max(calculatedRadius, minimumRadius);
            }
            
            Debug.Log($"Layer {currentLayer}: placing {soldiersInThisLayer} soldiers at radius {radius:F2}");
            
            for (int i = 0; i < soldiersInThisLayer; i++)
            {
                if (soldierIndex < soldiers.Count && soldiers[soldierIndex] != null)
                {
                    // All soldiers get positioned in circles around the player (no center position)
                    float angle = (360f / soldiersInThisLayer) * i;
                    float angleRad = angle * Mathf.Deg2Rad;
                    float x = Mathf.Sin(angleRad) * radius;
                    float z = Mathf.Cos(angleRad) * radius;
                    
                    Vector3 targetLocalPosition = new Vector3(x, 0f, z);
                    
                    // Animate soldier to new position with slight delay per soldier
                    Transform currentSoldier = soldiers[soldierIndex];
                    float animDelay = soldierIndex * 0.02f; // Stagger animation
                    
                    reformSequence.Insert(animDelay, 
                        currentSoldier.DOLocalMove(targetLocalPosition, reformAnimationDuration)
                        .SetEase(reformEase)
                        .OnComplete(() => {
                            // Little bounce when reaching position
                            currentSoldier.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.3f);
                        })
                    );
                    
                    // Also animate rotation
                    reformSequence.Insert(animDelay,
                        currentSoldier.DOLocalRotateQuaternion(Quaternion.identity, reformAnimationDuration * 0.5f)
                        .SetEase(Ease.OutQuart)
                    );
                    
                    Debug.Log($"Soldier {soldierIndex} will move to layer {currentLayer}, position {targetLocalPosition}");
                }
                soldierIndex++;
            }
            
            currentLayer++;
        }
        
        // Handle overflow soldiers with random positions around the outer edge
        while (soldierIndex < soldiers.Count)
        {
            if (soldiers[soldierIndex] != null)
            {
                // Place overflow soldiers in an additional layer beyond the defined layers
                float maxRadius = baseLayerRadius * (layerCounts.Count * radiusMultiplier);
                
                // Create a rough circle for overflow soldiers
                float angle = (360f / (soldiers.Count - soldierIndex)) * (soldierIndex - currentLayer);
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * maxRadius;
                float z = Mathf.Cos(angle * Mathf.Deg2Rad) * maxRadius;
                
                Vector3 targetPosition = new Vector3(x, 0f, z);
                
                Transform currentSoldier = soldiers[soldierIndex];
                float animDelay = soldierIndex * 0.02f;
                
                reformSequence.Insert(animDelay,
                    currentSoldier.DOLocalMove(targetPosition, reformAnimationDuration)
                    .SetEase(reformEase)
                    .OnComplete(() => {
                        currentSoldier.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.3f);
                    })
                );
                
                Debug.Log($"Soldier {soldierIndex} placed in overflow layer at {targetPosition}");
            }
            soldierIndex++;
        }
        
        // Play the entire reformation sequence
        reformSequence.Play();
        
        Debug.Log($"Formation complete: {soldierIndex} soldiers positioned in {currentLayer} layers");
    }
    
    void CleanupNullSoldiers()
    {
        // Remove any null references from the soldiers list
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            if (soldiers[i] == null)
            {
                soldiers.RemoveAt(i);
                Debug.Log("Removed null soldier reference");
            }
        }
    }
    
    // Ensure all soldiers have proper army manager reference
    void RefreshArmyManagerReferences()
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
                if (soldierScript != null)
                {
                    if (soldierScript.armyManager == null)
                    {
                        soldierScript.armyManager = this;
                        Debug.Log($"Refreshed army manager reference for soldier: {soldier.name}");
                    }
                }
            }
        }
    }
    
    public int GetArmySize()
    {
        CleanupNullSoldiers(); // Clean up before counting
        return soldiers.Count + (isPlayerAlive ? 1 : 0); // +1 for the player only if alive
    }
    
    // Method to mark player as dead (for combat scenarios)
    public void SetPlayerDead()
    {
        isPlayerAlive = false;
        Debug.Log("Player marked as dead. Army size now: " + GetArmySize());
    }
    
    // Method to revive player (for respawn scenarios)
    public void SetPlayerAlive()
    {
        isPlayerAlive = true;
        Debug.Log("Player marked as alive. Army size now: " + GetArmySize());
    }
    
    // Check if player is alive
    public bool IsPlayerAlive()
    {
        return isPlayerAlive;
    }
    
    // Get available soldiers for combat (excludes player, player fights last)
    public List<Transform> GetAvailableSoldiers()
    {
        CleanupNullSoldiers();
        return new List<Transform>(soldiers); // Return copy of soldiers list (player not included)
    }
    
    // Public method to manually trigger reformation (useful for testing)
    [ContextMenu("Force Army Reformation")]
    public void ForceReformation()
    {
        PositionSoldiersInFormation();
    }
    
    // Test method to refresh all army manager references
    [ContextMenu("Refresh Army Manager References")]
    public void ForceRefreshReferences()
    {
        RefreshArmyManagerReferences();
        Debug.Log($"Refreshed references for {soldiers.Count} soldiers");
    }
    
    // Test method to simulate soldier death and reformation
    [ContextMenu("Test Remove Random Soldier")]
    public void TestRemoveRandomSoldier()
    {
        if (soldiers.Count > 0)
        {
            int randomIndex = Random.Range(0, soldiers.Count);
            Transform soldierToRemove = soldiers[randomIndex];
            Debug.Log($"Testing: Removing soldier at index {randomIndex}");
            RemoveSoldier(soldierToRemove);
        }
        else
        {
            Debug.Log("No soldiers to remove!");
        }
    }
    
    public void ClearArmy()
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                // Kill DOTween animations before destroying
                soldier.DOKill();
                DOTween.Kill(soldier);
                Destroy(soldier.gameObject);
            }
        }
        soldiers.Clear();
        
        // Reset player status when clearing army
        isPlayerAlive = true;
        Debug.Log("Army cleared and player status reset to alive");
    }
    
    void OnDestroy()
    {
        // Kill all DOTween animations on this GameObject and its children when destroyed
        if (transform != null)
        {
            transform.DOKill();
            DOTween.Kill(transform);
        }
        
        // Kill animations on all soldiers
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.DOKill();
                DOTween.Kill(soldier);
            }
        }
        
        // Kill animations on player if it exists
        if (player != null)
        {
            player.DOKill();
            DOTween.Kill(player);
        }
        
        Debug.Log("ArmyManager: All DOTween animations cleaned up on destroy");
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // Kill all DOTween animations when app is paused (mobile)
        if (pauseStatus)
        {
            KillAllDOTweenAnimations();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // Kill all DOTween animations when app loses focus
        if (!hasFocus)
        {
            KillAllDOTweenAnimations();
        }
    }
    
    // Method to kill all DOTween animations on this army
    public void KillAllDOTweenAnimations()
    {
        if (transform != null)
        {
            transform.DOKill();
            DOTween.Kill(transform);
        }
        
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                soldier.DOKill();
                DOTween.Kill(soldier);
            }
        }
        
        if (player != null)
        {
            player.DOKill();
            DOTween.Kill(player);
        }
        
        Debug.Log("ArmyManager: All DOTween animations killed manually");
    }
    
    // Method to trigger victory animations for all soldiers
    public void PlayVictoryAnimation(float animationDuration = 1f, float bounceScale = 1.3f)
    {
        Debug.Log($"Playing victory animation for {soldiers.Count} soldiers + player!");
        
        // Clean up the soldiers list first
        CleanupNullSoldiers();
        
        // Trigger victory animation on all soldiers
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] != null && soldiers[i].gameObject != null)
            {
                // Kill any existing animations on this soldier first
                soldiers[i].DOKill();
                
                Animator soldierAnimator = soldiers[i].GetComponent<Animator>();
                if (soldierAnimator == null)
                {
                    soldierAnimator = soldiers[i].GetComponentInChildren<Animator>();
                }
                
                if (soldierAnimator != null)
                {
                    soldierAnimator.SetTrigger("victory");
                }
                else
                {
                    Debug.LogWarning($"No Animator found on soldier: {soldiers[i].name}");
                }
            }
        }
        
        // Trigger victory animation on player too
        if (player != null && player.gameObject != null)
        {
            // Kill any existing animations on player first
            player.DOKill();
            
            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = player.GetComponentInChildren<Animator>();
            }
            
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("victory");
            }
            else
            {
                Debug.LogWarning($"No Animator found on player: {player.name}");
            }
        }
    }
}
