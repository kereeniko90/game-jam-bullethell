using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance { get; private set; }
  public float survivalTime = 0f;

  void Awake()
  {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
  }

  void Update()
  {
    survivalTime += Time.deltaTime;
  }

  public int GetDifficultyStage()
  {
    if (survivalTime < 30f) return 0;
    else if (survivalTime < 60f) return 1;
    else return 2;
  }
}
