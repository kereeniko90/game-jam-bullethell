using System;
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
    [SerializeField] private float firePointDistance = 0.5f; // Distance from player center
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private bool autoShoot = true;

    [Header("Direction Indicator")]
    [SerializeField] private float indicatorLength = 2f;
    [SerializeField] private Color indicatorColor = Color.red;

    [Header("Bullet Elements")]
    [SerializeField] private BulletManager.BulletType currentBulletType = BulletManager.BulletType.Normal;

    // Animation Parameters
    private readonly int moveXHash = Animator.StringToHash("MoveX");
    private readonly int moveYHash = Animator.StringToHash("MoveY");
    //private readonly int isMovingHash = Animator.StringToHash("IsMoving");

    // Animation direction enum - kept for animation purposes
    private enum FacingDirection
    {
        Up,
        Right,
        Down,
        Left
    }

    private FacingDirection currentAnimDirection = FacingDirection.Up;

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
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        dashBar = dashCooldownSlider != null ? dashCooldownSlider.fillRect.GetComponent<Image>() : null;
        mainCamera = Camera.main;

        // Set interpolation for smoother movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Make sure we have a firePoint
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint not assigned! Creating a default one.");
            GameObject newFirePoint = new GameObject("FirePoint");
            newFirePoint.transform.parent = transform;
            newFirePoint.transform.localPosition = new Vector3(0, firePointDistance, 0); // Default above player
            firePoint = newFirePoint.transform;
        }

        if (dashCooldownSlider != null)
        {
            dashCooldownSlider.maxValue = 1f;
            dashCooldownSlider.value = 1f; // Start with full dash
        }
    }

    private void Update()
    {
        // Get input for movement
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Mouse aim direction
        UpdateAimDirection();
        
        // Update animation parameters for movement
        UpdateMovementAnimation();

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

        // Update dash cooldown UI
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

    private void UpdateMovementAnimation()
    {
        if (animator != null)
        {
            // For animation purposes, we'll use aim direction for facing
            // and movement magnitude for deciding if we're moving
            animator.SetFloat(moveXHash, aimDirection.x);
            animator.SetFloat(moveYHash, aimDirection.y);
            //animator.SetBool(isMovingHash, moveDirection.sqrMagnitude > 0.01f);
            
            // Update animation direction for future use
            if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
            {
                // Horizontal direction is dominant
                currentAnimDirection = aimDirection.x > 0 ? FacingDirection.Right : FacingDirection.Left;
            }
            else
            {
                // Vertical direction is dominant
                currentAnimDirection = aimDirection.y > 0 ? FacingDirection.Up : FacingDirection.Down;
            }
        }
    }

    private void UpdateAimDirection()
    {
        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        // Calculate direction from player to mouse
        Vector2 direction = (mousePosition - transform.position).normalized;
        aimDirection = direction;

        // Update firePoint position and rotation
        UpdateFirePointPosition();
        UpdateFirePointRotation();
    }

    private void UpdateFirePointPosition()
    {
        // Position the firepoint at a fixed distance from the player in the aim direction
        firePoint.localPosition = aimDirection * firePointDistance;
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
        //playerCollider.enabled = false; // Turn off collider during dash
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
        //playerCollider.enabled = true; // Turn collider back on
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
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            // Ensure bullet is facing the same direction as firePoint
            bullet.transform.rotation = firePoint.rotation;
        }
    }

    public void SwitchBulletType(BulletManager.BulletType newType)
    {
        currentBulletType = newType;
    }

    public void SetBulletPrefab(GameObject newBulletPrefab)
    {
        bulletPrefab = newBulletPrefab;
    }

    public GameObject GetBulletPrefab()
    {
        return bulletPrefab;
    }

    public void ModifyDashDuration(float multiplier)
    {
        dashDuration *= multiplier;
    }

    public void ModifyDashCooldown(float multiplier)
    {
        dashCooldown *= multiplier;
    }

    public void ModifyMoveSpeed(float multiplier)
    {
        moveSpeed = multiplier;
    }

    public void ModifyDashSpeed(float multiplier)
    {
        dashSpeed *= multiplier;
    }

    public void ModifyFireRate(float multiplier)
    {
        fireRate *= multiplier;
    }

    public bool GetDashingStatus() {
        return isDashing;
    }
}