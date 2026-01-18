using UnityEngine;
using System.Collections;

public class enemigoverde : enemigo_base
{
    [Header("Referencias")]
    private enemigo_animacion anim;
    private Collider2D col;

    [Header("Ataque a Distancia")]
    public GameObject balaPrefab;
    public float velocidadBala = 8f;
    public float rangoDisparo = 6f;
    public float enfriamientoDisparo = 2f;

    [Header("Movimiento en Combate")]
    public float distanciaIdeal = 4.5f;
    public float velocidadStrafe = 2f;
    public float cambioDireccionTiempo = 1.5f;

    [Header("Fatality")]
    public float duracionFatalityAnim = 1.4f;

    [Header("Retroceso")]
    public float fuerzaRetroceso = 4f;
    public float tiempoRetroceso = 0.08f;

    [Header("Stun")]
    public float tiempoStun = 0.25f;

    // Estados
    bool enRetroceso, enStun, fatalityEjecutada;

    Vector2 velocidadRetroceso;

    float timerDisparo;
    float timerStrafe;
    int dirStrafe = 1;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<enemigo_animacion>();
        col = GetComponent<Collider2D>();
        timerStrafe = cambioDireccionTiempo;
    }

    protected override void Update()
    {
        if (objetivo == null || Bloqueado())
            return;

        if (JugadorIntocable())
        {
            Detener();
            return;
        }

        HandleTimers();
        HandleMovement();
        HandleShooting();
    }

    bool Bloqueado() => estaMuerto || fatalityEjecutada || enRetroceso || enStun;

    bool JugadorIntocable()
    {
        VidaJugador v = objetivo.GetComponent<VidaJugador>();
        return v != null && v.EsIntocable();
    }

    void Detener()
    {
        velocidadActual = Vector2.zero;
        AplicarMovimiento();
    }

    void HandleTimers()
    {
        timerDisparo -= Time.deltaTime;
        timerStrafe -= Time.deltaTime;

        if (timerStrafe <= 0f)
        {
            dirStrafe = Random.value > 0.5f ? 1 : -1;
            timerStrafe = cambioDireccionTiempo;
        }
    }

    void HandleMovement()
    {
        float dist = Vector2.Distance(transform.position, objetivo.position);
        Vector2 dir = (objetivo.position - transform.position).normalized;
        Vector2 movimiento;

        if (dist > distanciaIdeal + 0.5f)
            movimiento = dir * velocidad;
        else if (dist < distanciaIdeal - 0.5f)
            movimiento = -dir * velocidad;
        else
            movimiento = new Vector2(-dir.y, dir.x) * dirStrafe * velocidadStrafe;

        velocidadActual = Vector2.Lerp(
            velocidadActual,
            movimiento,
            aceleracion * Time.deltaTime
        );

        AplicarMovimiento();

        if (sprite && Mathf.Abs(dir.x) > 0.05f)
            sprite.flipX = dir.x > 0;
    }

    void HandleShooting()
    {
        float dist = Vector2.Distance(transform.position, objetivo.position);

        if (dist <= rangoDisparo && timerDisparo <= 0f)
            StartCoroutine(Disparar());
    }

    IEnumerator Disparar()
    {
        timerDisparo = enfriamientoDisparo;
        anim?.PlayTrigger("atacando");

        yield return new WaitForSeconds(0.7f);

        if (Bloqueado() || JugadorIntocable())
            yield break;

        GameObject bala = Instantiate(balaPrefab, transform.position, Quaternion.identity);
        Vector2 dir = (objetivo.position - transform.position).normalized;

        bala.GetComponent<Rigidbody2D>()?.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
    }

    // 🔴 DAÑO
    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || fatalityEjecutada) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            StartCoroutine(EjecutarFatality());
            return;
        }

        anim?.PlayTrigger("defensa_1");

        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        velocidadRetroceso = dir * fuerzaRetroceso;

        StartCoroutine(Retroceso());
    }

    IEnumerator Retroceso()
    {
        enRetroceso = true;
        yield return new WaitForSeconds(tiempoRetroceso);
        enRetroceso = false;

        enStun = true;
        yield return new WaitForSeconds(tiempoStun);
        enStun = false;
    }

    IEnumerator EjecutarFatality()
    {
        estaMuerto = true;
        fatalityEjecutada = true;

        StopAllCoroutines();
        velocidadActual = Vector2.zero;

        if (col) col.enabled = false;

        bool fatalityDerecha = objetivo.position.x < transform.position.x;

        anim?.PlayTrigger(fatalityDerecha ? "fatality_izquierda" : "fatality_derecha");

        yield return new WaitForSeconds(duracionFatalityAnim);

        Destroy(gameObject);
    }
}
