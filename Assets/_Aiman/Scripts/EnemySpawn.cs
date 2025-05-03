using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
  public GameObject[] enemyPrefabs;
  public float spawnInterval = 3f;
  private float timer;

  void Update()
  {
    timer += Time.deltaTime;
    if (timer >= spawnInterval)
    {
      SpawnEnemy();
      timer = 0f;
    }
  }

  void SpawnEnemy()
  {
    int index = Random.Range(0, enemyPrefabs.Length);
    Vector2 spawnPos = new Vector2(Random.Range(-8f, 8f), 3f); // top spawn for now
    Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);
  }
  // TODO: create spawn for left right bottom
}
