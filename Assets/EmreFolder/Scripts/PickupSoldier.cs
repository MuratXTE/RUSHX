using UnityEngine;
using DG.Tweening;

public class PickupSoldier : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Detection radius for when player gets close")]
    public float detectionRadius = 3f;
    
    [Tooltip("How fast the soldier walks toward the army")]
    public float walkSpeed = 2f;
    
    [Tooltip("Should this soldier be destroyed after pickup?")]
    public bool destroyAfterPickup = true;
    
    [Header("Animation Settings")]
    [Tooltip("Duration for the join army animation")]
    public float joinAnimationDuration = 0.8f;
    
    [Header("Visual Feedback")]
    [Tooltip("Particle effect when soldier gets recruited")]
    public GameObject recruitParticleEffect;
    
    [Tooltip("Should soldier bounce while idle?")]
    public bool idleBounce = true;
    
    [Tooltip("Height of idle bounce")]
    public float bounceHeight = 0.2f;
    
    [Tooltip("Speed of idle bounce")]
    public float bounceSpeed = 1.5f;
    
    [Header("Detection Visualization")]
    [Tooltip("Show detection radius in scene view")]
    public bool showDetectionRadius = true;
    
    private enum SoldierState
    {
        Idle,           // Waiting to be picked up
        Detected,       // Player is nearby, soldier is excited
        WalkingToArmy,  // Moving toward the army
        Joining         // Playing join animation
    }
    
    private SoldierState currentState = SoldierState.Idle;
    private Transform playerTransform;
    private ArmyManager armyManager;
    private Vector3 originalPosition;
    private bool hasBeenRecruited = false;
    
    void Start()
    {
        // Store original properties
        originalPosition = transform.position;
        
        // Find the player and army manager
        FindPlayerAndArmyManager();
        
        // Start idle animation if enabled
        if (idleBounce)
        {
            StartIdleBounce();
        }
        
        // Ensure the soldier has proper components
        SetupSoldierComponents();
    }
    
    void Update()
    {
        if (hasBeenRecruited || playerTransform == null || armyManager == null) 
            return;
            
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        switch (currentState)
        {
            case SoldierState.Idle:
                if (distanceToPlayer <= detectionRadius)
                {
                    TransitionToDetected();
                }
                break;
                
            case SoldierState.Detected:
                if (distanceToPlayer > detectionRadius)
                {
                    TransitionToIdle();
                }
                else if (distanceToPlayer <= detectionRadius * 0.5f) // Closer threshold to start walking
                {
                    TransitionToWalkingToArmy();
                }
                break;
                
            case SoldierState.WalkingToArmy:
                WalkTowardsArmy();
                break;
        }
    }
    
    void FindPlayerAndArmyManager()
    {
        // Try to find player by tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            armyManager = playerObj.GetComponent<ArmyManager>();
            
            if (armyManager == null)
            {
                armyManager = playerObj.GetComponentInChildren<ArmyManager>();
            }
        }
        
        // If still not found, search for ArmyManager in scene
        if (armyManager == null)
        {
            armyManager = FindFirstObjectByType<ArmyManager>();
            if (armyManager != null)
            {
                playerTransform = armyManager.player;
            }
        }
        
        if (armyManager == null || playerTransform == null)
        {
            Debug.LogWarning($"PickupSoldier on {gameObject.name} couldn't find Player or ArmyManager!");
        }
    }
    
    void SetupSoldierComponents()
    {
        // Ensure the pickup soldier has a collider for trigger detection
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)col).radius = 0.5f;
        }
        
        // Make sure it's set as a trigger for detection
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
        
        // Add ArmySoldier component if it doesn't exist (for item application later)
        ArmySoldier soldierScript = GetComponent<ArmySoldier>();
        if (soldierScript == null)
        {
            soldierScript = gameObject.AddComponent<ArmySoldier>();
        }
        
        // Temporarily disable the ArmySoldier death mechanics until recruited
        soldierScript.canDie = false;
    }
    
    void StartIdleBounce()
    {
        // Gentle up and down floating animation
        transform.DOMoveY(originalPosition.y + bounceHeight, bounceSpeed)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    void TransitionToDetected()
    {
        if (currentState == SoldierState.Detected) return;
        
        currentState = SoldierState.Detected;
        Debug.Log($"Soldier {gameObject.name} detected player!");
        
        // Kill previous animations
        transform.DOKill();
        
        // Add excited bouncing (only position animation, no scaling)
        transform.DOMoveY(originalPosition.y + bounceHeight * 1.5f, bounceSpeed * 0.7f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
        // Play excited sound effect if SoundManager exists
        PlayRecruitmentSound("detected");
    }
    
    void TransitionToIdle()
    {
        if (currentState == SoldierState.Idle) return;
        
        currentState = SoldierState.Idle;
        Debug.Log($"Soldier {gameObject.name} back to idle");
        
        // Kill animations and return to idle
        transform.DOKill();
        
        // Return to original position (no scaling)
        transform.DOMove(originalPosition, 0.5f).SetEase(Ease.OutQuart);
        
        // Resume idle bounce
        if (idleBounce)
        {
            DOVirtual.DelayedCall(0.5f, () => {
                if (currentState == SoldierState.Idle)
                {
                    StartIdleBounce();
                }
            });
        }
    }
    
    void TransitionToWalkingToArmy()
    {
        if (currentState == SoldierState.WalkingToArmy) return;
        
        currentState = SoldierState.WalkingToArmy;
        Debug.Log($"Soldier {gameObject.name} starting to walk to army!");
        
        // Kill previous animations
        transform.DOKill();
        
        // Play walking sound
        PlayRecruitmentSound("walking");
    }
    
    void WalkTowardsArmy()
    {
        if (playerTransform == null) return;
        
        // Calculate direction to player
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Move towards player
        transform.position += directionToPlayer * walkSpeed * Time.deltaTime;
        
        // Rotate to face the player
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        
        // Check if close enough to join army
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= 1.5f) // Close enough to join
        {
            JoinArmy();
        }
    }
    
    void JoinArmy()
    {
        if (hasBeenRecruited) return;
        
        hasBeenRecruited = true;
        currentState = SoldierState.Joining;
        
        Debug.Log($"Soldier {gameObject.name} joining the army!");
        
        // Kill all animations
        transform.DOKill();
        
        // Enable ArmySoldier component for normal army behavior
        ArmySoldier soldierScript = GetComponent<ArmySoldier>();
        if (soldierScript != null)
        {
            soldierScript.canDie = true; // Now it can die like normal army soldiers
            soldierScript.armyManager = armyManager;
        }
        
        // Play recruitment particle effect
        if (recruitParticleEffect != null)
        {
            GameObject effect = Instantiate(recruitParticleEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Play recruitment sound
        PlayRecruitmentSound("joined");
        
        // Add soldier to army through the army manager's system
        StartCoroutine(JoinArmyAnimation());
    }
    
    System.Collections.IEnumerator JoinArmyAnimation()
    {
        // Add this soldier to the army using the army manager's method
        if (armyManager != null)
        {
            armyManager.AddExistingSoldier(transform);
            Debug.Log($"Successfully added soldier {gameObject.name} to army!");
        }
        
        yield return new WaitForSeconds(0.1f);
        
        // Cleanup this pickup component since it's no longer needed
        if (destroyAfterPickup)
        {
            // Destroy this pickup component but keep the soldier GameObject
            Destroy(this);
        }
    }
    
    void PlayRecruitmentSound(string eventType)
    {
        if (SoundManager.Instance != null)
        {
            switch (eventType)
            {
                case "detected":
                    // Play a light notification sound when soldier detects player
                    // You can add a specific method to SoundManager for this
                    break;
                    
                case "walking":
                    // Play footstep or movement sound
                    break;
                    
                case "joined":
                    // Play positive sound when soldier joins army
                    SoundManager.Instance.PlayPositiveGateSound(); // Reuse existing positive sound
                    break;
            }
        }
    }
    
    // Trigger detection for when player walks through the soldier
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenRecruited) return;
        
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            // If player walks directly into soldier, immediately start walking
            if (currentState == SoldierState.Idle || currentState == SoldierState.Detected)
            {
                TransitionToWalkingToArmy();
            }
        }
    }
    
    // Visualize detection radius in editor
    void OnDrawGizmosSelected()
    {
        if (showDetectionRadius)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius * 0.5f);
        }
    }
    
    // Public method to manually recruit this soldier (for testing or special events)
    [ContextMenu("Force Recruit Soldier")]
    public void ForceRecruit()
    {
        if (!hasBeenRecruited && armyManager != null)
        {
            JoinArmy();
        }
    }
}
