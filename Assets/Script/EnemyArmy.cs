using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Murat;

public class EnemyArmy : MonoBehaviour
{
    public GameObject enemySoldierPrefab;
    public int initialSoldierCount = 10;
    public bool spawnOnStart = true;

    public List<int> layerCounts = new List<int> { 1, 6, 12, 18, 24, 30 };
    public float baseLayerRadius = 1.2f;
    public float radiusMultiplier = 1.5f;
    public float soldierSpacing = 1f;

    public float combatTriggerDistance = 5f;
    public float soldierMoveSpeed = 3f;
    public float attackAnimationDuration = 1f;

    public TextMeshPro armyCountText;
    public Color enemyColor = Color.red;

    public GameObject combatParticleEffect;
    public GameObject gameLostUI;

    protected List<Transform> enemySoldiers = new List<Transform>();
    private bool isInCombat = false;
    private bool isPlayerDefeated = false;
    protected ArmyManager playerArmy;

    void Start()
    {
        if (spawnOnStart) SpawnEnemyArmy();
        playerArmy = FindFirstObjectByType<ArmyManager>();
        UpdateArmyDisplay();
    }

    void Update()
    {
        if (!isInCombat && !isPlayerDefeated && playerArmy != null)
            CheckForCombat();

        UpdateArmyDisplay();
    }

    protected virtual void SpawnEnemyArmy()
    {
        for (int i = 0; i < initialSoldierCount; i++)
        {
            GameObject enemySoldier = Instantiate(enemySoldierPrefab, transform.position, Quaternion.identity);
            enemySoldier.transform.SetParent(transform);

            Renderer renderer = enemySoldier.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = enemyColor;

            EnemySoldier soldierScript = enemySoldier.GetComponent<EnemySoldier>();
            if (soldierScript == null) soldierScript = enemySoldier.AddComponent<EnemySoldier>();
            soldierScript.enemyArmy = this;

            enemySoldiers.Add(enemySoldier.transform);
        }
        PositionSoldiersInFormation();
    }

    void PositionSoldiersInFormation()
    {
        CleanupNullSoldiers();
        if (enemySoldiers.Count == 0) return;

        int soldierIndex = 0, currentLayer = 0;
        while (soldierIndex < enemySoldiers.Count && currentLayer < layerCounts.Count)
        {
            int soldiersInThisLayer = Mathf.Min(layerCounts[currentLayer], enemySoldiers.Count - soldierIndex);
            float radius = currentLayer == 0
                ? Mathf.Max((soldiersInThisLayer * soldierSpacing) / (2f * Mathf.PI), baseLayerRadius)
                : Mathf.Max((soldiersInThisLayer * soldierSpacing) / (2f * Mathf.PI), baseLayerRadius * (currentLayer * radiusMultiplier));

            for (int i = 0; i < soldiersInThisLayer; i++)
            {
                if (soldierIndex < enemySoldiers.Count && enemySoldiers[soldierIndex] != null)
                {
                    float angle = (360f / soldiersInThisLayer) * i;
                    float x = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                    float z = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                    enemySoldiers[soldierIndex].localPosition = new Vector3(x, 0f, z);
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
            StartCombat();
    }

    void StartCombat()
    {
        if (isInCombat) return;
        isInCombat = true;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayCombatStartSound();

        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null) playerController.SetCombatState(true);

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
                combatPairs.Add(new CombatPair
                {
                    playerSoldier = playerSoldiers[i],
                    enemySoldier = enemySoldiers[i],
                    isPlayerInvolved = false
                });
            }

            if (playerNeedsToFight && fightingPairs > soldierPairs && soldierPairs < enemyCount)
            {
                combatPairs.Add(new CombatPair
                {
                    playerSoldier = playerArmy.player,
                    enemySoldier = enemySoldiers[soldierPairs],
                    isPlayerInvolved = true
                });
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
        if (playerController2 != null) playerController2.SetCombatState(false);

        playerArmy.ForceReformation();
        PositionSoldiersInFormation();
    }

    IEnumerator ExecuteCombat(List<CombatPair> combatPairs)
    {
        List<Tween> moveTweens = new List<Tween>();
        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null && pair.enemySoldier != null)
            {
                Vector3 playerPos = pair.playerSoldier.position;
                Vector3 enemyPos = pair.enemySoldier.position;
                Vector3 midPoint = (playerPos + enemyPos) / 2f;

                Tween playerMove = CreateSafeCombatMove(pair.playerSoldier, midPoint + Vector3.left * 0.3f);
                Tween enemyMove = CreateSafeCombatMove(pair.enemySoldier, midPoint + Vector3.right * 0.3f);

                if (playerMove != null) moveTweens.Add(playerMove);
                if (enemyMove != null) moveTweens.Add(enemyMove);
            }
        }

        if (moveTweens.Count > 0)
        {
            Sequence moveSequence = DOTween.Sequence();
            foreach (var tween in moveTweens) if (tween != null && tween.active) moveSequence.Join(tween);
            if (moveSequence != null && moveSequence.active) yield return moveSequence.WaitForCompletion();
        }

        combatPairs = ValidateCombatPairs(combatPairs);

        foreach (var pair in combatPairs)
        {
            if (pair.playerSoldier != null) pair.playerSoldier.GetComponentInChildren<Animator>()?.SetTrigger("attack");
            if (pair.enemySoldier != null) pair.enemySoldier.GetComponentInChildren<Animator>()?.SetTrigger("attack");

            if (combatParticleEffect != null && pair.playerSoldier != null && pair.enemySoldier != null)
            {
                Vector3 effectPos = (pair.playerSoldier.position + pair.enemySoldier.position) / 2f;
                GameObject effect = Instantiate(combatParticleEffect, effectPos, Quaternion.identity);
                if (pair.isPlayerInvolved) effect.transform.localScale *= 1.5f;
            }
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlayCombatAttackSound();

        yield return new WaitForSeconds(attackAnimationDuration);

        foreach (var pair in combatPairs)
        {
            if (pair.enemySoldier != null) RemoveEnemySoldier(pair.enemySoldier);

            if (pair.playerSoldier != null)
            {
                ArmySoldier soldierScript = pair.playerSoldier.GetComponent<ArmySoldier>();
                if (soldierScript != null) soldierScript.isInCombatMovement = false;

                if (pair.isPlayerInvolved)
                {
                    playerArmy.SetPlayerDead();
                    HandlePlayerDeath();
                }
                else playerArmy.RemoveSoldier(pair.playerSoldier, false);
            }
        }
    }

    Tween CreateSafeCombatMove(Transform soldier, Vector3 targetPosition)
    {
        if (soldier == null) return null;

        ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
        if (soldierScript != null) soldierScript.isInCombatMovement = true;

        Vector3 startPos = soldier.position;
        Vector3 direction = (targetPosition - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPosition);

        if (Physics.Raycast(startPos, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Obstacle"))
                targetPosition = FindSafeCombatPosition(startPos, targetPosition);
        }

        return soldier.DOMove(targetPosition, soldierMoveSpeed)
            .SetSpeedBased()
            .SetTarget(soldier)
            .OnComplete(() =>
            {
                if (soldier != null)
                {
                    soldier.position = targetPosition;
                    ArmySoldier script = soldier.GetComponent<ArmySoldier>();
                    if (script != null) script.isInCombatMovement = false;
                }
            });
    }

    Vector3 FindSafeCombatPosition(Vector3 startPos, Vector3 idealTarget)
    {
        Vector3[] offsets = {
            Vector3.zero, Vector3.right * 0.5f, Vector3.left * 0.5f,
            Vector3.forward * 0.5f, Vector3.back * 0.5f,
            (Vector3.right + Vector3.forward) * 0.3f,
            (Vector3.left + Vector3.forward) * 0.3f,
            (Vector3.right + Vector3.back) * 0.3f,
            (Vector3.left + Vector3.back) * 0.3f
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 testPosition = idealTarget + offset;
            if (!Physics.CheckSphere(testPosition, 0.3f, LayerMask.GetMask("Default")))
            {
                Vector3 pathDirection = (testPosition - startPos).normalized;
                float pathDistance = Vector3.Distance(startPos, testPosition);
                if (!Physics.Raycast(startPos, pathDirection, pathDistance)) return testPosition;
            }
        }
        return Vector3.Lerp(startPos, idealTarget, 0.5f);
    }

    List<CombatPair> ValidateCombatPairs(List<CombatPair> pairs)
    {
        List<CombatPair> validPairs = new List<CombatPair>();
        foreach (var pair in pairs)
        {
            bool playerValid = pair.playerSoldier != null;
            bool enemyValid = pair.enemySoldier != null;
            if (playerValid && enemyValid) validPairs.Add(pair);
        }
        return validPairs;
    }

    public void RemoveEnemySoldier(Transform soldier)
    {
        if (enemySoldiers.Contains(soldier))
        {
            enemySoldiers.Remove(soldier);
            soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                if (soldier != null) Destroy(soldier.gameObject);
            });
        }
    }

    void HandlePlayerDeath() { }

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
            if (enemySoldiers[i] == null) enemySoldiers.RemoveAt(i);
        }
    }

    public int GetArmySize()
    {
        CleanupNullSoldiers();
        return enemySoldiers.Count;
    }

    void UpdateArmyDisplay()
    {
        if (armyCountText != null) armyCountText.text = GetArmySize().ToString();
    }

    void OnDestroy() { KillAllEnemyDOTweenAnimations(); }
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) KillAllEnemyDOTweenAnimations(); }
    void OnApplicationFocus(bool hasFocus) { if (!hasFocus) KillAllEnemyDOTweenAnimations(); }

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
