using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Mine : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float travelTime = 0.4f;  
    [SerializeField] private float maxLifetime = 25f;

    [Header("Arme y explosión")]
    [SerializeField] private float armTime = 0.6f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private float knockbackForce = 6f;

    private Rigidbody2D rb;
    private float timer;        
    private float travelTimer;    
    private float armTimer;      

    private bool hasLanded;
    private bool isArmed;

    private Vector2 moveDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Launch(Vector2 dir)
    {
        moveDir = dir.normalized;

        timer = 0f;
        travelTimer = 0f;
        armTimer = 0f;

        hasLanded = false;
        isArmed = false;

        rb.bodyType = RigidbodyType2D.Kinematic; // nos movemos a mano, no con fuerzas

        Debug.Log("Mine: lanzada en dirección " + moveDir);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 1) Vuelo recto
        if (!hasLanded)
        {
            travelTimer += Time.deltaTime;
            transform.position += (Vector3)(moveDir * launchSpeed * Time.deltaTime);

            if (travelTimer >= travelTime)
            {
                hasLanded = true;   // se planta
                Debug.Log("Mine: plantada");
            }
        }
        // 2) Plantada → se arma después de armTime
        else if (!isArmed)
        {
            armTimer += Time.deltaTime;
            if (armTimer >= armTime)
            {
                isArmed = true;
                Debug.Log("Mine: armada (lista para explotar)");
            }
        }

        // 3) Vida máxima
        if (timer >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo explota si ya está armada
        if (!isArmed) return;

        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("Mine: ¡BOOM! Explosión");

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
