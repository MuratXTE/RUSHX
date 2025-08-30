using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ArmyManager : MonoBehaviour
{
    [Header("Army Settings")]
    public GameObject soldierPrefab;
    public Transform player;

    [Header("Formation Settings")]
    public List<int> layerCounts = new List<int> { 1, 6, 12, 18, 24, 30 };
    public float baseLayerRadius = 1.2f;
    public float radiusMultiplier = 1.5f;
    public float soldierSpacing = 1f;
    public float reformDelay = 2f;

    [Header("Animation Settings")]
    public float spawnAnimationDuration = 0.5f;
    public float reformAnimationDuration = 0.8f;
    public Ease spawnEase = Ease.OutBack;
    public Ease reformEase = Ease.OutQuart;
    public float spawnDelay = 0.05f;

    [Header("Effects")]
    public GameObject soldierDeathParticle;

    [Header("UI")]
    public TextMeshProUGUI armyCountText;

    [Header("Movement")]
    public float baseHeight = 0.5f;

    private List<Transform> soldiers = new List<Transform>();
    private bool isReforming = false;
    private bool isPlayerAlive = true;

    void Start()
    {
        if (player == null)
            player = transform;
    }

    void Update()
    {
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

    public void AddExistingSoldier(Transform soldierTransform)
    {
        if (soldierTransform == null) return;

        soldierTransform.SetParent(player);
        soldiers.Add(soldierTransform);

        ArmySoldier soldierScript = soldierTransform.GetComponent<ArmySoldier>();
        if (soldierScript == null)
            soldierScript = soldierTransform.gameObject.AddComponent<ArmySoldier>();

        soldierScript.armyManager = this;
        soldierScript.canDie = true;

        StartCoroutine(ApplyItemsToNewSoldier(soldierScript));
        StartCoroutine(DelayedReformation());
    }

    private IEnumerator ApplyItemsToNewSoldier(ArmySoldier soldierScript)
    {
        yield return new WaitForSeconds(0.1f);
        if (soldierScript != null)
            soldierScript.ApplyItemsToSoldier();
    }

    private IEnumerator DelayedReformation()
    {
        yield return new WaitForSeconds(0.2f);
        PositionSoldiersInFormation();
    }

    private IEnumerator SpawnSoldiersWithAnimation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newSoldier = Instantiate(soldierPrefab, player.position, Quaternion.identity);
            newSoldier.transform.SetParent(player);
            // Remove initial scale setting to prevent scaling issues
            // newSoldier.transform.localScale = Vector3.zero;

            soldiers.Add(newSoldier.transform);

            ArmySoldier soldierScript = newSoldier.GetComponent<ArmySoldier>();
            if (soldierScript == null)
                soldierScript = newSoldier.AddComponent<ArmySoldier>();

            soldierScript.armyManager = this;

            // Safe delayed call with null check
            Transform soldierTransform = newSoldier.transform;
            DOVirtual.DelayedCall(0.1f, () => {
                if (soldierScript != null && soldierTransform != null)
                    soldierScript.ApplyItemsToSoldier();
            });

            // Remove all scaling animations - just use position-based spawn effect
            if (soldierTransform != null && soldierTransform.gameObject != null)
            {
                // Simple spawn animation - soldier appears at player's Y level and moves to position
                Vector3 spawnPos = new Vector3(player.position.x, player.position.y, player.position.z); // Same Y as player
                soldierTransform.position = spawnPos;
                // Optional: small sideways offset so they don't spawn exactly on the player
                Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
                soldierTransform.position += randomOffset;
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        yield return new WaitForSeconds(spawnAnimationDuration);
        PositionSoldiersInFormation();
    }

    public void RemoveSoldier(Transform soldier, bool spawnParticles = true)
    {
        if (soldiers.Contains(soldier))
        {
            soldiers.Remove(soldier);

            if (soldier != null)
            {
                // Kill ALL animations on this soldier immediately to prevent errors
                soldier.DOKill(true); // Complete any running tweens
                DOTween.Kill(soldier, true); // Kill with complete = true
                DOTween.Kill(soldier.gameObject, true); // Also kill gameObject-targeted tweens

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySoldierDeathSound();

                if (spawnParticles && soldierDeathParticle != null)
                {
                    GameObject deathEffect = Instantiate(soldierDeathParticle, soldier.position, Quaternion.identity);
                    Destroy(deathEffect, 3f);
                }

                // Remove scaling death animation - just destroy immediately or fade out
                if (soldier != null && soldier.gameObject != null)
                {
                    // Simple fade out or immediate destruction
                    Destroy(soldier.gameObject);
                }
            }

            if (!isReforming)
                StartCoroutine(ReformArmy());
        }
    }

    public void RemoveRandomSoldiers(int count, bool spawnParticles = false)
    {
        int soldiersToRemove = Mathf.Min(count, soldiers.Count);

        for (int i = 0; i < soldiersToRemove; i++)
        {
            if (soldiers.Count > 0)
            {
                int randomIndex = Random.Range(0, soldiers.Count);
                Transform soldierToRemove = soldiers[randomIndex];
                soldiers.RemoveAt(randomIndex);

                if (soldierToRemove != null)
                {
                    // Kill ALL animations immediately
                    soldierToRemove.DOKill(true);
                    DOTween.Kill(soldierToRemove, true);
                    DOTween.Kill(soldierToRemove.gameObject, true);

                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlaySoldierDeathSound();

                    if (spawnParticles && soldierDeathParticle != null)
                    {
                        GameObject deathEffect = Instantiate(soldierDeathParticle, soldierToRemove.position, Quaternion.identity);
                        Destroy(deathEffect, 3f);
                    }

                    // Remove scaling death animation - destroy immediately with optional delay
                    float delay = i * 0.1f;
                    Transform currentSoldier = soldierToRemove;
                    
                    DOVirtual.DelayedCall(delay, () => {
                        if (currentSoldier != null && currentSoldier.gameObject != null)
                        {
                            Destroy(currentSoldier.gameObject);
                        }
                    });
                }
            }
        }

        if (soldiersToRemove > 0 && !isReforming)
            StartCoroutine(ReformArmy());
    }

    private IEnumerator ReformArmy()
    {
        isReforming = true;
        yield return new WaitForSeconds(reformDelay);
        PositionSoldiersInFormation();
        isReforming = false;
    }

    void PositionSoldiersInFormation()
    {
        CleanupNullSoldiers();
        RefreshArmyManagerReferences();

        if (soldiers.Count == 0)
            return;

        int soldierIndex = 0;
        int currentLayer = 0;
        Sequence reformSequence = DOTween.Sequence();

        while (soldierIndex < soldiers.Count && currentLayer < layerCounts.Count)
        {
            int soldiersInThisLayer = Mathf.Min(layerCounts[currentLayer], soldiers.Count - soldierIndex);
            float radius;

            if (currentLayer == 0)
            {
                float circumference = soldiersInThisLayer * soldierSpacing;
                radius = Mathf.Max(circumference / (2f * Mathf.PI), baseLayerRadius);
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
                if (soldierIndex < soldiers.Count && soldiers[soldierIndex] != null)
                {
                    float angle = (360f / soldiersInThisLayer) * i;
                    float angleRad = angle * Mathf.Deg2Rad;
                    float x = Mathf.Sin(angleRad) * radius;
                    float z = Mathf.Cos(angleRad) * radius;

                    Vector3 targetLocalPosition = new Vector3(x, 0f, z);
                    Transform currentSoldier = soldiers[soldierIndex];
                    float animDelay = soldierIndex * 0.02f;

                    // Enhanced null checks before adding to sequence
                    if (currentSoldier != null && currentSoldier.gameObject != null && !currentSoldier.Equals(null))
                    {
                        // Kill any existing movement animations on this soldier
                        try
                        {
                            currentSoldier.DOKill();
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Error killing DOTween on soldier {soldierIndex}: {e.Message}");
                            continue; // Skip this soldier if we can't kill its animations
                        }
                        
                        // Create movement tween with additional safety checks
                        var moveTween = currentSoldier.DOLocalMove(targetLocalPosition, reformAnimationDuration)
                            .SetEase(reformEase)
                            .SetTarget(currentSoldier)
                            .OnStart(() => {
                                // Double-check the soldier still exists when the tween starts
                                if (currentSoldier == null || currentSoldier.Equals(null))
                                {
                                    Debug.LogWarning("Soldier became null during formation movement start");
                                    return;
                                }
                            })
                            .OnUpdate(() => {
                                // Safety check during animation
                                if (currentSoldier == null || currentSoldier.Equals(null))
                                {
                                    Debug.LogWarning("Soldier became null during formation movement");
                                    return;
                                }
                            });

                        // Create rotation tween with additional safety checks  
                        var rotTween = currentSoldier.DOLocalRotateQuaternion(Quaternion.identity, reformAnimationDuration * 0.5f)
                            .SetEase(Ease.OutQuart)
                            .SetTarget(currentSoldier)
                            .OnStart(() => {
                                if (currentSoldier == null || currentSoldier.Equals(null))
                                {
                                    Debug.LogWarning("Soldier became null during formation rotation start");
                                    return;
                                }
                            })
                            .OnUpdate(() => {
                                if (currentSoldier == null || currentSoldier.Equals(null))
                                {
                                    Debug.LogWarning("Soldier became null during formation rotation");
                                    return;
                                }
                            });

                        // Only add to sequence if tweens were created successfully
                        if (moveTween != null && !moveTween.Equals(null))
                        {
                            reformSequence.Insert(animDelay, moveTween);
                        }
                        
                        if (rotTween != null && !rotTween.Equals(null))
                        {
                            reformSequence.Insert(animDelay, rotTween);
                        }
                    }
                }
                soldierIndex++;
            }
            currentLayer++;
        }

        // Only play sequence if it has tweens and add global safety callback
        if (reformSequence != null && reformSequence.Duration() > 0)
        {
            reformSequence.OnComplete(() => {
                // Clean up any null soldiers after formation completes
                CleanupNullSoldiers();
            }).Play();
        }
    }

    void CleanupNullSoldiers()
    {
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            // Enhanced null checking for destroyed Unity objects
            if (soldiers[i] == null || soldiers[i].Equals(null) || soldiers[i].gameObject == null)
            {
                soldiers.RemoveAt(i);
            }
        }
    }

    void RefreshArmyManagerReferences()
    {
        foreach (Transform soldier in soldiers.ToArray()) // Use ToArray to avoid collection modification issues
        {
            if (soldier != null && !soldier.Equals(null) && soldier.gameObject != null)
            {
                try
                {
                    ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
                    if (soldierScript != null && soldierScript.armyManager == null)
                        soldierScript.armyManager = this;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error refreshing army manager reference: {e.Message}");
                    // Remove the problematic soldier from the list
                    soldiers.Remove(soldier);
                }
            }
        }
        
        // Final cleanup after processing
        CleanupNullSoldiers();
    }

    public int GetArmySize()
    {
        CleanupNullSoldiers();
        return soldiers.Count + (isPlayerAlive ? 1 : 0);
    }

    public void SetPlayerDead()
    {
        isPlayerAlive = false;
    }

    public void SetPlayerAlive()
    {
        isPlayerAlive = true;
    }

    public bool IsPlayerAlive()
    {
        return isPlayerAlive;
    }

    public List<Transform> GetAvailableSoldiers()
    {
        CleanupNullSoldiers();
        
        // Additional safety check - filter out any null soldiers that might have slipped through
        List<Transform> availableSoldiers = new List<Transform>();
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null && !soldier.Equals(null) && soldier.gameObject != null)
            {
                availableSoldiers.Add(soldier);
            }
        }
        
        return availableSoldiers;
    }

    [ContextMenu("Force Army Reformation")]
    public void ForceReformation()
    {
        PositionSoldiersInFormation();
    }

    [ContextMenu("Refresh Army Manager References")]
    public void ForceRefreshReferences()
    {
        RefreshArmyManagerReferences();
    }

    [ContextMenu("Test Remove Random Soldier")]
    public void TestRemoveRandomSoldier()
    {
        if (soldiers.Count > 0)
        {
            int randomIndex = Random.Range(0, soldiers.Count);
            Transform soldierToRemove = soldiers[randomIndex];
            RemoveSoldier(soldierToRemove);
        }
    }

    public void ClearArmy()
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                // Kill all animations immediately and completely
                soldier.DOKill(true);
                DOTween.Kill(soldier, true);
                DOTween.Kill(soldier.gameObject, true);
                
                Destroy(soldier.gameObject);
            }
        }
        soldiers.Clear();
        isPlayerAlive = true;
    }

    void OnDestroy()
    {
        // Kill all DOTween animations before destroying
        if (transform != null && !transform.Equals(null))
        {
            try
            {
                transform.DOKill(true);
                DOTween.Kill(transform, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error killing DOTween on ArmyManager transform: {e.Message}");
            }
        }

        // Create a copy of the list to avoid modification during iteration
        var soldiersCopy = new List<Transform>(soldiers);
        foreach (Transform soldier in soldiersCopy)
        {
            if (soldier != null && !soldier.Equals(null))
            {
                try
                {
                    soldier.DOKill(true);
                    DOTween.Kill(soldier, true);
                    DOTween.Kill(soldier.gameObject, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error killing DOTween on soldier: {e.Message}");
                }
            }
        }

        if (player != null && !player.Equals(null))
        {
            try
            {
                player.DOKill(true);
                DOTween.Kill(player, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error killing DOTween on player: {e.Message}");
            }
        }
        
        // Clear the list to prevent further access
        soldiers.Clear();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            KillAllDOTweenAnimations();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            KillAllDOTweenAnimations();
    }

    public void KillAllDOTweenAnimations()
    {
        if (transform != null && !transform.Equals(null))
        {
            try
            {
                transform.DOKill(true);
                DOTween.Kill(transform, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error killing DOTween animations on ArmyManager: {e.Message}");
            }
        }

        // Create a copy to avoid collection modification issues
        var soldiersCopy = new List<Transform>(soldiers);
        foreach (Transform soldier in soldiersCopy)
        {
            if (soldier != null && !soldier.Equals(null))
            {
                try
                {
                    soldier.DOKill(true);
                    DOTween.Kill(soldier, true);
                    DOTween.Kill(soldier.gameObject, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error killing DOTween animations on soldier: {e.Message}");
                }
            }
        }

        if (player != null && !player.Equals(null))
        {
            try
            {
                player.DOKill(true);
                DOTween.Kill(player, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Error killing DOTween animations on player: {e.Message}");
            }
        }
        
        // Clean up null references after killing animations
        CleanupNullSoldiers();
    }

    public void PlayVictoryAnimation(float animationDuration = 1f, float bounceScale = 1.3f)
    {
        CleanupNullSoldiers();

        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] != null)
            {
                soldiers[i].DOKill();
                Animator soldierAnimator = soldiers[i].GetComponentInChildren<Animator>() ?? soldiers[i].GetComponentInChildren<Animator>();
                if (soldierAnimator != null)
                    soldierAnimator.SetTrigger("victory");
            }
        }

        if (player != null)
        {
            player.DOKill();
            Animator playerAnimator = player.GetComponent<Animator>() ?? player.GetComponentInChildren<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("victory");
        }
    }
}
