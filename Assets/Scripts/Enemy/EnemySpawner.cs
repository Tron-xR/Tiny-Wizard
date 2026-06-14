using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 3;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private int maxWaves = 0;

    [Header("Difficulty Scaling")]
    [SerializeField] private int enemiesPerWaveIncrease = 1;
    [SerializeField] private float waveDelayReduction = 0.25f;
    [SerializeField] private float minWaveDelay = 2f;

    public System.Action<int> OnWaveStarted;
    public System.Action OnAllWavesComplete;

    private int currentWave = 0;
    private int aliveCount = 0;
    private List<EnemyHealth> spawnedEnemies = new List<EnemyHealth>();
    private bool isSpawning = false;
    private bool allWavesDone = false;

    public int CurrentWave => currentWave;
    public int AliveCount => aliveCount;
    public bool AllWavesDone => allWavesDone;

    private void OnEnable()
    {
        StartNextWave();
    }

    private void OnDisable()
    {
        foreach (EnemyHealth enemy in spawnedEnemies)
        {
            if (enemy != null)
                enemy.OnDeath -= OnEnemyKilled;
        }
        spawnedEnemies.Clear();
    }

    [ContextMenu("Start Next Wave")]
    public void StartNextWave()
    {
        if (isSpawning) return;
        if (maxWaves > 0 && currentWave >= maxWaves)
        {
            allWavesDone = true;
            OnAllWavesComplete?.Invoke();
            return;
        }

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        currentWave++;

        int enemiesThisWave = baseEnemiesPerWave + (currentWave - 1) * enemiesPerWaveIncrease;
        OnWaveStarted?.Invoke(currentWave);

        for (int i = 0; i < enemiesThisWave; i++)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) break;
            if (spawnPoints == null || spawnPoints.Length == 0) break;

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Transform point = spawnPoints[i % spawnPoints.Length];

            GameObject instance = Instantiate(prefab, point.position, point.rotation);
            EnemyHealth health = instance.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.OnDeath += OnEnemyKilled;
                spawnedEnemies.Add(health);
                aliveCount++;
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
    }

    private void OnEnemyKilled()
    {
        aliveCount--;
        if (aliveCount <= 0 && !isSpawning)
        {
            float delay = Mathf.Max(minWaveDelay, timeBetweenWaves - (currentWave - 1) * waveDelayReduction);
            Invoke(nameof(StartNextWave), delay);
        }
    }
}
