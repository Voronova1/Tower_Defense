using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject mobPrefab;
        public int mobCount;
        public float spawnRate;
    }

    public Wave[] waves;
    public Transform[] spawnPoints;
    public float timeBetweenWaves = 5f;
    public bool loopWaves = false;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    public bool AllWavesCompleted { get; private set; } = false;

    void Start()
    {
        StartCoroutine(WaveSpawnLoop());
    }

    IEnumerator WaveSpawnLoop()
    {
        while (currentWaveIndex < waves.Length)
        {
            Wave currentWave = waves[currentWaveIndex];
            yield return StartCoroutine(SpawnWave(currentWave));

            if (currentWaveIndex < waves.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }

            currentWaveIndex++;
        }

        AllWavesCompleted = true;

        if (loopWaves)
        {
            currentWaveIndex = 0;
            AllWavesCompleted = false;
            StartCoroutine(WaveSpawnLoop());
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;

        for (int i = 0; i < wave.mobCount; i++)
        {
            SpawnMob(wave.mobPrefab);
            yield return new WaitForSeconds(1f / wave.spawnRate);
        }

        isSpawning = false;
    }

    void SpawnMob(GameObject mobPrefab)
    {
        if (spawnPoints.Length == 0) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}