using UnityEngine;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject mobPrefab; // ������ ����
        public int mobCount;         // ���������� ����� � �����
        public float spawnRate;      // �������� ����� ������� (1/���)
    }

    public Wave[] waves;            // ������ ����
    public Transform[] spawnPoints; // ����� ������
    public float timeBetweenWaves = 5f; // ����� ����� �������
    public bool loopWaves = false;   // ����������� �����

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
            // ���� ��� ����� �������� � �� ��������� - �������
            if (currentWaveIndex >= waves.Length && !loopWaves) yield break;

            // ��������� �����
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex % waves.Length]));

            // ����� ����� �������
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

    // ���������� ��� ������ ���� (���� ����� �����������)
    public void OnMobDeath()
    {
        // ������ ��� ������ ����
    }
}