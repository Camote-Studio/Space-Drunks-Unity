using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("XP")]
    public float xpValue = 5f;

    [Header("Ultimate Charge")]
    public float ultiChargeValue = 5f;

    [Header("Attraction")]
    public float attractDistance = 3f;
    public float attractSpeed = 6f;

    [Header("Lifetime")]
    public float lifetime = 10f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Destroy(gameObject, lifetime);

        Vector2 randomForce = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 3f));
        if (rb != null) rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        if (player == null || rb == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < attractDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * attractSpeed;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Agarra el player desde el padre aunque el collider sea de un hijo
        VidaJugador jugador = other.GetComponentInParent<VidaJugador>();
        if (jugador == null) return;

        jugador.AgregarExperiencia((int)xpValue);

        UltimateWeapon ult = jugador.GetComponentInChildren<UltimateWeapon>(true);
        if (ult != null)
        {
            ult.AddCharge(ultiChargeValue);
        }

        Destroy(gameObject);
    }
}