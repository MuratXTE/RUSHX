using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Murat;

public class BossFight : EnemyArmy
{
    public GameObject victoryParticleEffect;
    public float victoryCelebrationDuration = 3f;
    public float victoryBounceScale = 1.3f;
    public float victoryAnimationDuration = 0.8f;
    public bool endLevelAfterVictory = false;

    public Color bossColor = Color.magenta;
    public float bossSoldierScale = 1.2f;
    public Material bossMaterial;

    public AudioClip victorySoundClip;
    public GameObject gameWonUI;

    private bool hasTriggeredVictory = false;

    void Start()
    {
        enemyColor = bossColor;
        if (spawnOnStart) SpawnEnemyArmy();
        playerArmy = FindFirstObjectByType<ArmyManager>();
        if (armyCountText != null) armyCountText.text = GetArmySize().ToString();
        ApplyBossModifications();
    }

    void ApplyBossModifications()
    {
        foreach (Transform soldier in enemySoldiers)
        {
            if (soldier != null)
            {
                soldier.localScale = Vector3.one * bossSoldierScale;

                Renderer renderer = soldier.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (bossMaterial != null) renderer.material = bossMaterial;
                    else renderer.material.color = bossColor;
                }
                StartBossIdleAnimation(soldier);
            }
        }
    }

    void StartBossIdleAnimation(Transform soldier)
    {
        if (soldier == null) return;
        soldier.DOKill();
        DOTween.Kill(soldier);

        Vector3 originalScale = soldier.localScale;
        soldier.DOScale(originalScale * 1.05f, 2f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetTarget(soldier);
    }

    protected override IEnumerator HandleCombat()
    {
        yield return StartCoroutine(base.HandleCombat());
        CheckForVictoryOrDefeat();
    }

    void CheckForVictoryOrDefeat()
    {
        if (hasTriggeredVictory) return;
        if (GetArmySize() == 0 && playerArmy != null && playerArmy.GetArmySize() > 0)
            StartCoroutine(HandleVictory());
        else if (playerArmy != null && playerArmy.GetArmySize() == 0)
            StartCoroutine(HandleDefeat());
    }

    IEnumerator HandleDefeat()
    {
        if (hasTriggeredVictory) yield break;
        hasTriggeredVictory = true;

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

        if (playerArmy != null)
        {
            playerArmy.enabled = false;
            playerArmy.ClearArmy();
        }

        if (gameLostUI != null) gameLostUI.SetActive(true);
        if (playerController != null) playerController.enabled = false;

        this.enabled = false;
    }

    IEnumerator HandleVictory()
    {
        if (hasTriggeredVictory) yield break;
        hasTriggeredVictory = true;

        PlayerController playerController = playerArmy.player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetAutoMove(false);
            playerController.SetCombatState(true);
        }

        PlayVictorySound();

        if (victoryParticleEffect != null)
        {
            GameObject victoryEffect = Instantiate(victoryParticleEffect, transform.position, Quaternion.identity);
            Destroy(victoryEffect, victoryCelebrationDuration);
        }

        List<Transform> victorySoldiers = playerArmy.GetAvailableSoldiers();
        if (playerArmy.player != null) victorySoldiers.Add(playerArmy.player);

        yield return StartCoroutine(PlayVictoryAnimations(victorySoldiers));

        int remainingArmySize = playerArmy.GetArmySize();
        int pointsToAdd = remainingArmySize;
        BellekYonetim bellekYonetim = new BellekYonetim();
        int currentPoints = bellekYonetim.VeriOku_i("Puan");
        int newPoints = currentPoints + pointsToAdd;
        bellekYonetim.VeriKaydet_int("Puan", newPoints);

        if (gameWonUI != null) gameWonUI.SetActive(true);

        KillAllBossDOTweenAnimations();

        if (endLevelAfterVictory)
        {
            yield return new WaitForSeconds(1f);
            HandleLevelComplete();
        }
    }

    IEnumerator PlayVictoryAnimations(List<Transform> soldiers)
    {
        foreach (Transform enemySoldier in enemySoldiers)
        {
            if (enemySoldier != null)
            {
                enemySoldier.DOKill();
                DOTween.Kill(enemySoldier);
            }
        }

        if (playerArmy != null)
            playerArmy.PlayVictoryAnimation(victoryAnimationDuration, victoryBounceScale);

        yield return new WaitForSeconds(victoryCelebrationDuration);
    }

    void PlayVictorySound()
    {
        if (SoundManager.Instance != null)
        {
            if (victorySoundClip != null) SoundManager.Instance.PlaySound(victorySoundClip, 1f);
            else SoundManager.Instance.PlayPositiveGateSound();
        }
    }

    void HandleLevelComplete()
    {
        // Implement level completion logic here
        // Example:
        // UnityEngine.SceneManagement.SceneManager.LoadScene("NextLevel");
    }

    protected override void SpawnEnemyArmy()
    {
        base.SpawnEnemyArmy();
        ApplyBossModifications();
    }

    public new void RemoveEnemySoldier(Transform soldier)
    {
        if (enemySoldiers.Contains(soldier))
        {
            if (soldier != null)
            {
                soldier.DOKill();
                DOTween.Kill(soldier);
            }

            enemySoldiers.Remove(soldier);

            if (soldier != null)
            {
                soldier.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    if (soldier != null) Destroy(soldier.gameObject);
                });
            }
        }
    }

    [ContextMenu("Force Boss Victory")]
    public void ForceVictory()
    {
        if (!hasTriggeredVictory) StartCoroutine(HandleVictory());
    }

    public bool IsVictoryAchieved()
    {
        return hasTriggeredVictory;
    }

    void OnDestroy()
    {
        KillAllBossDOTweenAnimations();
    }

    public void KillAllBossDOTweenAnimations()
    {
        KillAllEnemyDOTweenAnimations();

        foreach (Transform soldier in enemySoldiers)
        {
            if (soldier != null)
            {
                DOTween.Kill(soldier);
                soldier.DOKill();
                DOTween.Kill(soldier, false);
            }
        }

        if (transform != null)
        {
            transform.DOKill();
            DOTween.Kill(transform);
        }

        if (victoryParticleEffect != null)
            DOTween.Kill(victoryParticleEffect.transform);
    }
}
