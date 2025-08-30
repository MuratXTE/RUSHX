using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The player transform to follow")]
    public Transform target;
    
    [Header("Follow Settings")]
    [Tooltip("Should the camera follow the target automatically?")]
    public bool followTarget = true;
    
    [Tooltip("Smooth follow speed (0 = instant, higher = smoother but slower)")]
    [Range(0f, 20f)]
    public float followSpeed = 5f;
    
    [Tooltip("Use smooth following (damping) or instant following?")]
    public bool useSmoothFollow = true;
    
    [Header("Offset Settings")]
    [Tooltip("Auto-calculate offset from current camera position when game starts?")]
    public bool autoCalculateOffset = true;
    
    [Tooltip("Manual offset from target (only used if autoCalculateOffset is false)")]
    public Vector3 manualOffset = new Vector3(0, 5, -10);
    
    [Header("Advanced Settings")]
    [Tooltip("Should the camera maintain its rotation set in inspector?")]
    public bool maintainRotation = true;
    
    [Tooltip("Update in FixedUpdate for physics-based movement?")]
    public bool useFixedUpdate = false;
    
    // Private variables
    private Vector3 offset;
    private Quaternion initialRotation;
    
    void Start()
    {
        // Find target if not assigned
        if (target == null)
        {
            // Try to find player by tag first
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("CameraController: Found player by tag");
            }
            else
            {
                // Try to find ArmyManager component
                ArmyManager armyManager = FindFirstObjectByType<ArmyManager>();
                if (armyManager != null)
                {
                    target = armyManager.player;
                    Debug.Log("CameraController: Found player through ArmyManager");
                }
            }
            
            if (target == null)
            {
                Debug.LogError("CameraController: No target found! Please assign a target or tag the player with 'Player' tag.");
                enabled = false;
                return;
            }
        }
        
        // Store initial rotation to maintain camera angle
        initialRotation = transform.rotation;
        
        // Calculate or set offset
        if (autoCalculateOffset)
        {
            offset = transform.position - target.position;
            Debug.Log($"CameraController: Auto-calculated offset: {offset}");
        }
        else
        {
            offset = manualOffset;
            Debug.Log($"CameraController: Using manual offset: {offset}");
        }
        
        Debug.Log($"CameraController initialized. Following: {target.name}");
    }
    
    void Update()
    {
        if (!useFixedUpdate)
        {
            UpdateCameraPosition();
        }
    }
    
    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            UpdateCameraPosition();
        }
    }
    
    void UpdateCameraPosition()
    {
        if (target == null || !followTarget) return;
        
        // Calculate target position with offset
        Vector3 targetPosition = target.position + offset;
        
        if (useSmoothFollow && followSpeed > 0)
        {
            // Smooth following with lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            // Instant following
            transform.position = targetPosition;
        }
        
        // Maintain initial rotation if specified
        if (maintainRotation)
        {
            transform.rotation = initialRotation;
        }
    }
    
    // Public methods for runtime control
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        // Recalculate offset if auto-calculate is enabled
        if (autoCalculateOffset && target != null)
        {
            offset = transform.position - target.position;
        }
        
        Debug.Log($"CameraController: Target changed to {(newTarget != null ? newTarget.name : "null")}");
    }
    
    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = Mathf.Clamp(newSpeed, 0f, 20f);
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        autoCalculateOffset = false; // Disable auto-calculate when manually setting offset
    }
    
    public void EnableFollow()
    {
        followTarget = true;
    }
    
    public void DisableFollow()
    {
        followTarget = false;
    }
    
    public void ToggleFollow()
    {
        followTarget = !followTarget;
    }
    
    // Utility methods
    public Vector3 GetCurrentOffset()
    {
        return offset;
    }
    
    public float GetDistanceToTarget()
    {
        if (target == null) return 0f;
        return Vector3.Distance(transform.position, target.position);
    }
    
    // Method to reset camera to target with offset instantly
    public void ResetToTarget()
    {
        if (target == null) return;
        
        transform.position = target.position + offset;
        if (maintainRotation)
        {
            transform.rotation = initialRotation;
        }
    }
    
    // Context menu methods for testing
    [ContextMenu("Reset Camera Position")]
    void TestResetPosition()
    {
        ResetToTarget();
    }
    
    [ContextMenu("Toggle Follow")]
    void TestToggleFollow()
    {
        ToggleFollow();
        Debug.Log($"Camera following: {followTarget}");
    }
    
    [ContextMenu("Recalculate Offset")]
    void TestRecalculateOffset()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
            Debug.Log($"New offset calculated: {offset}");
        }
    }
    
    // Gizmos for debugging in scene view
    void OnDrawGizmos()
    {
        if (target == null) return;
        
        // Draw line from camera to target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        
        // Draw target position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 0.5f);
        
        // Draw camera target position (target + offset)
        Vector3 targetPos = target.position + offset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 0.3f);
    }
}
