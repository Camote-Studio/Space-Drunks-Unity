using UnityEngine;

public class XPOrb : MonoBehaviour
{
    public float xpValue = 5f;
    public float attractDistance = 3f;
    public float attractSpeed = 6f;
    public float lifetime = 10f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Destroy(gameObject, lifetime);

        Vector2 randomForce = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
        rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attractDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * attractSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            VidaJugador jugador = other.GetComponent<VidaJugador>();
            if (jugador != null)
            {
                jugador.AgregarExperiencia((int)xpValue);
            }

            Destroy(gameObject);
        }
    }
}
