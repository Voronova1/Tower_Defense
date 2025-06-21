using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

// Добавляем enum для типов башен (перед классом Tower)
public enum TowerType { SingleTarget, MultiTarget }

public class Tower : MonoBehaviour
{
    [Header("Тип башни")]
    [SerializeField] private TowerType towerType = TowerType.SingleTarget;
    [SerializeField] private int maxTargets = 3; // Макс. кол-во целей для MultiTarget

    [Header("Настройки атаки")]
    [SerializeField] private float attackRadius = 15f;
    [SerializeField] private float startTimeBtwAttack = 1f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Ссылки")]
    //[SerializeField] private TowerBow bow; // Класс для управления поворотом башни

    private GameManager gameManager;
    private MovementMobs targetEnemy;
    private float timeBtwAttack;

    public GameObject nextTower;
    public int cost;

    [Header("Desert Effects")]
    [SerializeField] private float desertSpeedMultiplier = 0.25f;
    [SerializeField] private float desertFireRateMultiplier = 1.35f;
    [SerializeField] private LayerMask oasisLayer;
    [SerializeField] private float oasisCheckRadius = 3f;

    private float originalFireRate;
    private float originalBulletSpeed;
    private bool isInDesert = false;
    private bool hasOasisNearby = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip shootSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        gameManager = FindObjectOfType<GameManager>();
        originalFireRate = startTimeBtwAttack;

        // Запоминаем оригинальную скорость снаряда из префаба
        if (bulletPrefab != null && bulletPrefab.TryGetComponent<Bullet>(out var bullet))
        {
            originalBulletSpeed = bullet.speed;
        }

        // Проверяем тип уровня (пустыня или нет)
        isInDesert = gameManager != null && gameManager.currentLevelType == GameManager.LevelType.Desert;
    }

    void Update()
    {
        if (targetEnemy == null)
        {
            MovementMobs nearest = GetNearestEnemy();
            if (nearest != null)
            {
                targetEnemy = nearest;
                //bow.target = targetEnemy;
            }
        }
        if (targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) > attackRadius)
        {
            targetEnemy = null;
            //bow.target = null;
        }
        timeBtwAttack -= Time.deltaTime;
        if (timeBtwAttack <= 0 && targetEnemy != null)
        {
            timeBtwAttack = startTimeBtwAttack;
            if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy) // Если враг "живой"
            {
                Shoot();
            }
        }

        if (!isInDesert) return;

        // Проверяем наличие оазиса каждый кадр (можно оптимизировать, проверя реже)
        CheckForOasis();
        ApplyCurrentEffects();
    }

    private void CheckForOasis()
    {
        bool newOasisState = Physics.CheckSphere(transform.position, oasisCheckRadius, oasisLayer);

        if (newOasisState != hasOasisNearby)
        {
            hasOasisNearby = newOasisState;
            ApplyCurrentEffects();
        }
    }

    private void ApplyCurrentEffects()
    {
        if (!isInDesert)
        {
            startTimeBtwAttack = originalFireRate; // Нормальная скорость атаки
            return;
        }

        if (hasOasisNearby)
        {
            startTimeBtwAttack = originalFireRate; // Оазис рядом — нормальная скорость
        }
        else
        {
            startTimeBtwAttack = originalFireRate * desertFireRateMultiplier; // В пустыне — медленнее стрельба
        }
    }



    private List<MovementMobs> GetEnemiesInRange()
    {
        List<MovementMobs> enemiesInRange = new List<MovementMobs>();
        foreach (MovementMobs enemy in gameManager.EnemyList)
        {
            if(enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= attackRadius)
            {
                enemiesInRange.Add(enemy);
            }
        }
        return enemiesInRange;
    }

    private MovementMobs GetNearestEnemy()
    {
        MovementMobs nearestEnemy = null;
        float smallesDistance = float.PositiveInfinity;
        foreach(MovementMobs enemy in GetEnemiesInRange())
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < smallesDistance)
            {
                smallesDistance = Vector3.Distance(transform.position, enemy.transform.position);
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }

   

    private void Shoot()
    {
        if (bulletPrefab == null || shootPoint == null) return;

        if (towerType == TowerType.SingleTarget)
        {
            // Стандартная стрельба (1 снаряд)
            FireBullet(targetEnemy);
        }
        else if (towerType == TowerType.MultiTarget)
        {
            // Стрельба по нескольким целям
            List<MovementMobs> enemiesInRange = GetEnemiesInRange();
            int targetsToShoot = Mathf.Min(maxTargets, enemiesInRange.Count);

            for (int i = 0; i < targetsToShoot; i++)
            {
                if (i < enemiesInRange.Count)
                {
                    FireBullet(enemiesInRange[i]);
                }
            }
        }
    }

    // Создание снаряда в отдельный метод для избежания дублирования кода
    private void FireBullet(MovementMobs enemy)
    {
        GameObject newBullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Bullet bulletComponent = newBullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.target = enemy;
            bulletComponent.tower = this;

            // Устанавливаем скорость снаряда:
            if (isInDesert && !hasOasisNearby)
            {
                bulletComponent.speed = originalBulletSpeed * desertSpeedMultiplier; // Медленный снаряд в пустыне
            }
            else
            {
                bulletComponent.speed = originalBulletSpeed; // Обычная скорость
            }
        }
        else
        {
            Debug.LogError("У префаба снаряда нет компонента Bullet!");
        }

        if (shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    // Для визуализации радиуса в редакторе

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса атаки (синий)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Визуализация радиуса оазиса (зеленый)
        if (isInDesert)
        {
            // Радиус проверки оазиса
            Gizmos.color = hasOasisNearby ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, oasisCheckRadius);
        }
    }
}
