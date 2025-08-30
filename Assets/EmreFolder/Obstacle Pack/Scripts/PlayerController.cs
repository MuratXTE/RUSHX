using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sideSpeed = 3f;
    public bool autoMoveForward = true;
    public float maxSideMovement = 3f; // Limit for left/right movement
    
    [Header("Input Settings")]
    public bool enablePCInput = true; // For testing
    public bool enableMobileInput = true;
    
    [Header("Combat Settings")]
    public bool isInCombat = false; // Disables all input during combat
    
    [Header("Mobile Input")]
    public float touchSensitivity = 2f;
    
    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 lastTouchPosition;
    private bool isTouching = false;
    private float currentSidePosition = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Set up rigidbody for smooth movement
        rb.freezeRotation = true;
        rb.useGravity = true;
        
        startPosition = transform.position;
        
        // Ensure player has the Player tag
        if (!CompareTag("Player"))
        {
            tag = "Player";
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void FixedUpdate()
    {
        MovePlayer();
    }
    
    void HandleInput()
    {
        // Don't process any input if in combat
        if (isInCombat) return;
        
        float horizontalInput = 0f;
        
        // PC Input (for testing)
        if (enablePCInput)
        {
            horizontalInput = Input.GetAxis("Horizontal");
        }
        
        // Mobile Touch Input
        if (enableMobileInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                isTouching = true;
                lastTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
            }
            else if (touch.phase == TouchPhase.Moved && isTouching)
            {
                Vector3 currentTouchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
                float deltaX = (currentTouchPosition.x - lastTouchPosition.x) * touchSensitivity;
                horizontalInput = Mathf.Clamp(deltaX, -1f, 1f);
                lastTouchPosition = currentTouchPosition;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }
        
        // Apply horizontal movement with limits
        currentSidePosition += horizontalInput * sideSpeed * Time.deltaTime;
        currentSidePosition = Mathf.Clamp(currentSidePosition, -maxSideMovement, maxSideMovement);
    }
    
    void MovePlayer()
    {
        Vector3 targetPosition = transform.position;
        
        // Forward movement (auto-run) - also disabled during combat
        if (autoMoveForward && !isInCombat)
        {
            targetPosition += Vector3.forward * moveSpeed * Time.fixedDeltaTime;
        }
        
        // Horizontal movement (limited) - also disabled during combat
        if (!isInCombat)
        {
            targetPosition.x = startPosition.x + currentSidePosition;
        }
        
        // Apply movement
        rb.MovePosition(targetPosition);
    }
    
    // Method to temporarily stop/start auto movement (useful for game states)
    public void SetAutoMove(bool enable)
    {
        autoMoveForward = enable;
    }
    
    // Method to set combat state (disables all input and movement)
    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        if (inCombat)
        {
            // Reset touch state when entering combat
            isTouching = false;
        }
    }
    
    // Method to reset player position (useful for respawn)
    public void ResetPosition()
    {
        transform.position = startPosition;
        currentSidePosition = 0f;
        rb.linearVelocity = Vector3.zero;
    }
}
