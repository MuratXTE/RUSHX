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

    private Vector3 offset;
    private Quaternion initialRotation;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
#if UNITY_2023_1_OR_NEWER
                ArmyManager armyManager = FindFirstObjectByType<ArmyManager>();
#else
                ArmyManager armyManager = FindObjectOfType<ArmyManager>();
#endif
                if (armyManager != null)
                {
                    target = armyManager.player;
                }
            }

            if (target == null)
            {
                enabled = false;
                return;
            }
        }

        initialRotation = transform.rotation;

        if (autoCalculateOffset)
        {
            offset = transform.position - target.position;
        }
        else
        {
            offset = manualOffset;
        }
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

        Vector3 targetPosition = target.position + offset;

        if (useSmoothFollow && followSpeed > 0)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        if (maintainRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (autoCalculateOffset && target != null)
        {
            offset = transform.position - target.position;
        }
    }

    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = Mathf.Clamp(newSpeed, 0f, 20f);
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        autoCalculateOffset = false;
    }

    public void EnableFollow() => followTarget = true;
    public void DisableFollow() => followTarget = false;
    public void ToggleFollow() => followTarget = !followTarget;

    public Vector3 GetCurrentOffset() => offset;

    public float GetDistanceToTarget()
    {
        if (target == null) return 0f;
        return Vector3.Distance(transform.position, target.position);
    }

    public void ResetToTarget()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        if (maintainRotation)
        {
            transform.rotation = initialRotation;
        }
    }

    void OnDrawGizmos()
    {
        if (target == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 0.5f);

        Vector3 targetPos = target.position + offset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 0.3f);
    }
}
