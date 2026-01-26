using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("NormalBullet")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float normalDamage = 6f;
    [SerializeField] private float lifeTimeNormal = 0.35f;

    [Header("BigBullet")]
    [SerializeField] private float speedBig = 20f;
    [SerializeField] private float bigScaleMultiplier = 2f;
    [SerializeField] private float growSpeed = 5f;
    [SerializeField] private float bigDamage = 25f;
    [SerializeField] private float lifeTimeBig = 3f;

    private Vector2 direction = Vector2.right;
    private float timer;
    private float currentSpeed;
    private float currentLifeTime;
    private bool isBig;
    private Vector3 targetScale;
    private Vector3 baseScale;

    public void Initialize(Vector2 dir, bool big = false)
    {
        direction = dir.normalized;
        isBig = big;
        timer = 0f;

        currentSpeed = isBig ? speedBig : speed;
        currentLifeTime = isBig ? lifeTimeBig : lifeTimeNormal;

        baseScale = transform.localScale;
        targetScale = isBig ? baseScale * bigScaleMultiplier : baseScale;
        if (!isBig)
            transform.localScale = baseScale;
    }

    private void Awake()
    {
        baseScale = transform.localScale;
        currentSpeed = speed;
        currentLifeTime = lifeTimeNormal;
        targetScale = baseScale;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * currentSpeed * Time.deltaTime);

        if (isBig)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                targetScale,
                growSpeed * Time.deltaTime
            );
        }

        timer += Time.deltaTime;
        if (timer >= currentLifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
        {
            float damage = isBig ? bigDamage : normalDamage;
            enemy.RecibirDaño(damage);
            Destroy(gameObject);
        }
    }
}
