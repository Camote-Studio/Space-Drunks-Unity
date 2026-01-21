using UnityEngine;

public abstract class enemigo_base : MonoBehaviour
{
    [Header("Persecución")]
    public float distanciaParada = 1.2f;

    [Header("Estadísticas")]
    public float vidaMaxima = 100f;
    public float velocidad = 3f;
    public float aceleracion = 8f;

    protected float vidaActual;
    protected Transform objetivo;

    public bool estaMuerto = false;
    public bool estaAtacando = false;

    protected Vector2 velocidadActual;
    protected SpriteRenderer sprite;

    protected virtual void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        vidaActual = vidaMaxima;

        objetivo = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 🔍 DEBUG: detectar jugador al iniciar
        if (objetivo != null)
            Debug.Log($"[ENEMIGO] Jugador detectado: {objetivo.name}");
        else
            Debug.LogWarning("[ENEMIGO] No se encontró GameObject con tag Player");
    }

    protected virtual void Update()
    {
        if (estaMuerto || objetivo == null || estaAtacando)
            return;

        VidaJugador vida = objetivo.GetComponent<VidaJugador>();

        // 🔍 DEBUG: confirmar que el objetivo tiene VidaJugador
        if (vida != null)
        {
            Debug.Log($"[ENEMIGO] Objetivo tiene VidaJugador → {objetivo.name}");
            return;
        }

        MoverHaciaObjetivo();
    }

    protected virtual void MoverHaciaObjetivo()
    {
        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;

        // 🔍 DEBUG: distancia al jugador
        Debug.Log($"[ENEMIGO] Persiguiendo a {objetivo.name} | Distancia: {dist:F2}");

        if (dist <= distanciaParada)
        {
            velocidadActual = Vector2.Lerp(
                velocidadActual,
                Vector2.zero,
                aceleracion * Time.deltaTime
            );

            AplicarMovimiento();
            return;
        }

        Vector2 dir = toTarget.normalized;
        Vector2 targetVel = dir * velocidad;

        velocidadActual = Vector2.Lerp(
            velocidadActual,
            targetVel,
            aceleracion * Time.deltaTime
        );

        AplicarMovimiento();

        if (sprite != null && Mathf.Abs(dir.x) > 0.01f)
            sprite.flipX = dir.x > 0;
    }

    protected void AplicarMovimiento()
    {
        transform.position += (Vector3)(velocidadActual * Time.deltaTime);
    }

    public virtual void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
            Morir();
    }

    protected virtual void Morir()
    {
        Debug.Log("[ENEMIGO] Enemigo muerto");

        estaMuerto = true;
        Destroy(gameObject);
    }
}
