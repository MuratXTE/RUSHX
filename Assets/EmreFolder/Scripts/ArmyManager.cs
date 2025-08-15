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
                    
                    DOVirtual.DelayedCall(delay, () => {
                        if (soldierToRemove != null)
                        {
                            soldierToRemove.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                            DOVirtual.DelayedCall(0.3f, () => {
                                if (soldierToRemove != null && soldierToRemove.gameObject != null)
                                    Destroy(soldierToRemove.gameObject);
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
        return soldiers.Count +1; // +1 for the player
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
                Destroy(soldier.gameObject);
        }
        soldiers.Clear();
    }
}
