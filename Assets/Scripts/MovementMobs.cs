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

    private MobSpawner spawner;
    public int health = 10;

    public int loot; 
    void Start()
    {
        // Автоматически находим GameManager
        gameManager = FindObjectOfType<GameManager>();

        gameManager.AddEnemyOnList(this);

        wayPoints = GameObject.FindGameObjectsWithTag("MovingPoint").OrderBy(go => go.name).ToArray();
        wayPoints2 = GameObject.FindGameObjectsWithTag("MovingPoint2").OrderBy(go => go.name).ToArray();

        if (wayPoints.Length == 0)
        {
            Debug.LogError("Нет точек пути!");
            enabled = false;
            return;
        }
        if (wayPoints2.Length == 0)
        {
            Debug.LogError("Нет точек пути!");
            enabled = false;
            return;
        }

        rand = Random.Range(0, 2);
        if (rand == 0)
        {
            target = wayPoints[currentWayPoint].transform.position;
        }
        else
        {
            target = wayPoints2[currentWayPoint].transform.position;
        }

    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            gameManager.ChangeMoney(loot);
            Destroy(gameObject);
            
        }
    }

    private void OnDestroy()
    {   
        gameManager.RemoveEnemyFromList(this);
        int enemiesLeft = GameObject.FindGameObjectsWithTag("Enemy").Length;
        //if (enemiesLeft == 0)
        //{
        //    spawner.SpawnMob();
        //}
    }


    void Update()
    {
        // Движение к цели
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Дополнительная проверка расстояния
        if (Vector3.Distance(transform.position, target) < triggerDistance)
        {
            SwitchToNextWaypoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это именно текущая целевая точка
        if (other.CompareTag("MovingPoint") || other.CompareTag("MovingPoint")  && other.gameObject == wayPoints[currentWayPoint])
        {
            SwitchToNextWaypoint();
        }
    }

    private void SwitchToNextWaypoint()
    {
        // Сначала проверяем, не последняя ли это точка
        if (currentWayPoint >= wayPoints.Length - 1)
        {
            
            if (gameManager != null)
            {
                gameManager.ChangeHealth(1); // Наносим урон игроку
            }
            else
            {
                Debug.LogWarning("GameManager не назначен!");
            }
            
            Destroy(gameObject);
            return;
        }

        // Переключаемся на следующую точку
        currentWayPoint++;
        if (rand == 0)
        {
            target = wayPoints[currentWayPoint].transform.position;
        }
        else
        {
            target = wayPoints2[currentWayPoint].transform.position;
        }
        
    }

}