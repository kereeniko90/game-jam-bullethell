using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("UI Elements")]
    [SerializeField] private Slider dashCooldownSlider;
    private Image dashBar;
    [SerializeField] private Color dashOnCooldownColor;
    [SerializeField] private Color dashReadyColor;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform upFirePoint; // Fire point when facing up
    [SerializeField] private Transform rightFirePoint; // Fire point when facing right
    [SerializeField] private Transform downFirePoint; // Fire point when facing down
    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private bool autoShoot = true;

    [Header("Direction Indicator")]
    [SerializeField] private float indicatorLength = 2f;
    [SerializeField] private Color indicatorColor = Color.red;

    [Header("Bullet Elemetns")]
    [SerializeField] private BulletManager.BulletType currentBulletType = BulletManager.BulletType.Normal;

    // Animation Parameters
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveYHash = Animator.StringToHash("MoveY");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");

    // Private variables
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    [SerializeField] private Animator animator;
    private Vector2 moveDirection;
    private Vector2 aimDirection = Vector2.up; // Default facing up
    private bool isDashing = false;
    private bool canDash = true;
    private float lastFireTime;
    private float currentDashCooldown = 0f;
    private Color dashBarColor;

    private enum FacingDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    private FacingDirection currentDirection = FacingDirection.Up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        dashBar = dashCooldownSlider.fillRect.GetComponent<Image>();

        // Set interpolation for smoother movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Make sure we have a firePoint
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not assigned! Creating a default one.");
            GameObject newFirePoint = new GameObject("FirePoint");
            newFirePoint.transform.parent = transform;
            newFirePoint.transform.localPosition = new Vector3(0, 0.5f, 0); // Slightly above center
            firePoint = newFirePoint.transform;
        }
        else if (firePoint == null && upFirePoint != null)
        {
            // Use upFirePoint as the default if firePoint is not assigned
            firePoint = upFirePoint;
        }

        if (dashCooldownSlider != null)
        {
            dashCooldownSlider.maxValue = 1f;
            dashCooldownSlider.value = 1f; // Start with full dash
        }
    }

    private void Update()
    {
        // Get input
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        UpdateAnimationParameters();

        // Update aim direction if player is moving
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            aimDirection = moveDirection.normalized;

            // Update firePoint rotation
            UpdateFirePointPosition();
            UpdateFirePointRotation();
        }

        // Draw direction indicator - will be visible in both Scene and Game views during Play mode
        Debug.DrawLine(
            transform.position,
            transform.position + new Vector3(aimDirection.x, aimDirection.y, 0) * indicatorLength,
            indicatorColor
        );

        // Draw a line from firePoint showing its forward direction
        Debug.DrawLine(
            firePoint.position,
            firePoint.position + firePoint.up * indicatorLength,
            Color.blue
        );

        // Dash input
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing)
        {
            StartDash();
        }

        // Shooting
        if (autoShoot)
        {
            if (Time.time > lastFireTime + fireRate)
            {
                Shoot();
                lastFireTime = Time.time;
            }
        }
        else if (Input.GetKey(KeyCode.Mouse0) && Time.time > lastFireTime + fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }

        if (!canDash)
        {
            currentDashCooldown += Time.deltaTime;
            float cooldownProgress = currentDashCooldown / dashCooldown;

            if (dashCooldownSlider != null)
            {
                dashCooldownSlider.value = cooldownProgress;
                if (dashBar != null)
                {
                    dashBar.color = Color.Lerp(dashOnCooldownColor, dashReadyColor, cooldownProgress);
                }
            }
        }
    }

    private void UpdateAnimationParameters()
    {
        // Set parameters for blend tree
        animator.SetFloat(moveXHash, aimDirection.x);
        animator.SetFloat(moveYHash, aimDirection.y);

        // Set IsMoving parameter (useful for transitions between idle and movement states)
        //animator.SetBool(isMovingHash, moveDirection.sqrMagnitude > 0.01f);
    }

    private void UpdateFirePointPosition()
    {
        // Determine the predominant direction (up, right, down, or left)
        FacingDirection newDirection;

        // Find the dominant direction based on x and y values
        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
        {
            // Horizontal movement is dominant
            newDirection = aimDirection.x > 0 ? FacingDirection.Right : FacingDirection.Left;
        }
        else
        {
            // Vertical movement is dominant
            newDirection = aimDirection.y > 0 ? FacingDirection.Up : FacingDirection.Down;
        }

        //Debug.Log($"AimDirection: ({aimDirection.x}, {aimDirection.y}) - New Direction: {newDirection}, Current Direction: {currentDirection}");
        // Only update if direction has changed
        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;

            // Set firePoint based on direction
            switch (currentDirection)
            {
                case FacingDirection.Up:
                    if (upFirePoint != null) firePoint = upFirePoint;
                    //Debug.Log("Switched to UP fire point");
                    break;
                case FacingDirection.Right:
                    if (rightFirePoint != null) firePoint = rightFirePoint;
                    //Debug.Log("Switched to RIGHT fire point");
                    break;
                case FacingDirection.Down:
                    if (downFirePoint != null) firePoint = downFirePoint;
                    //Debug.Log("Switched to DOWN fire point");
                    break;
                case FacingDirection.Left:
                    if (leftFirePoint != null) firePoint = leftFirePoint;
                    //Debug.Log("Switched to LEFT fire point");
                    break;
            }
        }
    }

    private void UpdateFirePointRotation()
    {
        // Calculate rotation based on aim direction
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void FixedUpdate()
    {
        // Calculate movement
        Vector2 currentPosition = rb.position;
        float speed = isDashing ? dashSpeed : moveSpeed;

        // Calculate target position
        Vector2 targetPosition = currentPosition + moveDirection * speed * Time.fixedDeltaTime;

        // If no input, ensure we're completely stopped
        if (moveDirection.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Use MovePosition for smoother movement
            rb.MovePosition(targetPosition);
        }
    }

    private void StartDash()
    {
        isDashing = true;
        canDash = false;
        currentDashCooldown = 0f;
        playerCollider.enabled = false; // Turn off collider during dash
        if (dashBar != null)
        {
            dashBar.color = dashOnCooldownColor;
        }

        // End dash after duration
        Invoke(nameof(EndDash), dashDuration);

        // Reset cooldown
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void EndDash()
    {
        isDashing = false;
        playerCollider.enabled = true; // Turn collider back on
    }

    private void ResetDashCooldown()
    {
        canDash = true;
        if (dashCooldownSlider != null)
        {
            dashCooldownSlider.value = 1f;
            if (dashBar != null)
            {
                dashBar.color = dashReadyColor;
            }
        }
    }

    private void Shoot()
    {
        GameObject bulletPrefab = BulletManager.Instance.GetBulletPrefab(currentBulletType);
        if (bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
        bulletPrefab.transform.rotation = firePoint.rotation; // Ensure bullet is facing the same direction as firePoint
    }

    public void SwitchBulletType(BulletManager.BulletType newType)
    {
        currentBulletType = newType;
    }

    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }
}