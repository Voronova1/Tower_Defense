using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class Tower : MonoBehaviour
{
    [Header("��������� �����")]
    [SerializeField] private float attackRadius = 15f;
    [SerializeField] private float startTimeBtwAttack = 1f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("������")]
    //[SerializeField] private TowerBow bow; // ����� ��� ���������� ��������� �����

    private GameManager gameManager;
    private MovementMobs targetEnemy;
    private float timeBtwAttack;

    public GameObject nextTower;
    public int cost;

    private void Start()
    {
        // ������������� ������� GameManager
        gameManager = FindObjectOfType<GameManager>();
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

    private void Update()
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
        if(targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) > attackRadius)
        {
            targetEnemy = null;
            //bow.target = null;
        }
        timeBtwAttack -= Time.deltaTime;
        if(timeBtwAttack <= 0 && targetEnemy != null)
        {
            timeBtwAttack = startTimeBtwAttack;
            if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy) // ���� ���� "�����"
            {
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || shootPoint == null) return;

        GameObject newBullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
        Bullet bulletComponent = newBullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.target = targetEnemy;
            bulletComponent.tower = this;
        }
        else
        {
            Debug.LogError("� ������� ������� ����������� ��������� Bullet!");
        }

    }

    // ��� ������������ ������� � ���������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
