using UnityEngine;

public class Enemy : MonoBehaviour
{
  public GameObject bulletPrefab;
  public Transform bulletSpawnPoint;
  public float fireRate = 1f;
  private float cooldown;

  private HealthSystem healthSystem;



  [SerializeField] private float moveSpeed = 2f;
  [SerializeField] private float patrolRange = 10f;
  private Vector3 startPosition;
  private enum Axis { X, Y }
  private Axis patrolAxis;

  private void Awake()
  {
    healthSystem = GetComponent<EnemyHealth>();
    Animator animator = GetComponent<Animator>();
    animator.speed = .5f;
  }

  private void Start()
  {
    AdjustDifficulty();
    startPosition = transform.position;

    // Choose patrol axis based on screen position
    Vector2 screenPos = Camera.main.WorldToViewportPoint(transform.position);
    if (screenPos.y > 0.9f || screenPos.y < 0.1f)
      patrolAxis = Axis.X;
    else
      patrolAxis = Axis.Y;
  }


  private void OnEnable()
  {
    if (healthSystem != null)
    {
      healthSystem.OnDeath.AddListener(OnDeath);
    }
  }

  private void OnDisable()
  {
    if (healthSystem != null)
    {
      healthSystem.OnDeath.RemoveListener(OnDeath);
    }
  }

  private void OnDeath()
  {
    GameManager.Instance?.enemySpawner?.OnEnemyDestroyed();
    gameObject.SetActive(false);
  }

  private void Update()
  {
    Patrol();
    HandleShooting();
  }

  void ShootAtPlayer()
  {
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null || bulletSpawnPoint == null) return;

    Vector2 shootDir = (player.transform.position - bulletSpawnPoint.position).normalized;

    GameObject b = BulletPool.Instance.GetBullet(bulletSpawnPoint.position, Quaternion.identity);
    b.GetComponent<EnemyBullets>().Initialize(shootDir);
  }

  private void Patrol()
  {
    Vector3 newPos = startPosition;

    if (patrolAxis == Axis.X)
      newPos.x += Mathf.PingPong(Time.time * moveSpeed, patrolRange) - patrolRange / 2f;
    else
      newPos.y += Mathf.PingPong(Time.time * moveSpeed, patrolRange) - patrolRange / 2f;

    transform.position = newPos;
  }

  private void HandleShooting()
  {
    cooldown -= Time.deltaTime;
    if (cooldown <= 0f)
    {
      ShootAtPlayer();
      cooldown = 1f / fireRate;
    }
  }
  void AdjustDifficulty()
  {
    var upgradeType = GameManager.Instance.GetCurrentUpgrade();
    Debug.Log($"[AdjustDifficulty] Upgrade Type: {upgradeType}");

    if (upgradeType == GameManager.DifficultyUpgradeType.FireRate)
    {
      fireRate *= 1.2f; // Increase fire rate by 20%
      Debug.Log($"[FireRate] New fire rate: {fireRate}");
    }
    else if (upgradeType == GameManager.DifficultyUpgradeType.Health)
    {
      EnemyHealth enemyHealth = healthSystem as EnemyHealth;
      if (enemyHealth != null)
      {
        enemyHealth.MaxHealth = Mathf.CeilToInt(enemyHealth.MaxHealth * 1.3f);
        enemyHealth.CurrentHealth = enemyHealth.MaxHealth;
        Debug.Log($"[Health] New MaxHealth: {enemyHealth.MaxHealth}");
      }

    }
  }
}
