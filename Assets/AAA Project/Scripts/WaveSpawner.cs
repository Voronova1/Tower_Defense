using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    [Header("UI Elements")]
    public TMP_Text waveCounterText;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
    private int lastSpawnPointIndex = -1;
    private List<GameObject> activeMobs = new List<GameObject>();
    public bool AllWavesCompleted { get; private set; } = false;
    public bool IsSpawning => isSpawning;

    void Start()
    {
        UpdateWaveCounter();
        StartCoroutine(WaveSpawnLoop());
    }

    private void UpdateWaveCounter()
    {
        if (waveCounterText != null)
        {
            // Используем Mathf.Min чтобы не выходить за пределы количества волн
            int displayWaveIndex = Mathf.Min(currentWaveIndex + 1, waves.Length);
            waveCounterText.text = $"Волна: {displayWaveIndex}/{waves.Length}";
        }
    }

    IEnumerator WaveSpawnLoop()
    {
        while (currentWaveIndex < waves.Length)
        {
            UpdateWaveCounter();
            Wave currentWave = waves[currentWaveIndex];
            yield return StartCoroutine(SpawnWave(currentWave));

            yield return StartCoroutine(WaitForAllMobsDefeated());

            currentWaveIndex++;
            UpdateWaveCounter(); // Обновляем после увеличения индекса

            if (currentWaveIndex < waves.Length)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        AllWavesCompleted = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckLevelCompletion();
        }

        if (loopWaves)
        {
            currentWaveIndex = 0;
            AllWavesCompleted = false;
            UpdateWaveCounter();
            StartCoroutine(WaveSpawnLoop());
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        isSpawning = true;
        activeMobs.Clear();

        for (int i = 0; i < wave.mobCount; i++)
        {
            GameObject mob = SpawnMob(wave.mobPrefab);
            if (mob != null)
            {
                activeMobs.Add(mob);
            }
            yield return new WaitForSeconds(1f / wave.spawnRate);
        }

        isSpawning = false;
    }

    GameObject SpawnMob(GameObject mobPrefab)
    {
        if (spawnPoints.Length == 0 || mobPrefab == null)
            return null;

        lastSpawnPointIndex = (lastSpawnPointIndex + 1) % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[lastSpawnPointIndex];

        return Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    IEnumerator WaitForAllMobsDefeated()
    {
        while (true)
        {
            // Удаляем уничтоженных мобов из списка
            activeMobs.RemoveAll(mob => mob == null || !mob.activeSelf);

            if (activeMobs.Count == 0)
            {
                // Дополнительная проверка для GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CheckLevelCompletion();
                }
                yield break;
            }

            yield return null;
        }
    }
}