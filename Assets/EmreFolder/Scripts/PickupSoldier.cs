using UnityEngine;
using DG.Tweening;

public class PickupSoldier : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float detectionRadius = 3f;
    public float walkSpeed = 2f;
    public bool destroyAfterPickup = true;

    [Header("Animation Settings")]
    public float joinAnimationDuration = 0.8f;

    [Header("Visual Feedback")]
    public GameObject recruitParticleEffect;
    public bool idleBounce = true;
    public float bounceHeight = 0.2f;
    public float bounceSpeed = 1.5f;

    [Header("Detection Visualization")]
    public bool showDetectionRadius = true;

    private enum SoldierState
    {
        Idle,
        Detected,
        WalkingToArmy,
        Joining
    }

    private SoldierState currentState = SoldierState.Idle;
    private Transform playerTransform;
    private ArmyManager armyManager;
    private Vector3 originalPosition;
    private bool hasBeenRecruited = false;

    void Start()
    {
        originalPosition = transform.position;
        FindPlayerAndArmyManager();

        if (idleBounce)
        {
            StartIdleBounce();
        }

        SetupSoldierComponents();
    }

    void Update()
    {
        if (hasBeenRecruited || playerTransform == null || armyManager == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case SoldierState.Idle:
                if (distanceToPlayer <= detectionRadius) TransitionToDetected();
                break;

            case SoldierState.Detected:
                if (distanceToPlayer > detectionRadius) TransitionToIdle();
                else if (distanceToPlayer <= detectionRadius * 0.5f) TransitionToWalkingToArmy();
                break;

            case SoldierState.WalkingToArmy:
                WalkTowardsArmy();
                break;
        }
    }

    void FindPlayerAndArmyManager()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            armyManager = playerObj.GetComponent<ArmyManager>() ?? playerObj.GetComponentInChildren<ArmyManager>();
        }

        if (armyManager == null)
        {
            armyManager = FindFirstObjectByType<ArmyManager>();
            if (armyManager != null) playerTransform = armyManager.player;
        }
    }

    void SetupSoldierComponents()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)col).radius = 0.5f;
        }

        if (!col.isTrigger) col.isTrigger = true;

        ArmySoldier soldierScript = GetComponent<ArmySoldier>() ?? gameObject.AddComponent<ArmySoldier>();
        soldierScript.canDie = false;
    }

    void StartIdleBounce()
    {
        transform.DOMoveY(originalPosition.y + bounceHeight, bounceSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    void TransitionToDetected()
    {
        if (currentState == SoldierState.Detected) return;

        currentState = SoldierState.Detected;
        transform.DOKill();

        transform.DOMoveY(originalPosition.y + bounceHeight * 1.5f, bounceSpeed * 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        PlayRecruitmentSound("detected");
    }

    void TransitionToIdle()
    {
        if (currentState == SoldierState.Idle) return;

        currentState = SoldierState.Idle;
        transform.DOKill();
        transform.DOMove(originalPosition, 0.5f).SetEase(Ease.OutQuart);

        if (idleBounce)
        {
            DOVirtual.DelayedCall(0.5f, () => {
                if (currentState == SoldierState.Idle) StartIdleBounce();
            });
        }
    }

    void TransitionToWalkingToArmy()
    {
        if (currentState == SoldierState.WalkingToArmy) return;

        currentState = SoldierState.WalkingToArmy;
        transform.DOKill();
        PlayRecruitmentSound("walking");
    }

    void WalkTowardsArmy()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        transform.position += directionToPlayer * walkSpeed * Time.deltaTime;

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= 1.5f) JoinArmy();
    }

    void JoinArmy()
    {
        if (hasBeenRecruited) return;

        hasBeenRecruited = true;
        currentState = SoldierState.Joining;

        transform.DOKill();

        ArmySoldier soldierScript = GetComponent<ArmySoldier>();
        if (soldierScript != null)
        {
            soldierScript.canDie = true;
            soldierScript.armyManager = armyManager;
        }

        if (recruitParticleEffect != null)
        {
            GameObject effect = Instantiate(recruitParticleEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        PlayRecruitmentSound("joined");
        StartCoroutine(JoinArmyAnimation());
    }

    System.Collections.IEnumerator JoinArmyAnimation()
    {
        if (armyManager != null) armyManager.AddExistingSoldier(transform);
        yield return new WaitForSeconds(0.1f);

        if (destroyAfterPickup) Destroy(this);
    }

    void PlayRecruitmentSound(string eventType)
    {
        if (SoundManager.Instance == null) return;

        if (eventType == "joined")
        {
            SoundManager.Instance.PlayPositiveGateSound();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenRecruited) return;
        if (other.CompareTag("Player"))
        {
            if (currentState == SoldierState.Idle || currentState == SoldierState.Detected)
            {
                TransitionToWalkingToArmy();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDetectionRadius) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.5f);
    }

    [ContextMenu("Force Recruit Soldier")]
    public void ForceRecruit()
    {
        if (!hasBeenRecruited && armyManager != null)
        {
            JoinArmy();
        }
    }
}
