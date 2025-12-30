using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Mine : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float maxLifetime = 25f;

    [Header("Arme y explosi�n")]
    [SerializeField] private float armTime = 0.6f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float baseDamage = 12f;
    [SerializeField] private float knockbackForce = 6f;

    private Rigidbody2D rb;
    private float timer;
    private bool isArmed;
    private bool hasLanded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configura el rigidbody para que caiga pero no se gire loco
        rb.gravityScale = 1f;
        rb.freezeRotation = true;

        // Asegura que el collider es trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    public void Launch(Vector2 dir)
    {
        dir = dir.normalized;
        timer = 0f;
        isArmed = false;
        hasLanded = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = dir * launchSpeed;

        Debug.Log("Mine: lanzada en direcci�n " + dir);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Detectamos cuando ya �aterriz�
        if (!hasLanded && rb.linearVelocity.magnitude < 0.1f)
        {
            hasLanded = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Se arma despu�s de cierto tiempo en el suelo
        if (hasLanded && !isArmed && timer >= armTime)
        {
            isArmed = true;
            Debug.Log("Mine: armada (lista para explotar)");
        }

        // Caduca si pasa demasiado tiempo
        if (timer >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si a�n no est� armada, ignoramos
        if (!isArmed) return;

        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log("Mine: �BOOM! Explosi�n");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            enemigo_base enemy = hit.GetComponentInParent<enemigo_base>();
            if (enemy != null)
            {
                // da�o
                enemy.RecibirDa�o(baseDamage);

                // empuje
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

    // Solo para ver en el editor el radio de explosi�n
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
