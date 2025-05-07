using UnityEngine;

public class EnemyBullets : MonoBehaviour
{
  public float speed = 5f;
  public Vector2 direction = Vector2.right;

  public void Initialize(Vector2 dir)
  {
    direction = dir.normalized;
    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
  }

  void Update()
  {
    transform.Translate(direction * speed * Time.deltaTime, Space.World);
    if (OutOfBounds())
    {
      BulletPool.Instance.ReturnBullet(gameObject);
    }
  }

  private bool OutOfBounds()
  {
    Vector3 pos = transform.position;
    return Mathf.Abs(pos.x) > 30f || Mathf.Abs(pos.y) > 30f;
  }

  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.CompareTag("Player"))
    {
      HealthSystem hs = collision.GetComponent<HealthSystem>();
      if (hs != null)
      {
        hs.TakeDamage(1);
      }
      BulletPool.Instance.ReturnBullet(gameObject);
    }
  }

}