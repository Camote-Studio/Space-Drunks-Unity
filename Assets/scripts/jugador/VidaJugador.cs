using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float vidaActual;

    public float VidaMaxima => vidaMaxima;
    public float VidaActual => vidaActual;

    public event Action OnDamaged;

    [Header("Flotación")]
    [SerializeField] private bool flotando = false;
    [SerializeField] private bool invulnerable = false;

    [SerializeField] private float duracionFlotacion = 4.3f;
    [SerializeField] private float alturaFlotacion = 5f;
    [SerializeField] private float velocidadSubida = 2.5f;     // unidades/seg
    [SerializeField] private float velocidadDescenso = 1.5f;   // unidades/seg
    [SerializeField] private float velocidadRotacion = 180f;   // grados/seg

    private float timerFlotacion;

    private Rigidbody2D rb;
    private Collider2D col;

    private float yInicial;
    private Quaternion rotInicial;
    private string tagOriginal;

    private bool bajando = false;

    // Para restaurar estados originales (por si ya venían desactivados)
    private bool rbSimulatedOriginal;
    private bool colEnabledOriginal;

    private void Awake()
    {
        vidaActual = vidaMaxima;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        tagOriginal = gameObject.tag;

        rbSimulatedOriginal = rb.simulated;
        colEnabledOriginal = col.enabled;

        rotInicial = transform.rotation;
        yInicial = transform.position.y;
    }

    private void Update()
    {
        if (flotando) ManejarFlotacion();
        else if (bajando) ManejarDescenso();
    }

    // DAÑO
    public void RecibirDanio(float cantidad, string fuente = "")
    {
        if (invulnerable) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        OnDamaged?.Invoke();

        if (vidaActual <= 0f)
        {
            Morir();
            return;
        }

        if (fuente == "bala_gravedad")
        {
            ActivarFlotacion();
        }
    }

    // ACTIVAR FLOTACIÓN
    private void ActivarFlotacion()
    {
        // Captura la altura/rotación del momento del impacto (no desde Awake)
        yInicial = transform.position.y;
        rotInicial = transform.rotation;

        // Guarda estados actuales por si cambian durante gameplay
        rbSimulatedOriginal = rb.simulated;
        colEnabledOriginal = col.enabled;

        flotando = true;
        bajando = false;
        invulnerable = true;
        timerFlotacion = duracionFlotacion;

        // Evita que otros sistemas lo “detecten”
        gameObject.tag = "Untagged";

        // Congelar física y colisiones
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;

        col.enabled = false;
    }

    // SUBIDA
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

    // FIN FLOTACIÓN
    private void FinalizarFlotacion()
    {
        flotando = false;
        bajando = true;

        // Vuelve a la rotación que tenía al iniciar la flotación
        transform.rotation = rotInicial;
    }

    // BAJADA
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

    // MUERTE
    private void Morir()
    {
        Debug.Log("Jugador muerto");
        Destroy(gameObject);
    }

    public bool EsIntocable()
    {
        return flotando || invulnerable;
    }
}
