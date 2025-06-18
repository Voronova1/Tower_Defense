using System.Linq;
using UnityEngine;

public class MovementMobs : MonoBehaviour
{
    private int currentWayPoint = 0;
    private GameObject[] wayPoints;
    private GameObject[] wayPoints2;
    public float speed = 5f;
    private Vector3 target;
    private float triggerDistance = 0.5f;
    private int rand;
    private GameManager gameManager;
    public int health = 10;
    public int loot;

    public float rotationSpeed = 10f;
    private bool isBeingDestroyed = false;

    // Добавленные поля для избегания столкновений
    public float separationDistance = 1.5f;
    public float separationForce = 2f;
    private Collider[] nearbyColliders = new Collider[10];

    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager не найден!");
            enabled = false;
            return;
        }

        gameManager.AddEnemyOnList(this);

        wayPoints = GameObject.FindGameObjectsWithTag("MovingPoint").OrderBy(go => go.name).ToArray();
        wayPoints2 = GameObject.FindGameObjectsWithTag("MovingPoint2").OrderBy(go => go.name).ToArray();

        if (wayPoints.Length == 0 || wayPoints2.Length == 0)
        {
            Debug.LogError("Нет точек пути!");
            enabled = false;
            return;
        }

        // Улучшенный выбор пути
        rand = Random.value > 0.5f ? 1 : 0;

        // Добавленная вариативность скорости
        speed = Random.Range(speed * 0.8f, speed * 1f);

        UpdateTarget();
    }

    void Update()
    {
        if (isBeingDestroyed) return;

        // Добавленное избегание других мобов
        AvoidOtherMobs();

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (target != transform.position)
        {
            Vector3 direction = (target - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        if (Vector3.Distance(transform.position, target) < triggerDistance)
        {
            SwitchToNextWaypoint();
        }
    }

    // Метод для избегания столкновений
    private void AvoidOtherMobs()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, separationDistance, nearbyColliders);

        Vector3 separationDirection = Vector3.zero;
        int mobsCounted = 0;

        for (int i = 0; i < count; i++)
        {
            if (nearbyColliders[i] != null &&
                nearbyColliders[i].gameObject != gameObject &&
                nearbyColliders[i].CompareTag("Enemy"))
            {
                separationDirection += transform.position - nearbyColliders[i].transform.position;
                mobsCounted++;
            }
        }

        if (mobsCounted > 0)
        {
            separationDirection /= mobsCounted;
            separationDirection = separationDirection.normalized * separationForce;
            transform.position += separationDirection * Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isBeingDestroyed || gameManager == null) return;

        health -= damage;
        if (health <= 0)
        {
            gameManager.ChangeMoney(loot);
            DestroyMob();
        }
    }

    private void DestroyMob()
    {
        if (isBeingDestroyed) return;

        isBeingDestroyed = true;
        if (gameManager != null)
        {
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

    private void SwitchToNextWaypoint()
    {
        if (isBeingDestroyed) return;

        if (currentWayPoint >= (rand == 0 ? wayPoints.Length : wayPoints2.Length) - 1)
        {
            if (gameManager != null)
            {
                gameManager.ChangeHealth(1);
            }
            DestroyMob();
            return;
        }

        currentWayPoint++;
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (rand == 0 && wayPoints.Length > currentWayPoint)
        {
            target = wayPoints[currentWayPoint].transform.position;
        }
        else if (wayPoints2.Length > currentWayPoint)
        {
            target = wayPoints2[currentWayPoint].transform.position;
        }
    }
}