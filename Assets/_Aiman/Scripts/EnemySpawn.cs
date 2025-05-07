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
  private int lastSpawnSide = 0; // 0=Top, 1=Bottom, 2=Left, 3=Right

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

    // Add a small random offset to vary spawn position naturally
    Vector3 offset = Vector3.zero;
    switch (lastSpawnSide)
    {
      case 0: // Top
        offset = new Vector3(Random.Range(-25, 25), Random.Range(-0.5f, 0.5f), 0f);
        break;
      case 1: // Bottom
        offset = new Vector3(Random.Range(-25, 25), Random.Range(-0.5f, 0.5f), 0f);
        break;
      case 2: // Left
        offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-25, 25), 0f);
        break;
      case 3: // Right
        offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-25, 25), 0f);
        break;
    }

    Vector3 finalPosition = spawnPoint.position + offset;
    GameObject enemy = Instantiate(enemyToSpawn, finalPosition, Quaternion.identity);

    // Let enemy know which side it spawned from
    // enemy.GetComponent<Enemy>()?.SetSpawnSide(lastSpawnSide);
  }

  Transform GetRandomSpawnPoint()
  {
    int side = Random.Range(0, 4);
    lastSpawnSide = side;
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