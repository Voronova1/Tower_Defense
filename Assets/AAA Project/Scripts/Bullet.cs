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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

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
                PlayHitEffects(hitInfo.point);
                enemy.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    private void PlayHitEffects(Vector3 hitPosition)
    {
        // Воспроизведение звука
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // Создание эффекта попадания
        if (hitEffect != null)
        {
            ParticleSystem effect = Instantiate(hitEffect, hitPosition, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }
}