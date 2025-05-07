using UnityEngine;

public class Enemy : MonoBehaviour
{
  public GameObject bulletPrefab;
  public Transform bulletSpawnPoint;
  public float fireRate = 1f;
  private float cooldown;

  private HealthSystem healthSystem;

  private void Awake()
  {
    healthSystem = GetComponent<HealthSystem>();
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

  void Update()
  {
    cooldown -= Time.deltaTime;
    if (cooldown <= 0f)
    {
      ShootAtPlayer();
      cooldown = 1f / fireRate;
    }
  }

  void ShootAtPlayer()
  {
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null || bulletSpawnPoint == null) return;

    Vector2 shootDir = (player.transform.position - bulletSpawnPoint.position).normalized;

    GameObject b = BulletPool.Instance.GetBullet(bulletSpawnPoint.position, Quaternion.identity);
    b.GetComponent<EnemyBullets>().Initialize(shootDir);
  }
}
