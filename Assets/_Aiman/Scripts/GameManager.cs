using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }

  public float survivalTime = 0f;
  public int currentWave = 0;
  public float timeBetweenWaves = 10f;

  private float waveTimer = 0f;

  [SerializeField] public EnemySpawn enemySpawner;
  [SerializeField] private GameObject bossPrefab;
  [SerializeField] private Transform bossSpawnPoint;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  void Update()
  {
    survivalTime += Time.deltaTime;
    waveTimer += Time.deltaTime;

    if (waveTimer >= timeBetweenWaves)
    {
      waveTimer = 0f;
      SpawnWave();
      currentWave++;
    }
  }

  public int GetDifficultyStage()
  {
    if (survivalTime < 30f) return 0;
    else if (survivalTime < 60f) return 1;
    else return 2;
  }

  void SpawnWave()
  {
    int difficulty = GetDifficultyStage();

    if (currentWave % 5 == 0 && currentWave != 0)
    {
      SpawnBoss();
    }
    else
    {
      int enemyCount = 3 + difficulty * 2; // More enemies per difficulty
      enemySpawner.SpawnWave(enemyCount, difficulty);
    }

    Debug.Log($"Wave {currentWave} spawned (Difficulty Stage: {difficulty})");
  }

  void SpawnBoss()
  {
    Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
    Debug.Log("[Boss Spawned]");
  }
}