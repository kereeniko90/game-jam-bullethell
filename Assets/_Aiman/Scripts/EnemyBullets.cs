using UnityEngine;

public class EnemyBullets : MonoBehaviour
{
  public float speed = 5f;
  public Vector2 direction = Vector2.right;

  void Update()
  {
    transform.Translate(direction * speed * Time.deltaTime);

  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.CompareTag("Player")) //TODO:change name and add the damage done to player
    {
      Destroy(gameObject);
    }
  }

  // TODO: change bullet orientation based on position/direction of enemy

}
