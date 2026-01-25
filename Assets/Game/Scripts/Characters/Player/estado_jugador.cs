using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(VidaJugador))]
public class estado_jugador : MonoBehaviour
{
    [Header("Flotación")]
    [SerializeField] private float duracionFlotacion = 4.3f;
    [SerializeField] private float alturaFlotacion = 5f;
    [SerializeField] private float velocidadSubida = 2.5f;
    [SerializeField] private float velocidadDescenso = 1.5f;
    [SerializeField] private float velocidadRotacion = 180f;

    private bool flotando;
    private bool bajando;
    private bool invulnerable;

    private float timerFlotacion;
    private float yInicial;
    private Quaternion rotInicial;
    private string tagOriginal;

    private Rigidbody2D rb;
    private Collider2D col;
    private VidaJugador vida;

    private bool rbSimulatedOriginal;
    private bool colEnabledOriginal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        vida = GetComponent<VidaJugador>();

        tagOriginal = gameObject.tag;

        vida.OnDamaged += OnJugadorDaniado;
    }

    private void Update()
    {
        if (flotando) ManejarFlotacion();
        else if (bajando) ManejarDescenso();
    }

    private void OnDestroy()
    {
        vida.OnDamaged -= OnJugadorDaniado;
    }

    // 📩 EVENTO DE VIDA
    private void OnJugadorDaniado(string fuente)
    {
        if (invulnerable) return;

        if (fuente == "bala_gravedad")
        {
            ActivarFlotacion();
        }
    }

    private void ActivarFlotacion()
    {
        yInicial = transform.position.y;
        rotInicial = transform.rotation;

        rbSimulatedOriginal = rb.simulated;
        colEnabledOriginal = col.enabled;

        flotando = true;
        bajando = false;
        invulnerable = true;
        timerFlotacion = duracionFlotacion;

        gameObject.tag = "Untagged";

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
        col.enabled = false;
    }

    private void ManejarFlotacion()
    {
        timerFlotacion -= Time.deltaTime;

        float yObjetivo = yInicial + alturaFlotacion;

        float nuevaY = Mathf.MoveTowards(
            transform.position.y,
            yObjetivo,
            velocidadSubida * Time.deltaTime
        );

        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
        transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);

        if (timerFlotacion <= 0f)
            FinalizarFlotacion();
    }

    private void FinalizarFlotacion()
    {
        flotando = false;
        bajando = true;
        transform.rotation = rotInicial;
    }

    private void ManejarDescenso()
    {
        float nuevaY = Mathf.MoveTowards(
            transform.position.y,
            yInicial,
            velocidadDescenso * Time.deltaTime
        );

        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);

        if (Mathf.Abs(transform.position.y - yInicial) <= 0.01f)
        {
            transform.position = new Vector3(transform.position.x, yInicial, transform.position.z);

            bajando = false;
            invulnerable = false;

            gameObject.tag = tagOriginal;
            rb.simulated = rbSimulatedOriginal;
            col.enabled = colEnabledOriginal;
        }
    }

    public bool EsIntocable()
    {
        return flotando || invulnerable;
    }
}
