using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject mobPrefab; // Префаб моба
        public int mobCount;         // Количество мобов в волне
        public float spawnRate;      // Интервал между спавном (1/сек)
    }

    public Wave[] waves;            // Массив волн
    public Transform[] spawnPoints; // Точки спавна
    public float timeBetweenWaves = 5f; // Пауза между волнами
    public bool loopWaves = false;   // Зацикливать волны

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(WaveSpawnLoop());
    }

    IEnumerator WaveSpawnLoop()
    {
        while (true)
        {
            // Если все волны пройдены и не зациклены - выходим
            if (currentWaveIndex >= waves.Length && !loopWaves) yield break;

            // Запускаем волну
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex % waves.Length]));

            // Пауза между волнами
            yield return new WaitForSeconds(timeBetweenWaves);

            currentWaveIndex++;
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

    // Вызывается при смерти моба (если нужно отслеживать)
    public void OnMobDeath()
    {
        // Логика при смерти моба
    }
}