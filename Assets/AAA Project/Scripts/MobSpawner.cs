using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab; // Префаб моба
    public Transform[] spawnPoints; // Точки спавна
    public float spawnInterval = 3f; // Интервал между спавном
    public int maxMobs = 10; 

    private float timer;
    private int currentMobs;

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

        // Выбираем случайную точку спавна
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Создаём моба
        Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
        currentMobs++;
    }
}
