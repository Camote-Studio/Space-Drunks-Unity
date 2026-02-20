using UnityEngine;

/// <summary>
/// Enemigo tipo gato:
/// - Orbita alrededor del jugador
/// - Dispara proyectiles usando Pooling
/// - Usa una FSM simple para mantener el orden del comportamiento
/// </summary>
public class enemigo_gato : enemigo_base
{
    // =====================================================
    // ESTADOS (FSM SIMPLE)
    // =====================================================
    private enum EstadoGato
    {
        Patrulla,   // No hay jugador
        Orbita,     // Orbita y ataca
        Abduciendo, 
        Muerto
    }

    private EstadoGato estadoActual;

    // =====================================================
    // CONFIGURACIÓN DE DISPARO (POOLING)
    // =====================================================
    [Header("Disparo")]
    [SerializeField] private string bulletTag = "Bala";   
    [SerializeField] private Transform puntoDisparo;      
    public float velocidadBala = 8f;
    public float rangoDisparo = 7f;
    public float enfriamientoDisparo = 2f;

    private float timerDisparo;

    // =====================================================
    // 🛸 CONFIGURACIÓN DE ABDUCCIÓN (¡NUEVO!)
    // =====================================================
    [Header("Abducción (IA)")]
    public float rangoAbduccion = 2.5f; // A qué distancia decide soltar el rayo
    public float enfriamientoAbduccion = 6f; // Cuánto tarda en volver a intentarlo

    private float timerAbduccion;

    // =====================================================
    // 🌀 MOVIMIENTO ORBITAL
    // =====================================================
    [Header("Órbita")]
    public float distanciaMin = 3.5f;
    public float distanciaMax = 6f;
    public float velocidadOrbita = 4f;
    public float correccionRadial = 3f;
    public float cambioDireccionTiempo = 5f;
    public float suavizadoMovimiento = 5f;

    private float timerCambioDireccion;
    private int direccionOrbita = 1;

    [SerializeField] private ZonaAbduccionGato zonaAbduccion;

    // =====================================================
    // CICLO DE VIDA
    // =====================================================
    protected override void Awake()
    {
        base.Awake();

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        if (zonaAbduccion == null)
        {
            zonaAbduccion = GetComponentInChildren<ZonaAbduccionGato>();
        }

        if (zonaAbduccion != null)
        {
            zonaAbduccion.Configurar(this);
        }

        CambiarEstado(EstadoGato.Patrulla);
    }

    protected override void Update()
    {
        base.Update();
        
        if (estaMuerto)
        {
            CambiarEstado(EstadoGato.Muerto);
            return;
        }

        if (objetivo == null)
        {
            CambiarEstado(EstadoGato.Patrulla);
            return; // Añadido un return por seguridad para no ejecutar código extra
        }
        else if (estadoActual == EstadoGato.Patrulla)
        {
            CambiarEstado(EstadoGato.Orbita);
        }

        // Ejecutar lógica según el estado
        switch (estadoActual)
        {
            case EstadoGato.Patrulla:
                Patrullar();
                break;

            case EstadoGato.Orbita:
                ActualizarTimers();
                MoverOrbita();
                IntentarAbducir(); // <-- NUEVO: Ahora el gato decide si abduce
                IntentarDisparar();
                break;
            
            case EstadoGato.Abduciendo:
                // El gato se queda quieto anclado, esperando que termine la animación
                if (agent != null)
                    agent.isStopped = true;
                break;
        }
    }

    // =====================================================
    // RESET AL SALIR DEL POOL
    // =====================================================
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();

        timerDisparo = enfriamientoDisparo;
        timerAbduccion = enfriamientoAbduccion; // Reiniciamos el timer de abducción
        timerCambioDireccion = cambioDireccionTiempo;
        direccionOrbita = Random.value > 0.5f ? 1 : -1;

        CambiarEstado(EstadoGato.Patrulla);
    }

    // =====================================================
    // CAMBIO DE ESTADO
    // =====================================================
    void CambiarEstado(EstadoGato nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        estadoActual = nuevoEstado;
        Debug.Log($"[GATO] Estado → {estadoActual}");
    }

    // =====================================================
    // TEMPORIZADORES
    // =====================================================
    void ActualizarTimers()
    {
        timerDisparo -= Time.deltaTime;
        timerAbduccion -= Time.deltaTime; // Restamos tiempo al cooldown de abducción
        timerCambioDireccion -= Time.deltaTime;

        if (timerCambioDireccion <= 0f)
        {
            direccionOrbita *= -1;
            timerCambioDireccion = cambioDireccionTiempo;
        }
    }

    // =====================================================
    // MOVIMIENTO ORBITAL
    // =====================================================
    void MoverOrbita()
    {
        if (agent == null || !agent.isOnNavMesh || objetivo == null)
            return;

        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;

        Vector2 tangente = new Vector2(-dir.y, dir.x) * direccionOrbita;
        Vector2 correccion = Vector2.zero;

        if (dist > distanciaMax)
            correccion = dir * correccionRadial;
        else if (dist < distanciaMin)
            correccion = -dir * correccionRadial;

        Vector2 destino = (Vector2)transform.position + tangente * velocidadOrbita + correccion;

        agent.isStopped = false;
        agent.SetDestination(destino);

        if (spriteRenderer != null && agent.velocity.x != 0)
            spriteRenderer.flipX = agent.velocity.x < 0;
    }

    // =====================================================
    // INTELIGENCIA DE ATAQUES
    // =====================================================
    
    // --- 1. INTENTO DE ABDUCCIÓN (NUEVO) ---
    void IntentarAbducir()
    {
        if (timerAbduccion > 0f) return;
        if (Vector2.Distance(transform.position, objetivo.position) > rangoAbduccion) return;

        // Si está cerca y tiene recarga, inicia la secuencia
        timerAbduccion = enfriamientoAbduccion;
        IniciarAbduccion();
    }

    // --- 2. INTENTO DE DISPARO ---
    void IntentarDisparar()
    {
        if (timerDisparo > 0f) return;
        
        // Evitar que dispare si justo acaba de decidir abducir
        if (estadoActual == EstadoGato.Abduciendo) return; 

        if (Vector2.Distance(transform.position, objetivo.position) > rangoDisparo) return;

        timerDisparo = enfriamientoDisparo;
        animator?.SetTrigger("atacando");
        Invoke(nameof(DisparoReal), 0.5f);
    }

    void DisparoReal()
    {
        if (objetivo == null || estaMuerto) return;

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        GameObject bala = PoolManager.Instance.SpawnFromPool(bulletTag, origen, Quaternion.identity);

        if (bala == null) return;

        Vector2 dir = (objetivo.position - transform.position).normalized;
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bala.transform.rotation = Quaternion.Euler(0, 0, angulo);

        Rigidbody2D balaRB = bala.GetComponent<Rigidbody2D>();
        if (balaRB != null)
        {
            balaRB.linearVelocity = Vector2.zero;
            balaRB.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
        }
    }

    void Patrullar()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        agent.isStopped = true; 
    }

    // =====================================================
    // EJECUCIÓN FÍSICA Y ANIMACIÓN
    // =====================================================
    public bool PuedeAbducir()
    {
        return !estaMuerto && estadoActual != EstadoGato.Abduciendo;
    }

    public void IniciarAbduccion()
    {
        CambiarEstado(EstadoGato.Abduciendo);

        if (agent != null)
        {
            agent.isStopped = true; 
            agent.velocity = Vector3.zero;
        }
        
        Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
        if (rbLocal != null)
        {
            rbLocal.linearVelocity = Vector2.zero;
            rbLocal.bodyType = RigidbodyType2D.Kinematic; 
        }

        animator?.SetBool("abduciendo", true);
    }

    // ESTA ES LA FUNCIÓN QUE DEBES LLAMAR DESDE EL ANIMATION EVENT AL FINAL DE LA ANIMACIÓN
    public void FinalizarAbduccion()
    {
        CambiarEstado(EstadoGato.Orbita);
        animator?.SetBool("abduciendo", false);

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;

        Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
        if (rbLocal != null)
        {
            rbLocal.bodyType = RigidbodyType2D.Dynamic; 
        }
    }
}