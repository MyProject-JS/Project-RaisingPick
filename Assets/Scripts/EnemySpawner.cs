using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [Tooltip("스폰할 적의 유형(EnemyData) 목록입니다.")]
    [SerializeField] private List<EnemyData> spawnableEnemyTypes;
    [SerializeField] private float initialSpawnRate = 1.5f;
    [Tooltip("코어로부터의 생성 반경(월드 단위)")]
    [SerializeField] private float spawnRadius = 20f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseInterval = 10f;
    [SerializeField] private float minSpawnRate = 0.3f;
    [SerializeField] private float spawnRateReduction = 0.1f;
    [SerializeField] private float initialEnemySpeed = 5f;
    [SerializeField] private float maxEnemySpeed = 15f;
    [SerializeField] private float speedIncrease = 0.5f;

    private float currentSpawnRate;
    private float currentEnemySpeed;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        currentEnemySpeed = initialEnemySpeed;
        
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(IncreaseDifficultyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentSpawnRate);

            // 스폰 가능한 적 목록이 비어있는지 확인합니다.
            if (spawnableEnemyTypes == null || spawnableEnemyTypes.Count == 0)
            {
                Debug.LogWarning("스폰할 적 유형이 없습니다. EnemySpawner의 목록을 확인해주세요.");
                continue; // 다음 프레임까지 기다리지 않고 다음 루프로 넘어갑니다.
            }

            // 목록에서 무작위로 적 유형을 선택합니다.
            EnemyData chosenEnemyData = spawnableEnemyTypes[Random.Range(0, spawnableEnemyTypes.Count)];
            
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // 선택된 적의 poolType을 사용하여 오브젝트 풀에서 가져옵니다.
            GameObject enemyObject = ObjectPooler.Instance.SpawnFromPool(chosenEnemyData.poolType, spawnPosition, Quaternion.identity);

            if (enemyObject != null)
            {
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    // 적에게 EnemyData와 현재 난이도가 적용된 속도, 생성 반경을 전달합니다.
                    enemy.Initialize(chosenEnemyData, currentEnemySpeed, spawnRadius);
                }
            }
        }
    }

    private IEnumerator IncreaseDifficultyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            if (currentSpawnRate > minSpawnRate)
            {
                currentSpawnRate = Mathf.Max(currentSpawnRate - spawnRateReduction, minSpawnRate);
            }

            if (currentEnemySpeed < maxEnemySpeed)
            {
                currentEnemySpeed = Mathf.Min(currentEnemySpeed + speedIncrease, maxEnemySpeed);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(randomAngle) * spawnRadius;
        float y = Mathf.Sin(randomAngle) * spawnRadius;
        return new Vector3(x, y, 0);
    }
    
    public void ResetSpawner()
    {
        StopAllCoroutines();
        currentSpawnRate = initialSpawnRate;
        currentEnemySpeed = initialEnemySpeed;
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(IncreaseDifficultyRoutine());
    }
}
