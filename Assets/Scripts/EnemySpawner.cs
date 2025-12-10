using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float initialSpawnRate = 1.5f;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseInterval = 10f; // 난이도 증가 간격 (초)
    [SerializeField] private float minSpawnRate = 0.3f; // 최소 생성 간격
    [SerializeField] private float spawnRateReduction = 0.1f; // 간격마다 감소할 생성 시간
    [SerializeField] private float initialEnemySpeed = 5f; // 초기 적 속도
    [SerializeField] private float maxEnemySpeed = 15f; // 최대 적 속도
    [SerializeField] private float speedIncrease = 0.5f; // 간격마다 증가할 속도

    private Camera mainCamera;
    private float currentSpawnRate;
    private float currentEnemySpeed;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Please tag a camera as 'MainCamera'.");
            return; // Stop execution if no camera is found
        }

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

            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemyObject = ObjectPooler.Instance.SpawnFromPool(enemyTag, spawnPosition, Quaternion.identity);

            if (enemyObject != null)
            {
                Enemy enemy = enemyObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.SetMoveSpeed(currentEnemySpeed);
                }
            }
        }
    }

    private IEnumerator IncreaseDifficultyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            // Decrease spawn rate
            if (currentSpawnRate > minSpawnRate)
            {
                currentSpawnRate = Mathf.Max(currentSpawnRate - spawnRateReduction, minSpawnRate);
            }

            // Increase enemy speed
            if (currentEnemySpeed < maxEnemySpeed)
            {
                currentEnemySpeed = Mathf.Min(currentEnemySpeed + speedIncrease, maxEnemySpeed);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int edge = Random.Range(0, 4);
        Vector2 viewportPoint = Vector2.zero;

        switch (edge)
        {
            case 0: // Top
                viewportPoint = new Vector2(Random.Range(0f, 1f), 1 + spawnOffset);
                break;
            case 1: // Bottom
                viewportPoint = new Vector2(Random.Range(0f, 1f), 0 - spawnOffset);
                break;
            case 2: // Left
                viewportPoint = new Vector2(0 - spawnOffset, Random.Range(0f, 1f));
                break;
            case 3: // Right
                viewportPoint = new Vector2(1 + spawnOffset, Random.Range(0f, 1f));
                break;
        }

        Vector3 worldPoint = mainCamera.ViewportToWorldPoint(viewportPoint);
        worldPoint.z = 0;

        return worldPoint;
    }
    public void ResetSpawner()
    {
        // 모든 코루틴을 중지하여 기존의 적 생성 및 난이도 증가를 멈춤
        StopAllCoroutines();

        // 난이도 관련 변수들을 초기값으로 리셋
        currentSpawnRate = initialSpawnRate;
        currentEnemySpeed = initialEnemySpeed;
        
        // 초기화된 값으로 코루틴을 다시 시작
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(IncreaseDifficultyRoutine());
    }
}
