using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    public event Action OnDamaged;

    [Header("Flotación")]
    public bool flotando = false;
    public bool invulnerable = false;

    public float duracionFlotacion = 4.3f;
    private float timerFlotacion;

    public float alturaFlotacion = 5f;
    public float velocidadSubida = 2.5f;
    public float velocidadDescenso = 1.5f;
    public float velocidadRotacion = 180f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float yInicial;
    private string tagOriginal;

    private bool bajando = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        yInicial = transform.position.y;
        tagOriginal = gameObject.tag;
    }

    private void Update()
    {
        if (flotando)
            ManejarFlotacion();
        else if (bajando)
            ManejarDescenso();
    }

    // 🔴 DAÑO
    public void RecibirDaño(float cantidad, string fuente = "")
    {
        if (invulnerable) return;

        vidaActual -= cantidad;
        OnDamaged?.Invoke();

        if (vidaActual <= 0f)
        {
            vidaActual = 0f;
            Morir();
            return;
        }

        if (fuente == "bala_gravedad")
        {
            ActivarFlotacion();
        }
    }

    // 🟣 ACTIVAR FLOTACIÓN
    private void ActivarFlotacion()
    {
        flotando = true;
        bajando = false;
        invulnerable = true;
        timerFlotacion = duracionFlotacion;

        gameObject.tag = "Untagged";

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (col != null)
            col.enabled = false;
    }

    // ⬆ SUBIDA
    private void ManejarFlotacion()
    {
        timerFlotacion -= Time.deltaTime;

        float yObjetivo = yInicial + alturaFlotacion;

        float nuevaY = Mathf.Lerp(
            transform.position.y,
            yObjetivo,
            velocidadSubida * Time.deltaTime
        );

        transform.position = new Vector3(
            transform.position.x,
            nuevaY,
            transform.position.z
        );

        transform.Rotate(0, 0, velocidadRotacion * Time.deltaTime);

        if (timerFlotacion <= 0f)
            FinalizarFlotacion();
    }

    // 🔚 FIN FLOTACIÓN
    private void FinalizarFlotacion()
    {
        flotando = false;
        bajando = true;
        transform.rotation = Quaternion.identity;
    }

    // ⬇ BAJADA
    private void ManejarDescenso()
    {
        float nuevaY = Mathf.Lerp(
            transform.position.y,
            yInicial,
            velocidadDescenso * Time.deltaTime
        );

        transform.position = new Vector3(
            transform.position.x,
            nuevaY,
            transform.position.z
        );

        if (Mathf.Abs(transform.position.y - yInicial) < 0.05f)
        {
            transform.position = new Vector3(
                transform.position.x,
                yInicial,
                transform.position.z
            );

            bajando = false;
            invulnerable = false;
            gameObject.tag = tagOriginal;

            if (rb != null)
                rb.simulated = true;

            if (col != null)
                col.enabled = true;
        }
    }

    // ☠ MUERTE
    private void Morir()
    {
        Debug.Log("Jugador muerto");
        Destroy(gameObject);
    }
}
