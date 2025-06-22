using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab; // Префаб моба
    public Transform[] spawnPoints; // Точки спавна
    public float spawnInterval = 3f; // Интервал между спавном
    public int maxMobs = 10;

    private float timer;
    private int currentMobs;
    private int lastSpawnPointIndex = -1;

    void Update()
    {
        if (currentMobs >= maxMobs) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnMob();
            timer = 0f;
        }
    }

    void SpawnMob()
    {
        if (spawnPoints.Length == 0) return;

        // Выбираем следующую точку спавна по порядку
        lastSpawnPointIndex = (lastSpawnPointIndex + 1) % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[lastSpawnPointIndex];

        // Создаём моба
        Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
        currentMobs++;
    }
}