using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bullet : MonoBehaviour
{
    public float speed;
    public int damage;
    public LayerMask whatIsSolid;

    [Header("Effects")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    [HideInInspector] public MovementMobs target;
    [HideInInspector] public Tower tower;

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float distanceTraveled;
    private const float maxDistance = 50f; // Макс. дистанция полета

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Сохраняем предыдущую позицию для проверки коллизий
        lastPosition = transform.position;

        // Движение снаряда (оригинальная логика)
        Vector3 dir = (target.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position,
                                               speed * Time.deltaTime * Time.timeScale);

        // Проверка дистанции (чтобы снаряды не летели вечно)
        distanceTraveled += Vector3.Distance(lastPosition, transform.position);
        if (distanceTraveled > maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        // Улучшенная проверка коллизий
        CheckCollision();
    }

    private void CheckCollision()
    {
        // Рассчитываем пройденное расстояние за кадр
        float frameDistance = Vector3.Distance(lastPosition, transform.position);

        // Используем SphereCast для надежного обнаружения коллизий
        if (Physics.SphereCast(lastPosition, 0.3f, (transform.position - lastPosition).normalized,
            out RaycastHit hitInfo, frameDistance * 1.1f, whatIsSolid))
        {
            if (hitInfo.collider.CompareTag("Enemy"))
            {
                MovementMobs enemy = hitInfo.collider.GetComponent<MovementMobs>();
                if (enemy != null)
                {
                    PlayHitEffects(hitInfo.point);
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }

    private void PlayHitEffects(Vector3 hitPosition)
    {
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }
}
