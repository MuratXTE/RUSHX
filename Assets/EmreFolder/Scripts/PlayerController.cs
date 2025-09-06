using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sideSpeed = 3f;
    public bool autoMoveForward = true;
    public float maxSideMovement = 3f;

    [Header("Input Settings")]
    public bool enablePCInput = true;
    public bool enableMobileInput = true;
    public bool invertHorizontal = false; // yön ters ise true yapın

    [Header("Combat Settings")]
    public bool isInCombat = false;

    [Header("Mobile Input")]
    [Tooltip("Dokunmatik hassasiyeti. 1-3 ideal.")]
    public float touchSensitivity = 2f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector2 lastTouchPosition;   // ekran (piksel) koordinatı
    private bool isTouching = false;
    private float currentSidePosition = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        startPosition = transform.position;

        if (!CompareTag("Player")) tag = "Player";
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
        if (isInCombat) return;

        float horizontalInput = 0f;

        // Klavye
        if (enablePCInput)
        {
            horizontalInput += Input.GetAxis("Horizontal");
        }

        // Dokunmatik
        if (enableMobileInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isTouching = true;
                lastTouchPosition = touch.position; // sadece ekran pozisyonunu sakla
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isTouching)
            {
                // Ekrandaki x farkını al, ekran genişliğine oranla normalize et
                float deltaX = (touch.position.x - lastTouchPosition.x);
                float norm = deltaX / (Screen.width * 0.5f); // ~[-1, 1] aralığı
                horizontalInput += Mathf.Clamp(norm * touchSensitivity, -1f, 1f);

                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isTouching = false;
            }
        }

        // Yön ters çevrilecekse
        if (invertHorizontal) horizontalInput = -horizontalInput;

        // Zaman ve hızla ölçekle, aralığı sınırla
        currentSidePosition += horizontalInput * sideSpeed * Time.deltaTime;
        currentSidePosition = Mathf.Clamp(currentSidePosition, -maxSideMovement, maxSideMovement);
    }

    void MovePlayer()
    {
        Vector3 targetPosition = transform.position;

        if (autoMoveForward && !isInCombat)
        {
            targetPosition += Vector3.forward * moveSpeed * Time.fixedDeltaTime;
        }

        if (!isInCombat)
        {
            // Dünya x ekseninde kaydırma (transform.right yerine sabit dünya x)
            targetPosition.x = startPosition.x + currentSidePosition;
        }

        rb.MovePosition(targetPosition);
    }

    public void SetAutoMove(bool enable)
    {
        autoMoveForward = enable;
    }

    public void SetCombatState(bool inCombat)
    {
        isInCombat = inCombat;
        if (inCombat) isTouching = false;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        currentSidePosition = 0f;
        rb.linearVelocity = Vector3.zero; // Unity 6
    }
}
