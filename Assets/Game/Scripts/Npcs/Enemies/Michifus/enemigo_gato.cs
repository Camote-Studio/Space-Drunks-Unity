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
    [SerializeField] private string bulletTag = "Bala";   // Tag usado por PoolManager
    [SerializeField] private Transform puntoDisparo;      // Boca / arma del gato
    public float velocidadBala = 8f;
    public float rangoDisparo = 7f;
    public float enfriamientoDisparo = 2f;

    private float timerDisparo;

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

        //rb = GetComponent<Rigidbody2D>();

        // Evita conflictos si el enemigo usa NavMesh en otro modo
        if (agent != null)
        {
            //agent.updatePosition = false;
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
        if (objetivo != null && estadoActual == EstadoGato.Patrulla)
        {
            CambiarEstado(EstadoGato.Orbita);
        }
        
        if (estaMuerto)
        {
            CambiarEstado(EstadoGato.Muerto);
            return;
        }

        // Si no hay jugador, patrulla
        if (objetivo == null)
        {
            CambiarEstado(EstadoGato.Patrulla);
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
                IntentarDisparar();
                break;
            
            case EstadoGato.Abduciendo:
                // No hace nada, la zona controla el láser
                //rb.linearVelocity = Vector2.zero;
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

        // Tangente para orbitar
        Vector2 tangente = new Vector2(-dir.y, dir.x) * direccionOrbita;

        // Corrección radial
        Vector2 correccion = Vector2.zero;

        if (dist > distanciaMax)
            correccion = dir * correccionRadial;
        else if (dist < distanciaMin)
            correccion = -dir * correccionRadial;

        // Punto destino sobre NavMesh
        Vector2 destino = (Vector2)transform.position +
                        tangente * velocidadOrbita +
                        correccion;

        agent.isStopped = false;
        agent.SetDestination(destino);

        // Flip visual
        if (spriteRenderer != null && agent.velocity.x != 0)
            spriteRenderer.flipX = agent.velocity.x < 0;
    }

    // =====================================================
    // ATAQUE A DISTANCIA
    // =====================================================
    void IntentarDisparar()
    {
        if (timerDisparo > 0f) return;
        if (Vector2.Distance(transform.position, objetivo.position) > rangoDisparo) return;

        timerDisparo = enfriamientoDisparo;

        // Animación de ataque
        animator?.SetTrigger("atacando");

        // Delay para sincronizar con animación
        Invoke(nameof(DisparoReal), 0.5f);
    }

    void DisparoReal()
    {
        if (objetivo == null || estaMuerto) return;

        Vector3 origen = puntoDisparo != null
            ? puntoDisparo.position
            : transform.position;

        // Obtener bala del Pool
        GameObject bala = PoolManager.Instance.SpawnFromPool(
            bulletTag,
            origen,
            Quaternion.identity
        );

        if (bala == null) return;

        Vector2 dir = (objetivo.position - transform.position).normalized;

        // Rotación visual
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bala.transform.rotation = Quaternion.Euler(0, 0, angulo);

        // Impulso
        Rigidbody2D balaRB = bala.GetComponent<Rigidbody2D>();
        if (balaRB != null)
        {
            balaRB.linearVelocity = Vector2.zero;
            balaRB.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
        }
    }

    // =====================================================
    // PATRULLA (SIN JUGADOR)
    // =====================================================
    void Patrullar()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        agent.isStopped = true; // Quieto hasta ver al jugador
    }

    // =====================================================
    // MÉTODOS LLAMADOS POR LA ZONA (ZonaAbduccionGato)
    // =====================================================
    // 1. ¿Puede la zona atrapar al jugador?
    public bool PuedeAbducir()
    {
        return !estaMuerto;
    }

    // 2. Iniciar abducción
    public void IniciarAbduccion()
    {
        if (zonaAbduccion == null) return;

        CambiarEstado(EstadoGato.Abduciendo);

        if (agent != null)
            agent.isStopped = true;

        animator?.SetBool("abduciendo", true);
    }

    // 3. Finalizar abducción
    public void FinalizarAbduccion()
    {
        if (zonaAbduccion == null) return;

        if (agent != null)
            agent.isStopped = false;

        animator?.SetBool("abduciendo", false);

        CambiarEstado(EstadoGato.Orbita);
    }

}