using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Mine : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float travelDuration = 0.35f;
    [SerializeField] private float travelDistance = 3f;
    [SerializeField] private float arcHeight = 0.7f;
    [SerializeField] private float maxLifetime = 25f;

    [Header("Arme y explosion")]
    [SerializeField] private float armTime = 0.6f; // después de caer
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private float knockbackForce = 6f;

    [SerializeField] private Transform visual;

    private Rigidbody2D rb;
    private Collider2D col;

    private float lifeTimer;
    private float travelTimer;
    private float armTimer;

    private bool isMoving;
    private bool isArmed;
    private bool hasLanded;

    private Vector2 moveDir;
    private Vector3 startPos;

    private Vector3 baseVisualLocalPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;

        if (visual == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) visual = sr.transform;
        }

        if (visual != null)
            baseVisualLocalPos = visual.localPosition;
    }

    public void Launch(Vector2 dir)
    {
        Launch(dir, transform.position); 
    }

    public void Launch(Vector2 dir, Vector3 worldStartPos)
    {
        transform.position = worldStartPos;
        startPos = worldStartPos;

        moveDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        lifeTimer = 0f;
        travelTimer = 0f;
        armTimer = 0f;

        isMoving = true;
        hasLanded = false;
        isArmed = false;

        if (visual != null)
            visual.localPosition = baseVisualLocalPos;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (isMoving)
        {
            travelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(travelTimer / travelDuration);

            Vector3 planePos = startPos + (Vector3)(moveDir * (travelDistance * t));
            float h = 4f * arcHeight * t * (1f - t);

            transform.position = planePos;

            if (visual != null)
                visual.localPosition = baseVisualLocalPos + Vector3.up * h;

            if (t >= 1f)
            {
                isMoving = false;
                hasLanded = true;

                if (visual != null)
                    visual.localPosition = baseVisualLocalPos;
            }
        }

        if (hasLanded && !isArmed)
        {
            armTimer += Time.deltaTime;
            if (armTimer >= armTime)
                isArmed = true;
        }

        if (lifeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed) return;

        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
            Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            enemigo_base enemy = hit.GetComponentInParent<enemigo_base>();
            if (enemy == null) continue;

            enemy.RecibirDaño(baseDamage);

            Rigidbody2D er = enemy.GetComponent<Rigidbody2D>();
            if (er != null && er.bodyType == RigidbodyType2D.Dynamic)
            {
                Vector2 dir = (enemy.transform.position - transform.position);
                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
                dir.Normalize();

                er.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
