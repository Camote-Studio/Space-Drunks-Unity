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
    [SerializeField] private float armTime = 0.6f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private float knockbackForce = 6f;

    [SerializeField] private Transform visual;

    //VARIABLES

    private Rigidbody2D rb;
    private float lifeTimer;
    private float travelTimer;
    private bool isMoving;
    private bool isArmed;
    private bool hasLanded;

    private Vector2 moveDir;
    private Vector3 startPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (visual == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                visual = sr.transform;
        }
    }

    public void Launch(Vector2 dir)
    {
        //Si la entrada de dirección (dir) es mayor que cero, usa esa dirección normalizada. Si la entrada es cero (el jugador no toca nada), 
        // asume que la dirección es hacia la derecha por defecto.
        moveDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right; 
        startPos = transform.position;

        lifeTimer = 0f;
        travelTimer = 0f;
        isMoving = true;
        hasLanded = false;
        isArmed = false;

        if (visual != null)
        {
            var lp = visual.localPosition;
            lp.y = 0f;
            visual.localPosition = lp;
        }
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (isMoving)
        {
            travelTimer += Time.deltaTime;
            float t = Mathf.Clamp01(travelTimer / travelDuration);

            float dist = travelDistance * t;
            Vector3 planePos = startPos + (Vector3)(moveDir * dist);

            float h = 4f * arcHeight * t * (1f - t);

            transform.position = planePos;

            if (visual != null)
            {
                var lp = visual.localPosition;
                lp.y = h;
                visual.localPosition = lp;
            }

            if (t >= 1f)
            {
                isMoving = false;
                hasLanded = true;

                if (visual != null)
                {
                    var lp = visual.localPosition;
                    lp.y = 0f;
                    visual.localPosition = lp;
                }
            }
        }

        if (hasLanded && !isArmed && lifeTimer >= armTime)
        {
            isArmed = true;
        }

        if (lifeTimer >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isArmed) return;

        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            enemigo_base enemy = hit.GetComponentInParent<enemigo_base>();
            if (enemy != null)
            {
                enemy.RecibirDaño(baseDamage);

                Rigidbody2D er = enemy.GetComponent<Rigidbody2D>();
                if (er != null)
                {
                    Vector2 dir = (enemy.transform.position - transform.position).normalized;
                    er.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
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
