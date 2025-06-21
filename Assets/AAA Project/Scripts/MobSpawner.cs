using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject mobPrefab; // ������ ����
    public Transform[] spawnPoints; // ����� ������
    public float spawnInterval = 3f; // �������� ����� �������
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

        // �������� ��������� ����� ������
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // ������ ����
        Instantiate(mobPrefab, spawnPoint.position, spawnPoint.rotation);
        currentMobs++;
    }
}
