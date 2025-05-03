using UnityEngine;

public class Enemy : MonoBehaviour
{
  public GameObject EnemyBullets;
  public Transform[] EnemyBulletSpawnPoint;
  public float fireRate = 1f;
  private float cooldown = 0f;

  void Update()
  {
    cooldown -= Time.deltaTime;
    if (cooldown <= 0f)
    {
      Shoot();
      cooldown = 1f / fireRate;
    }
  }

  void Shoot()
  {
    int stage = GameManager.Instance.GetDifficultyStage();
    // TODO: find way for better change in difficulty for this. Maybe just firerate and bullet spawn point amount. Type of bullets maybe stay the same for one type of enemy
    switch (stage)
    {
      case 0:
        ShootSingle();
        break;
      case 1:
        ShootSpread();
        break;
      case 2:
        ShootCircular();
        break;
    }
  }

  void ShootSingle()
  {
    // TODO:change this to object pooling instead of instantiate and destroy
    GameObject b = Instantiate(EnemyBullets, EnemyBulletSpawnPoint[0].position, Quaternion.identity);
    Destroy(b, 5f);
  }

  void ShootSpread()
  {
    foreach (var point in EnemyBulletSpawnPoint)
    {
      GameObject b = Instantiate(EnemyBullets, point.position, point.rotation);
      Destroy(b, 5f);
    }
  }

  void ShootCircular()
  {
    int count = 8;
    float angleStep = 360f / count;
    for (int i = 0; i < count; i++)
    {
      float angle = i * angleStep;
      Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
      GameObject b = Instantiate(EnemyBullets, transform.position, Quaternion.identity);
      b.GetComponent<EnemyBullets>().direction = dir;
      Destroy(b, 5f);
    }

  }
}
