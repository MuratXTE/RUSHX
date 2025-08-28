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
            newSoldier.transform.localScale = Vector3.zero;

            soldiers.Add(newSoldier.transform);

            ArmySoldier soldierScript = newSoldier.GetComponent<ArmySoldier>();
            if (soldierScript == null)
                soldierScript = newSoldier.AddComponent<ArmySoldier>();

            soldierScript.armyManager = this;

            DOVirtual.DelayedCall(0.1f, () => {
                if (soldierScript != null)
                    soldierScript.ApplyItemsToSoldier();
            });

            newSoldier.transform.DOScale(Vector3.one, spawnAnimationDuration)
                .SetEase(spawnEase)
                .OnComplete(() => {
                    newSoldier.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 1, 0.5f);
                });

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
                soldier.DOKill();

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySoldierDeathSound();

                if (spawnParticles && soldierDeathParticle != null)
                {
                    GameObject deathEffect = Instantiate(soldierDeathParticle, soldier.position, Quaternion.identity);
                    Destroy(deathEffect, 3f);
                }

                soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                DOVirtual.DelayedCall(0.3f, () => {
                    if (soldier != null)
                        Destroy(soldier.gameObject);
                });
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
                    soldierToRemove.DOKill();

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
                        if (currentSoldier != null)
                        {
                            currentSoldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                            DOVirtual.DelayedCall(0.3f, () => {
                                if (currentSoldier != null)
                                    Destroy(currentSoldier.gameObject);
                            });
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

                    reformSequence.Insert(animDelay,
                        currentSoldier.DOLocalMove(targetLocalPosition, reformAnimationDuration)
                        .SetEase(reformEase)
                        .OnComplete(() => {
                            currentSoldier.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.3f);
                        })
                    );

                    reformSequence.Insert(animDelay,
                        currentSoldier.DOLocalRotateQuaternion(Quaternion.identity, reformAnimationDuration * 0.5f)
                        .SetEase(Ease.OutQuart)
                    );
                }
                soldierIndex++;
            }
            currentLayer++;
        }

        reformSequence.Play();
    }

    void CleanupNullSoldiers()
    {
        for (int i = soldiers.Count - 1; i >= 0; i--)
        {
            if (soldiers[i] == null)
                soldiers.RemoveAt(i);
        }
    }

    void RefreshArmyManagerReferences()
    {
        foreach (Transform soldier in soldiers)
        {
            if (soldier != null)
            {
                ArmySoldier soldierScript = soldier.GetComponent<ArmySoldier>();
                if (soldierScript != null && soldierScript.armyManager == null)
                    soldierScript.armyManager = this;
            }
        }
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
        return new List<Transform>(soldiers);
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
                soldier.DOKill();
                DOTween.Kill(soldier);
                Destroy(soldier.gameObject);
            }
        }
        soldiers.Clear();
        isPlayerAlive = true;
    }

    void OnDestroy()
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
    }

    public void PlayVictoryAnimation(float animationDuration = 1f, float bounceScale = 1.3f)
    {
        CleanupNullSoldiers();

        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] != null)
            {
                soldiers[i].DOKill();
                Animator soldierAnimator = soldiers[i].GetComponent<Animator>() ?? soldiers[i].GetComponentInChildren<Animator>();
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
