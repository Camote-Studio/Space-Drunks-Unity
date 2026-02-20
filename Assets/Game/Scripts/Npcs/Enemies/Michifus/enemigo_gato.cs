using UnityEngine;
using System.Collections;

// ============================================================
//  enemigo_gato : enemigo_base
//  OVNI de beat-em-up. Comportamiento táctico:
//  - Dispara casi de inmediato al aparecer (primer disparo rápido)
//  - SIEMPRE quiere estar entre rangoDisparoMin y rangoDisparoMax
//  - Si el jugador se acerca sin atacar → se aleja activamente
//  - Si el jugador invade rangoHuir     → Abducción o huida de pánico
//  - Sistema de combo: tras N golpes seguidos, contraataca
// ============================================================
public class enemigo_gato : enemigo_base
{
    // ----------------------------------------------------------
    //  ESTADOS
    // ----------------------------------------------------------
    private enum EstadoGato
    {
        Patrulla,
        Posicionandose,
        Disparando,
        Retrocediendo,
        PreparandoAbduccion,
        Abduciendo,
        Stun,
        Muerto
    }

    private EstadoGato estadoActual;

    // ----------------------------------------------------------
    //  DISTANCIAS TÁCTICAS
    // ----------------------------------------------------------
    [Header("Distancias Tácticas")]
    public float rangoDisparoMax = 8f;  // Más lejos de esto → se acerca
    public float rangoDisparoMin = 5f;  // Más cerca de esto → se aleja
    public float rangoHuir       = 3f;  // Zona de pánico: abducción o huida

    // ----------------------------------------------------------
    //  DISPARO
    // ----------------------------------------------------------
    [Header("Disparo (Ráfaga de Hilo)")]
    [SerializeField] private string    bulletTag    = "Bala";
    [SerializeField] private Transform puntoDisparo;
    public float velocidadBala        = 8f;
    public int   bolasPorRafaga       = 3;
    public float tiempoEntreBolas     = 0.4f;
    public float enfriamientoDisparo  = 2f;
    [Tooltip("Cooldown del PRIMER disparo al hacer spawn. 0 = ataca casi de inmediato.")]
    public float primerDisparoDelay   = 0.5f; // <-- MUY CORTO para que ataque rápido al aparecer

    private float timerDisparo    = 0f;
    private bool  disparoEnCurso  = false;

    // ----------------------------------------------------------
    //  ABDUCCIÓN
    // ----------------------------------------------------------
    [Header("Abducción (Defensa)")]
    public float enfriamientoAbduccion = 6f;
    private float timerAbduccion;
    [Tooltip("Distancia en Y sobre el jugador para lanzar el rayo")]
    public float alturaAbduccion = 1.5f;

    // ----------------------------------------------------------
    //  RETROCESO
    // ----------------------------------------------------------
    [Header("Movimiento")]
    public float tiempoRetroceso = 1.2f;
    private float timerRetrocesoActual;

    // ----------------------------------------------------------
    //  STUN Y CONTRAATAQUE
    // ----------------------------------------------------------
    [Header("Stun y Contraataque")]
    public int   maxGolpesPermitidos = 3;
    public float tiempoStun          = 0.4f;
    public float fuerzaRetrocesoStun = 2f;
    private int  comboRecibido = 0;
    private bool enStun        = false;

    // Corrutina del parpadeo guardada para no matarla con StopAllCoroutines
    private Coroutine corrutinaParpadeeo;

    // ----------------------------------------------------------
    //  ZONA DE ABDUCCIÓN
    // ----------------------------------------------------------
    [SerializeField] private ZonaAbduccionGato zonaAbduccion;

    // ==========================================================
    //  INICIALIZACIÓN
    // ==========================================================
    protected override void Awake()
    {
        base.Awake();
        if (agent != null) { agent.updateRotation = false; agent.updateUpAxis = false; }
        if (zonaAbduccion == null) zonaAbduccion = GetComponentInChildren<ZonaAbduccionGato>();
        if (zonaAbduccion != null) zonaAbduccion.Configurar(this);

        timerDisparo = primerDisparoDelay;
        timerAbduccion = enfriamientoAbduccion;
        CambiarEstado(EstadoGato.Patrulla);
    }

    // ==========================================================
    //  BUCLE PRINCIPAL
    // ==========================================================
    protected override void Update()
    {
        base.Update();

        if (estaMuerto) { CambiarEstado(EstadoGato.Muerto); return; }
        if (objetivo == null) { CambiarEstado(EstadoGato.Patrulla); return; }

        ActualizarTimers();

        // Estados bloqueantes: la IA se pausa
        if (estadoActual == EstadoGato.Stun || estadoActual == EstadoGato.Abduciendo) return;

        float dist = Vector2.Distance(transform.position, objetivo.position);

        // ==========================================================
        //  REGLAS DE SUPERVIVENCIA (mayor prioridad)
        // ==========================================================
        if (dist <= rangoHuir)
        {
            if (timerAbduccion <= 0f && estadoActual != EstadoGato.PreparandoAbduccion)
            {
                // Rayo listo → posicionarse para abducir
                StopDisparoSeguro();
                CambiarEstado(EstadoGato.PreparandoAbduccion);
                return;
            }
            if (timerAbduccion > 0f
                && estadoActual != EstadoGato.Retrocediendo
                && estadoActual != EstadoGato.PreparandoAbduccion)
            {
                // Sin rayo → huida de pánico
                EmpezarRetroceso();
                return;
            }
        }

        // ==========================================================
        //  MÁQUINA DE ESTADOS
        // ==========================================================
        switch (estadoActual)
        {
            case EstadoGato.Patrulla:
                if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
                if (objetivo != null) CambiarEstado(EstadoGato.Posicionandose);
                break;

            case EstadoGato.Posicionandose:
                EjecutarPosicionamiento(dist);
                break;

            case EstadoGato.Disparando:
                // La corrutina lleva el control; solo orientamos el sprite
                MirarAlJugador();
                break;

            case EstadoGato.Retrocediendo:
                EjecutarRetroceso(dist);
                break;

            case EstadoGato.PreparandoAbduccion:
                EjecutarPreparacionAbduccion();
                break;
        }
    }

    // ==========================================================
    //  POSICIONAMIENTO TÁCTICO
    //
    //  Tres zonas claras:
    //  dist > rangoDisparoMax  → acercarse
    //  dist < rangoDisparoMin  → alejarse (y disparar si puede)
    //  zona ideal              → detenerse y disparar
    // ==========================================================
    void EjecutarPosicionamiento(float dist)
    {
        if (agent == null || !agent.isOnNavMesh || objetivo == null) return;

        if (dist > rangoDisparoMax)
        {
            // Demasiado lejos: acercarse al jugador
            agent.isStopped = false;
            agent.SetDestination(objetivo.position);
        }
        else if (dist < rangoDisparoMin)
        {
            // Demasiado cerca: calcular punto de retirada y moverse hacia él
            Vector2 dirAlejar  = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
            Vector2 puntoIdeal = (Vector2)objetivo.position + dirAlejar * (rangoDisparoMin + 0.5f);
            // +0.5f de margen para que no oscile en el borde exacto

            agent.isStopped = false;
            agent.SetDestination(puntoIdeal);

            // Dispara igualmente mientras retrocede: el jugador queda expuesto
            if (timerDisparo <= 0f && !disparoEnCurso)
                StartCoroutine(RutinaDisparoRafaga());
        }
        else
        {
            // Zona ideal: detenerse y disparar
            agent.isStopped = true;

            if (timerDisparo <= 0f && !disparoEnCurso)
                StartCoroutine(RutinaDisparoRafaga());
        }

        MirarAlJugador();
    }

    // ==========================================================
    //  UTILIDADES
    // ==========================================================
    void ActualizarTimers()
    {
        if (timerDisparo   > 0) timerDisparo   -= Time.deltaTime;
        if (timerAbduccion > 0) timerAbduccion -= Time.deltaTime;
    }

    void MirarAlJugador()
    {
        if (spriteRenderer != null && objetivo != null)
            spriteRenderer.flipX = objetivo.position.x < transform.position.x;
    }

    /// <summary>
    /// Cancela la corrutina de disparo de forma segura sin afectar el parpadeo.
    /// </summary>
    void StopDisparoSeguro()
    {
        // NO usamos StopAllCoroutines() para no matar el parpadeo rojo
        // La corrutina de ráfaga se autodestruye al comprobar el estado
        disparoEnCurso = false;
    }

    // ==========================================================
    //  SISTEMA DE DAÑO, STUN Y CONTRAATAQUE
    // ==========================================================
    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || estadoActual == EstadoGato.Abduciendo) return;

        comboRecibido = enStun ? comboRecibido + 1 : 1;
        vidaActual   -= cantidad;

        if (vidaActual <= 0) { Morir(); return; }

        if (comboRecibido > maxGolpesPermitidos) Contraatacar();
        else                                     EntrarEnStun();
    }

    IEnumerator ParpadeoRojoDaño()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            // Restaurar solo si nadie más cambió el color (ej: muerte)
            if (!estaMuerto) spriteRenderer.color = Color.white;
        }
        corrutinaParpadeeo = null;
    }

    void EntrarEnStun()
    {
        CambiarEstado(EstadoGato.Stun);
        enStun = true;

        if (agent != null && agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }

        // Cancelar el disparo sin matar el parpadeo
        StopDisparoSeguro();

        // Reiniciar parpadeo (cancelar el anterior si aún corre)
        if (corrutinaParpadeeo != null) StopCoroutine(corrutinaParpadeeo);
        corrutinaParpadeeo = StartCoroutine(ParpadeoRojoDaño());

        // Empujón en dirección contraria al jugador
        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
        if (rbLocal != null)
        {
            rbLocal.linearVelocity = Vector2.zero;
            rbLocal.AddForce(dir * fuerzaRetrocesoStun, ForceMode2D.Impulse);
        }

        CancelInvoke(nameof(FinStun));
        Invoke(nameof(FinStun), tiempoStun);
    }

    void FinStun()
    {
        if (estadoActual != EstadoGato.Stun || estaMuerto) return;
        enStun        = false;
        comboRecibido = 0;
        EmpezarRetroceso();
    }

    void Contraatacar()
    {
        enStun        = false;
        comboRecibido = 0;
        StopDisparoSeguro();
        CancelInvoke(nameof(FinStun));

        if (timerAbduccion <= 0f) CambiarEstado(EstadoGato.PreparandoAbduccion);
        else                      EmpezarRetroceso();
    }

    // ==========================================================
    //  ATAQUE — RÁFAGA DE HILO
    // ==========================================================

    /// <summary>
    /// La corrutina comprueba el estado en cada bala para poder ser
    /// interrumpida limpiamente sin necesidad de StopAllCoroutines.
    /// </summary>
    IEnumerator RutinaDisparoRafaga()
    {
        disparoEnCurso = true;
        CambiarEstado(EstadoGato.Disparando);
        timerDisparo = enfriamientoDisparo;

        if (estadoActual != EstadoGato.Retrocediendo)
        {
            CambiarEstado(EstadoGato.Disparando);
        }

        animator?.SetTrigger("atacando");
        yield return new WaitForSeconds(0.3f); // Wind-up estético

        for (int i = 0; i < bolasPorRafaga; i++)
        {
            // La corrutina se autodestruye si el estado cambió externamente
            if ((estadoActual != EstadoGato.Disparando && estadoActual != EstadoGato.Retrocediendo)|| estaMuerto || !disparoEnCurso)
            {
                disparoEnCurso = false;
                yield break;
            }
            DisparoReal();
            yield return new WaitForSeconds(tiempoEntreBolas);
        }

        disparoEnCurso = false;

        // Volver al posicionamiento para recalcular distancia inmediatamente
        if (!estaMuerto && estadoActual == EstadoGato.Disparando)
            CambiarEstado(EstadoGato.Posicionandose);
    }

    void DisparoReal()
    {
        if (objetivo == null) return;

        Vector3    origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        GameObject bala   = PoolManager.Instance.SpawnFromPool(bulletTag, origen, Quaternion.identity);

        if (bala != null)
        {
            Vector2 dir    = (objetivo.position - transform.position).normalized;
            float   angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bala.transform.rotation = Quaternion.Euler(0, 0, angulo);

            Rigidbody2D balaRB = bala.GetComponent<Rigidbody2D>();
            if (balaRB != null)
            {
                balaRB.linearVelocity = Vector2.zero;
                balaRB.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
            }
        }
    }

    // ==========================================================
    //  RETROCESO
    // ==========================================================
    void EmpezarRetroceso()
    {
        StopDisparoSeguro();
        CambiarEstado(EstadoGato.Retrocediendo);
        timerRetrocesoActual = tiempoRetroceso;
    }

    /// <summary>
    /// Huye del jugador. Cuando termina el tiempo, si el jugador
    /// sigue cerca vuelve a evaluar; si está lejos, posicionamiento normal.
    /// </summary>
    void EjecutarRetroceso(float dist)
    {
        if (agent == null || !agent.isOnNavMesh || objetivo == null) return;

        timerRetrocesoActual -= Time.deltaTime;

        Vector2 dirHuir  = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        Vector2 destHuir = (Vector2)transform.position + dirHuir * 2f;

        agent.isStopped = false;
        agent.SetDestination(destHuir);
        MirarAlJugador();

        if (timerDisparo <= 0f && !disparoEnCurso)
        {    
            StartCoroutine(RutinaDisparoRafaga());
        }

        if (timerRetrocesoActual <= 0f)
        {
                CambiarEstado(EstadoGato.Posicionandose);   
        }
    }

    // ==========================================================
    //  ABDUCCIÓN
    // ==========================================================
    public bool PuedeAbducir()
    {
        return !estaMuerto
               && estadoActual != EstadoGato.Abduciendo
               && estadoActual != EstadoGato.Stun;
    }

    void EjecutarPreparacionAbduccion()
    {
        if (agent == null || objetivo == null) return;

        Vector2 puntoIdealArriba = (Vector2)objetivo.position + new Vector2(0f, alturaAbduccion);

        // 1. PREVENCIÓN: Asegurarnos de que el punto al que quiere ir existe en el NavMesh
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(puntoIdealArriba, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            puntoIdealArriba = hit.position; // Usar la posición real y alcanzable más cercana
        }

        agent.isStopped = false;
        agent.SetDestination(puntoIdealArriba);
        MirarAlJugador();

        // 2. TOLERANCIA: Ampliamos un poco el rango para que no tenga que ser un posicionamiento milimétrico
        float distAlPunto = Vector2.Distance(transform.position, puntoIdealArriba);
        
        if (distAlPunto <= 0.6f) 
        {
            IniciarAbduccion();
        }
        // 3. PLAN B (ESCAPE DE ATASCO): Si el agente ya no tiene camino o llegó a su límite, pero no está cerca
        else if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Significa que chocó con una pared o no puede acercarse más al jugador por la geometría.
            // ¡No nos quedamos congelados! Abortamos la abducción temporalmente.
            
            timerAbduccion = 1.5f; // Le damos un cooldown corto para que no intente abducir en el mismo frame
            CambiarEstado(EstadoGato.Posicionandose); // Volver al combate normal (huir o disparar)
        }
    }

    public void IniciarAbduccion()
    {
        CambiarEstado(EstadoGato.Abduciendo);
        timerAbduccion = enfriamientoAbduccion;

        if (agent != null && agent.isOnNavMesh) { agent.isStopped = true; agent.velocity = Vector3.zero; }

        Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
        if (rbLocal != null) { rbLocal.linearVelocity = Vector2.zero; rbLocal.bodyType = RigidbodyType2D.Kinematic; }

        animator?.SetBool("abduciendo", true);
    }

    public void FinalizarAbduccion()
    {
        CambiarEstado(EstadoGato.Posicionandose);
        animator?.SetBool("abduciendo", false);

        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        Rigidbody2D rbLocal = GetComponent<Rigidbody2D>();
        if (rbLocal != null) rbLocal.bodyType = RigidbodyType2D.Dynamic;
    }

    // ==========================================================
    //  OBJECT POOL — REINICIO
    // ==========================================================
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();

        // Primer disparo rápido: usa primerDisparoDelay en vez del cooldown completo
        // Así el gato ataca casi de inmediato al aparecer en la wave
        timerDisparo   = primerDisparoDelay;
        timerAbduccion = enfriamientoAbduccion;

        enStun         = false;
        comboRecibido  = 0;
        disparoEnCurso = false;
        corrutinaParpadeeo = null;

        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        CambiarEstado(EstadoGato.Patrulla);
    }

    // ==========================================================
    //  CAMBIO DE ESTADO
    // ==========================================================
    void CambiarEstado(EstadoGato nuevoEstado)
    {
        if (estadoActual == nuevoEstado) return;
        estadoActual = nuevoEstado;
    }
}