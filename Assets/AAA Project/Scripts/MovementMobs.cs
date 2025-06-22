using UnityEngine;
using System.Linq;

public class MovementMobs : MonoBehaviour
{
    private Transform[][] waypointGroups; // Группы точек пути
    private Transform currentWaypoint;
    private int currentPointIndex = 0;
    public float baseSpeed = 3f;
    private float actualSpeed;

    [Header("Settings")]
    public float rotationSpeed = 10f;
    public float arrivalDistance = 0.1f;

    [Header("Combat")]
    public int health = 10;
    public int loot = 1;

    private GameManager gameManager;
    private bool isBeingDestroyed = false;
    private bool killedByPlayer = false; // Новый флаг для отслеживания смерти от игрока

    public void Initialize(params Transform[][] groups)
    {
        // Фильтруем пустые группы
        waypointGroups = groups?
            .Where(g => g != null && g.Length > 0)
            .ToArray();

        if (waypointGroups == null || waypointGroups.Length == 0)
        {
            Debug.LogError("No valid waypoint groups provided!");
            SafeDestroy();
            return;
        }

        currentPointIndex = 0;
        actualSpeed = Random.Range(baseSpeed * 1f, baseSpeed * 1f);

        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager instance not found");
            SafeDestroy();
            return;
        }

        gameManager.AddEnemyOnList(this);
        SelectNextWaypoint();
    }

    private void SafeDestroy()
    {
        if (!isBeingDestroyed)
        {
            isBeingDestroyed = true;
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (isBeingDestroyed || currentWaypoint == null)
            return;

        MoveToWaypoint();
        CheckWaypointReached();
    }

    private void MoveToWaypoint()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentWaypoint.position,
            actualSpeed * Time.deltaTime
        );

        RotateTowards(currentWaypoint.position);
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void CheckWaypointReached()
    {
        if (Vector3.Distance(transform.position, currentWaypoint.position) <= arrivalDistance)
        {
            currentPointIndex++;
            SelectNextWaypoint();
        }
    }

    private void SelectNextWaypoint()
    {
        // Находим все группы, у которых есть точка с текущим индексом
        var validGroups = waypointGroups
            .Where(g => currentPointIndex < g.Length && g[currentPointIndex] != null)
            .ToArray();

        if (validGroups.Length == 0)
        {
            ReachedEndOfPath();
            return;
        }

        // Выбираем случайную группу из подходящих
        int randomGroupIndex = Random.Range(0, validGroups.Length);
        currentWaypoint = validGroups[randomGroupIndex][currentPointIndex];
    }

    private void ReachedEndOfPath()
    {
        if (isBeingDestroyed) return;

        if (gameManager != null)
        {
            gameManager.ChangeHealth(1);
        }
        Die();
    }

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed) return;

        health -= damage;
        if (health <= 0)
        {
            killedByPlayer = true; // Устанавливаем флаг, что моб убит игроком
            Die();
        }
    }

    private void Die()
    {
        if (isBeingDestroyed) return;

        isBeingDestroyed = true;

        if (gameManager != null)
        {
            // Начисляем награду только если моб был убит игроком
            if (killedByPlayer)
            {
                gameManager.ChangeMoney(loot);
            }
            gameManager.SafeRemoveEnemyFromList(this);
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (!isBeingDestroyed && gameManager != null)
        {
            gameManager.SafeRemoveEnemyFromList(this);
        }
    }
}