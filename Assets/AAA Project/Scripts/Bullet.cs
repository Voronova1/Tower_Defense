using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public int damage;
    public LayerMask whatIsSolid;

    [HideInInspector] public MovementMobs target;
    [HideInInspector] public Tower tower;

    void Start()
    {

    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 dir = target.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, dir.normalized, out hitInfo, 2f, whatIsSolid))
            {
                if (hitInfo.collider.CompareTag("Enemy"))
                {
                    MovementMobs enemy = hitInfo.collider.GetComponent<MovementMobs>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
