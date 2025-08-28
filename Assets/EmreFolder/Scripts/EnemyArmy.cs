using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Murat;

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

    [Header("UI References")]
    [Tooltip("Game Lost UI object to enable when player loses")]
    public GameObject gameLostUI;

    protected List<Transform> enemySoldiers = new List<Transform>();
    private bool isInCombat = false;
    private bool isPlayerDefeated = false;
    protected ArmyManager playerArmy;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnEnemyArmy();
        }

        playerArmy = FindFirstObjectByType<ArmyManager>();
        UpdateArmyDisplay();
    }

    void Update()
    {
        if (!isInCombat && !isPlayerDefeated && playerArmy != null)
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

            Renderer renderer = enemySoldier.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = enemyColor;
            }

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

        if (distance <= combatTriggerDistance && GetArmySize() > 0 && playerArmy.GetArmySize() > 0)
        {
            StartCombat();
        }
    }

    void StartCombat()
    {
        if (isInCombat) return;
        isInCombat = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCombatStartSound();
        }

        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetCombatState(true);
        }

        StartCoroutine(HandleCombat());
    }

    protected virtual IEnumerator HandleCombat()
    {
        List<Transform> playerSoldiers = playerArmy.GetAvailableSoldiers();
        List<Transform> enemySoldiers = GetAvailableSoldiers();

        int enemyCount = enemySoldiers.Count;
        int playerSoldierCount = playerSoldiers.Count;
        bool playerNeedsToFight = enemyCount >= playerArmy.GetArmySize();

        int fightingPairs = playerNeedsToFight
            ? Mathf.Min(playerSoldierCount + 1, enemyCount)
            : Mathf.Min(playerSoldierCount, enemyCount);

        if (fightingPairs > 0)
        {
            List<CombatPair> combatPairs = new List<CombatPair>();

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

            if (playerNeedsToFight && fightingPairs > soldierPairs && soldierPairs < enemyCount)
            {
                CombatPair playerPair = new CombatPair
                {
                    playerSoldier = playerArmy.player,
                    enemySoldier = enemySoldiers[soldierPairs],
                    isPlayerInvolved = true
                };
                combatPairs.Add(playerPair);
            }

            yield return StartCoroutine(ExecuteCombat(combatPairs));
        }

        isInCombat = false;

        if (playerArmy != null && playerArmy.GetArmySize() == 0)
        {
            yield return StartCoroutine(HandlePlayerDefeat());
            yield break;
        }

        PlayerController playerController2 = playerArmy.player.GetComponent<PlayerController>();
        if (playerController2 != null)
        {
            playerController2.SetCombatState(false);
        }

        playerArmy.ForceReformation();
        PositionSoldiersInFormation();
    }

    IEnumerator ExecuteCombat(List<CombatPair> combatPairs)
    {
        List<Tween> moveTweens = new List<Tween>();
        List<Transform> soldiersInCombat = new List<Transform>();

        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null && pair.enemySoldier != null)
            {
                // Add soldiers to combat tracking list
                soldiersInCombat.Add(pair.playerSoldier);
                soldiersInCombat.Add(pair.enemySoldier);

                Vector3 playerPos = pair.playerSoldier.position;
                Vector3 enemyPos = pair.enemySoldier.position;
                Vector3 midPoint = (playerPos + enemyPos) / 2f;

                // Create safe movement tweens with collision detection
                Tween playerMove = CreateSafeCombatMove(pair.playerSoldier, midPoint + Vector3.left * 0.3f);
                Tween enemyMove = CreateSafeCombatMove(pair.enemySoldier, midPoint + Vector3.right * 0.3f);

                if (playerMove != null) moveTweens.Add(playerMove);
                if (enemyMove != null) moveTweens.Add(enemyMove);
            }
        }

        // Wait for movement phase with safety checks
        if (moveTweens.Count > 0)
        {
            Sequence moveSequence = DOTween.Sequence();
            foreach (var tween in moveTweens)
            {
                if (tween != null && tween.active) moveSequence.Join(tween);
            }

            if (moveSequence != null && moveSequence.active)
            {
                yield return moveSequence.WaitForCompletion();
            }
        }

        // Additional safety check - verify all soldiers still exist before proceeding
        combatPairs = ValidateCombatPairs(combatPairs);

        // Attack phase
        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null && pair.enemySoldier != null)
            {
                Animator playerAnimator = pair.playerSoldier.GetComponentInChildren<Animator>();
                if (playerAnimator != null) playerAnimator.SetTrigger("attack");

                Animator enemyAnimator = pair.enemySoldier.GetComponentInChildren<Animator>();
                if (enemyAnimator != null) enemyAnimator.SetTrigger("attack");

                if (combatParticleEffect != null)
                {
                    Vector3 effectPos = (pair.playerSoldier.position + pair.enemySoldier.position) / 2f;
                    GameObject effect = Instantiate(combatParticleEffect, effectPos, Quaternion.identity);
                    if (pair.isPlayerInvolved) effect.transform.localScale *= 1.5f;
                }
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCombatAttackSound();
        }

        yield return new WaitForSeconds(attackAnimationDuration);

        // Final combat resolution with validation
        foreach (var pair in combatPairs)
        {
            if (pair.enemySoldier != null) RemoveEnemySoldier(pair.enemySoldier);

            if (pair.playerSoldier != null)
            {
                // Ensure combat movement state is cleared
                ArmySoldier soldierScript = pair.playerSoldier.GetComponent<ArmySoldier>();
                if (soldierScript != null)
                {
                    soldierScript.isInCombatMovement = false;
                }

                if (pair.isPlayerInvolved)
                {
                    playerArmy.SetPlayerDead();
                    HandlePlayerDeath();
                }
                else
                {
                    playerArmy.RemoveSoldier(pair.playerSoldier, false);
                }
            }
        }
    }

    // Create safe combat movement with collision detection
    Tween CreateSafeCombatMove(Transform soldier, Vector3 targetPosition)
    {
        if (soldier == null || soldier.Equals(null)) return null;

        // Mark soldier as in combat movement to prevent obstacle death
        ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
        if (soldierScript != null)
        {
            soldierScript.isInCombatMovement = true;
        }

        // Check if path to target position would collide with obstacles
        Vector3 startPos = soldier.position;
        Vector3 direction = (targetPosition - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPosition);

        // Raycast to check for obstacles in the path
        if (Physics.Raycast(startPos, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                // Obstacle in path - find alternative safe position
                Vector3 safePosition = FindSafeCombatPosition(startPos, targetPosition);
                targetPosition = safePosition;
            }
        }

        // Create movement tween with death detection
        return soldier.DOMove(targetPosition, soldierMoveSpeed)
            .SetSpeedBased()
            .SetTarget(soldier)
            .OnUpdate(() => {
                // Check if soldier died during movement
                if (soldier == null || soldier.Equals(null))
                {
                    // Soldier died, but tween will be killed by DOTween cleanup
                    return;
                }
            })
            .OnComplete(() => {
                if (soldier != null && !soldier.Equals(null))
                {
                    // Ensure soldier is at correct position after movement
                    soldier.position = targetPosition;
                    
                    // Remove combat movement protection
                    ArmySoldier script = soldier.GetComponent<ArmySoldier>();
                    if (script != null)
                    {
                        script.isInCombatMovement = false;
                    }
                }
            });
    }

    // Find a safe position for combat that avoids obstacles
    Vector3 FindSafeCombatPosition(Vector3 startPos, Vector3 idealTarget)
    {
        Vector3 direction = (idealTarget - startPos).normalized;
        float maxDistance = Vector3.Distance(startPos, idealTarget);
        
        // Try different positions around the ideal target
        Vector3[] offsets = {
            Vector3.zero,
            Vector3.right * 0.5f,
            Vector3.left * 0.5f,
            Vector3.forward * 0.5f,
            Vector3.back * 0.5f,
            (Vector3.right + Vector3.forward) * 0.3f,
            (Vector3.left + Vector3.forward) * 0.3f,
            (Vector3.right + Vector3.back) * 0.3f,
            (Vector3.left + Vector3.back) * 0.3f
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 testPosition = idealTarget + offset;
            
            // Check if this position is safe (no obstacles nearby)
            if (!Physics.CheckSphere(testPosition, 0.3f, LayerMask.GetMask("Default")))
            {
                // Also check path to this position
                Vector3 pathDirection = (testPosition - startPos).normalized;
                float pathDistance = Vector3.Distance(startPos, testPosition);
                
                if (!Physics.Raycast(startPos, pathDirection, pathDistance))
                {
                    return testPosition;
                }
            }
        }
        
        // If no safe position found, return a position closer to start
        return Vector3.Lerp(startPos, idealTarget, 0.5f);
    }

    // Validate combat pairs and remove any with null/destroyed soldiers
    List<CombatPair> ValidateCombatPairs(List<CombatPair> pairs)
    {
        List<CombatPair> validPairs = new List<CombatPair>();
        
        foreach (var pair in pairs)
        {
            bool playerValid = pair.playerSoldier != null && !pair.playerSoldier.Equals(null);
            bool enemyValid = pair.enemySoldier != null && !pair.enemySoldier.Equals(null);
            
            if (playerValid && enemyValid)
            {
                validPairs.Add(pair);
            }
            else
            {
                // One or both soldiers died during movement - handle cleanup
                if (!playerValid && pair.playerSoldier != null)
                {
                    Debug.Log("Player soldier died during combat movement");
                }
                if (!enemyValid && pair.enemySoldier != null)
                {
                    Debug.Log("Enemy soldier died during combat movement");
                }
            }
        }
        
        return validPairs;
    }

    public void RemoveEnemySoldier(Transform soldier)
    {
        if (enemySoldiers.Contains(soldier))
        {
            enemySoldiers.Remove(soldier);
            soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                if (soldier != null) Destroy(soldier.gameObject);
            });
        }
    }

    void HandlePlayerDeath()
    {
        Debug.Log("Player character eliminated in combat.");
    }

    IEnumerator HandlePlayerDefeat()
    {
        isPlayerDefeated = true;
        isInCombat = false;

        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoMove(false);
            playerController.SetCombatState(true);
        }

        if (playerArmy.player != null)
        {
            playerArmy.player.DOKill();
            DOTween.Kill(playerArmy.player);
        }

        if (playerArmy != null) playerArmy.enabled = false;
        if (playerArmy != null) playerArmy.ClearArmy();

        if (gameLostUI != null) gameLostUI.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        this.enabled = false;

        yield return null;
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

    void OnDestroy()
    {
        KillAllEnemyDOTweenAnimations();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) KillAllEnemyDOTweenAnimations();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) KillAllEnemyDOTweenAnimations();
    }

    public void KillAllEnemyDOTweenAnimations()
    {
        if (transform != null)
        {
            transform.DOKill();
            DOTween.Kill(transform);
        }

        foreach (Transform enemySoldier in enemySoldiers)
        {
            if (enemySoldier != null)
            {
                enemySoldier.DOKill();
                DOTween.Kill(enemySoldier);
            }
        }
    }
}

[System.Serializable]
public class CombatPair
{
    public Transform playerSoldier;
    public Transform enemySoldier;
    public bool isPlayerInvolved = false;
}
