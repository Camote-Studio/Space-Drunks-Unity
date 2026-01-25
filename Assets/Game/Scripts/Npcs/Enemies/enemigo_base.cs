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

        ActualizarObjetivoMasCercano();
    }

    protected virtual void Update()
    {
        if (estaMuerto || estaAtacando)
            return;

        if (objetivo == null)
            ActualizarObjetivoMasCercano();

        if (objetivo == null)
            return;

        MoverHaciaObjetivo();
    }

    // =====================================================
    // 🎯 BUSCAR PLAYER MÁS CERCANO (SEGURO)
    // =====================================================
    protected void ActualizarObjetivoMasCercano()
    {
        Transform nearest = null;
        float nearestDist = Mathf.Infinity;
        Vector2 myPos = transform.position;

        BuscarConTag("Player", ref nearest, ref nearestDist, myPos);
        BuscarConTag("Player_2", ref nearest, ref nearestDist, myPos);

        objetivo = nearest;
    }

    void BuscarConTag(string tag, ref Transform nearest, ref float nearestDist, Vector2 myPos)
    {
        GameObject[] objs;

        try
        {
            objs = GameObject.FindGameObjectsWithTag(tag);
        }
        catch
        {
            return; // el tag no existe → no rompe nada
        }

        foreach (GameObject o in objs)
        {
            if (o == null) continue;

            float dist = Vector2.Distance(myPos, o.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = o.transform;
            }
        }
    }

    // =====================================================
    // 🏃 MOVIMIENTO
    // =====================================================
    protected virtual void MoverHaciaObjetivo()
    {
        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;

        if (dist <= distanciaParada)
        {
            velocidadActual = Vector2.Lerp(
                velocidadActual,
                Vector2.zero,
                aceleracion * Time.deltaTime
            );
        }
        else
        {
            Vector2 dir = toTarget.normalized;
            Vector2 targetVel = dir * velocidad;

            velocidadActual = Vector2.Lerp(
                velocidadActual,
                targetVel,
                aceleracion * Time.deltaTime
            );

            if (sprite && Mathf.Abs(dir.x) > 0.01f)
                sprite.flipX = dir.x > 0;
        }

        AplicarMovimiento();
    }

    protected void AplicarMovimiento()
    {
        transform.position += (Vector3)(velocidadActual * Time.deltaTime);
    }

    // =====================================================
    // ❤️ VIDA
    // =====================================================
    public virtual void RecibirDaño(float cantidad)
    {
        if (estaMuerto || !PuedeRecibirDaño()) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
            Morir();
    }
    protected virtual bool PuedeRecibirDaño() => true;

    protected virtual void Morir()
    {
        estaMuerto = true;
        Destroy(gameObject);
    }

    protected virtual bool PuedeMorir()
    {
        return true;
    }


}
