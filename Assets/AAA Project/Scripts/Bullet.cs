using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public int damage;
    public LayerMask whatIsSolid;

    [HideInInspector] public MovementMobs target;
    [HideInInspector] public Tower tower;

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

        if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hitInfo, 2f, whatIsSolid))
        {
            if (hitInfo.collider.CompareTag("Enemy") && hitInfo.collider.TryGetComponent<MovementMobs>(out var enemy))
            {
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}