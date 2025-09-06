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

            soldiers.Add(newSoldier.transform);

            ArmySoldier soldierScript = newSoldier.GetComponent<ArmySoldier>();
            if (soldierScript == null)
                soldierScript = newSoldier.AddComponent<ArmySoldier>();

            soldierScript.armyManager = this;

            Transform soldierTransform = newSoldier.transform;
            DOVirtual.DelayedCall(0.1f, () => {
                if (soldierScript != null && soldierTransform != null)
                    soldierScript.ApplyItemsToSoldier();
            });

            if (soldierTransform != null && soldierTransform.gameObject != null)
            {
                Vector3 spawnPos = new Vector3(player.position.x, player.position.y, player.position.z);
                soldierTransform.position = spawnPos;
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
                soldier.DOKill(true);
                DOTween.Kill(soldier, true);
                DOTween.Kill(soldier.gameObject, true);

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySoldierDeathSound();

                if (spawnParticles && soldierDeathParticle != null)
                {
                    GameObject deathEffect = Instantiate(soldierDeathParticle, soldier.position, Quaternion.identity);
                    Destroy(deathEffect, 3f);
                }

                if (soldier != null && soldier.gameObject != null)
                {
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

                    if (currentSoldier != null && currentSoldier.gameObject != null && !currentSoldier.Equals(null))
                    {
                        try
                        {
                            currentSoldier.DOKill();
                        }
                        catch { continue; }

                        var moveTween = currentSoldier.DOLocalMove(targetLocalPosition, reformAnimationDuration)
                            .SetEase(reformEase)
                            .SetTarget(currentSoldier);

                        var rotTween = currentSoldier.DOLocalRotateQuaternion(Quaternion.identity, reformAnimationDuration * 0.5f)
                            .SetEase(Ease.OutQuart)
                            .SetTarget(currentSoldier);

                        if (moveTween != null && !moveTween.Equals(null))
                            reformSequence.Insert(animDelay, moveTween);

                        if (rotTween != null && !rotTween.Equals(null))
                            reformSequence.Insert(animDelay, rotTween);
                    }
                }
                soldierIndex++;
            }
            currentLayer++;
        }

        if (reformSequence != null && reformSequence.Duration() > 0)
        {
            reformSequence.OnComplete(() => {
                CleanupNullSoldiers();
            }).Play();
        }
    }

    void CleanupNullSoldiers()
    {
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            if (soldiers[i] == null || soldiers[i].Equals(null) || soldiers[i].gameObject == null)
            {
                soldiers.RemoveAt(i);
            }
        }
    }

    void RefreshArmyManagerReferences()
    {
        foreach (Transform soldier in soldiers.ToArray())
        {
            if (soldier != null && !soldier.Equals(null) && soldier.gameObject != null)
            {
                ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
                if (soldierScript != null && soldierScript.armyManager == null)
                    soldierScript.armyManager = this;
            }
        }
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
        if (transform != null && !transform.Equals(null))
        {
            try
            {
                transform.DOKill(true);
                DOTween.Kill(transform, true);
            }
            catch { }
        }

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
                catch { }
            }
        }

        if (player != null && !player.Equals(null))
        {
            try
            {
                player.DOKill(true);
                DOTween.Kill(player, true);
            }
            catch { }
        }

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
            catch { }
        }

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
                catch { }
            }
        }

        if (player != null && !player.Equals(null))
        {
            try
            {
                player.DOKill(true);
                DOTween.Kill(player, true);
            }
            catch { }
        }

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
