using UnityEngine;

public class enemigo_gato : enemigo_base
{
    private enemigo_animacion anim;

    enum EstadoGato
    {
        Patrulla,
        Orbita,
        IntentandoAbducir,
        Abduciendo
    }

    private EstadoGato estadoActual;

    // =====================================================
    // 🌀 ÓRBITA
    // =====================================================
    [Header("Órbita")]
    public float distanciaMin = 8f;
    public float distanciaMax = 12f;
    public float velocidadOrbita = 4f;
    public float correccionRadial = 3f;
    public float cambioDireccionTiempo = 5f;

    private float timerCambioDir;
    private int direccionOrbita = 1;

    // ⏱️ Tiempo mínimo obligatorio en órbita
    [Header("Control de Órbita")]
    public float tiempoMinimoOrbita = 6f;
    private float timerOrbitaEstado;

    // =====================================================
    // 🔫 DISPARO
    // =====================================================
    [Header("Disparo")]
    public GameObject balaPrefab;
    public float velocidadBala = 8f;
    public float rangoDisparo = 5f;
    public float enfriamientoDisparo = 3f;
    private float cooldownDisparo;

    // =====================================================
    // 👽 ABDUCCIÓN
    // =====================================================
    [Header("Abducción")]
    public float alturaAbduccionY = 3.5f;
    public float velocidadAbduccion = 2f;
    public float rayDistancia = 3f;
    public float tiempoMaxIntento = 8f;

    private float timerIntento;
    private estado_jugador jugadorAbducido;

    // =====================================================
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<enemigo_animacion>();
        CambiarEstado(EstadoGato.Patrulla);
    }

    // =====================================================
    protected override void Update()
    {
        if (estaMuerto) return;

        cooldownDisparo -= Time.deltaTime;

        // =========================
        // SIN JUGADOR → PATRULLA
        // =========================
        if (objetivo == null)
        {
            CambiarEstado(EstadoGato.Patrulla);
            Patrullar();
            return;
        }

        // =========================
        // ESTADOS ESPECIALES
        // =========================
        if (estadoActual == EstadoGato.IntentandoAbducir)
        {
            IntentarAbduccion();
            return;
        }

        if (estadoActual == EstadoGato.Abduciendo)
        {
            MantenerAbduccion();
            return;
        }

        // =========================
        // MODO HABITUAL → ÓRBITA
        // =========================
        if (estadoActual != EstadoGato.Orbita)
            CambiarEstado(EstadoGato.Orbita);

        timerOrbitaEstado -= Time.deltaTime;

        MoverOrbita();
        Disparar();

        // 👽 Intentar abducción SOLO después de tiempo mínimo
        if (timerOrbitaEstado <= 0f &&
            JugadorAtacable() &&
            Random.value < 0.003f)
        {
            CambiarEstado(EstadoGato.IntentandoAbducir);
        }
    }

    // =====================================================
    void CambiarEstado(EstadoGato nuevo)
    {
        if (estadoActual == nuevo) return;

        estadoActual = nuevo;

        switch (nuevo)
        {
            case EstadoGato.Orbita:
                timerOrbitaEstado = tiempoMinimoOrbita;
                break;

            case EstadoGato.IntentandoAbducir:
                timerIntento = tiempoMaxIntento;
                break;
        }

        anim?.SetBool(
            "abduciendo",
            nuevo == EstadoGato.IntentandoAbducir || nuevo == EstadoGato.Abduciendo
        );

        Debug.Log($"[GATO] Estado → {estadoActual}");
    }

    // =====================================================
    bool JugadorAtacable()
    {
        if (objetivo == null) return false;
        var ej = objetivo.GetComponent<estado_jugador>();
        return ej != null && ej.PuedeAtacar();
    }

    // =====================================================
    // 🌀 MOVIMIENTO ORBITAL
    // =====================================================
    void MoverOrbita()
    {
        Vector2 dir = (objetivo.position - transform.position);
        float dist = dir.magnitude;
        dir.Normalize();

        if ((timerCambioDir -= Time.deltaTime) <= 0f)
        {
            direccionOrbita *= -1;
            timerCambioDir = cambioDireccionTiempo;
        }

        Vector2 tangente = new Vector2(-dir.y, dir.x) * direccionOrbita;

        Vector2 correccion = Vector2.zero;
        if (dist < distanciaMin) correccion = -dir * correccionRadial;
        else if (dist > distanciaMax) correccion = dir * correccionRadial;

        velocidadActual = Vector2.Lerp(
            velocidadActual,
            tangente * velocidadOrbita + correccion,
            aceleracion * Time.deltaTime
        );

        AplicarMovimiento();
    }

    // =====================================================
    // 👽 INTENTAR ABDUCCIÓN
    // =====================================================
    void IntentarAbduccion()
    {
        timerIntento -= Time.deltaTime;

        if (timerIntento <= 0f)
        {
            CambiarEstado(EstadoGato.Orbita);
            return;
        }

        Vector3 destino = objetivo.position + Vector3.up * alturaAbduccionY;
        Vector2 dir = (destino - transform.position).normalized;

        velocidadActual = dir * velocidadAbduccion;
        AplicarMovimiento();

        if (Mathf.Abs(transform.position.x - objetivo.position.x) < 0.3f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistancia);
            if (hit.collider)
            {
                var ej = hit.collider.GetComponent<estado_jugador>();
                if (ej && ej.PuedeAtacar())
                {
                    jugadorAbducido = ej;
                    ej.ActivarAbduccion();
                    CambiarEstado(EstadoGato.Abduciendo);
                }
            }
        }
    }

    // =====================================================
    // 👽 MANTENER ABDUCCIÓN
    // =====================================================
    void MantenerAbduccion()
    {
        if (jugadorAbducido == null || !jugadorAbducido.EstaAbducido)
        {
            jugadorAbducido = null;
            cooldownDisparo = 0f; // 👈 dispara de inmediato
            CambiarEstado(EstadoGato.Orbita);
            return;
        }

        Vector3 pos = jugadorAbducido.transform.position + Vector3.up * alturaAbduccionY;
        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 5f);
    }

    // =====================================================
    // 🔫 DISPARO
    // =====================================================
    void Disparar()
    {
        if (cooldownDisparo > 0f) return;
        if (Vector2.Distance(transform.position, objetivo.position) > rangoDisparo) return;

        cooldownDisparo = enfriamientoDisparo;
        anim?.PlayTrigger("atacando");
        Invoke(nameof(DisparoReal), 0.4f);
    }

    void DisparoReal()
    {
        if (objetivo == null) return;

        var bala = Instantiate(balaPrefab, transform.position, Quaternion.identity);
        Vector2 dir = (objetivo.position - transform.position).normalized;
        bala.GetComponent<Rigidbody2D>()?.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
    }

    // =====================================================
    // 🚶 PATRULLA (solo sin jugador)
    // =====================================================
    void Patrullar()
    {
        velocidadActual = Vector2.Lerp(
            velocidadActual,
            Random.insideUnitCircle.normalized * velocidadOrbita * 0.5f,
            aceleracion * Time.deltaTime
        );

        AplicarMovimiento();
    }
}
