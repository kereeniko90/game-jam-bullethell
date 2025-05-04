using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
  public GameObject[] enemyPrefabs;
  public float enemyLimit = 10;
  public float enemyCount = 0;

  [Header("Spawn Points")]
  public Transform[] topSpawnPoints;
  public Transform[] bottomSpawnPoints;
  public Transform[] leftSpawnPoints;
  public Transform[] rightSpawnPoints;

  public void SpawnWave(int count, int difficulty)
  {
    for (int i = 0; i < count; i++)
    {
      if (enemyCount >= enemyLimit) break;
      SpawnEnemy(difficulty);
      enemyCount++;
    }
  }

  void SpawnEnemy(int difficulty)
  {
    int index = Random.Range(0, Mathf.Min(difficulty + 1, enemyPrefabs.Length));
    GameObject enemyToSpawn = enemyPrefabs[index];

    Transform spawnPoint = GetRandomSpawnPoint();
    Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
  }

  Transform GetRandomSpawnPoint()
  {
    int side = Random.Range(0, 4);
    Transform[] chosenArray = topSpawnPoints;

    switch (side)
    {
      case 0: chosenArray = topSpawnPoints; break;
      case 1: chosenArray = bottomSpawnPoints; break;
      case 2: chosenArray = leftSpawnPoints; break;
      case 3: chosenArray = rightSpawnPoints; break;
    }

    return chosenArray[Random.Range(0, chosenArray.Length)];
  }

  public void OnEnemyDestroyed()
  {
    enemyCount = Mathf.Max(0, enemyCount - 1);
  }
}
