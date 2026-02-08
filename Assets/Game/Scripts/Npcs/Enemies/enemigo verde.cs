using UnityEngine;
using System.Collections;

public class enemigoverde : enemigo_base
{
    [Header("Configuración de Proyectil")]
    [SerializeField] private string bulletTag = "Bala"; // Debe coincidir con el PoolManager
    [SerializeField] private Transform puntoDisparo;

    [Header("Ataque a Distancia")]
    public float velocidadBala = 8f;
    public float rangoDisparo = 6f;
    public float enfriamientoDisparo = 2f;

    [Header("Movimiento (Strafe)")]
    public float distanciaIdeal = 4.5f;
    public float velocidadMovimiento = 3f; // Velocidad de acercarse/alejarse
    public float velocidadStrafe = 2f;     // Velocidad lateral
    public float cambioDireccionTiempo = 1.5f;
    public float suavizadoMovimiento = 5f; // Para el Lerp del Rigidbody

    [Header("Fatality")]
    public float duracionFatalityAnim = 1.4f;

    [Header("Retroceso & Stun")]
    public float fuerzaRetroceso = 4f;
    public float tiempoRetroceso = 0.1f;
    public float tiempoStun = 0.25f;

    // Estados Internos
    private bool enRetroceso;
    private bool enStun;
    private bool fatalityEjecutada;
    private Rigidbody2D rb;

    // Timers
    private float timerDisparo;
    private float timerStrafe;
    private int dirStrafe = 1;

    // Referencia de velocidad para retroceso
    private Vector2 vectorRetroceso;

    // ===================== UNITY =====================

    protected override void Awake()
    {
        base.Awake(); // Inicializa referencias base
        rb = GetComponent<Rigidbody2D>();
        timerStrafe = cambioDireccionTiempo;
    }

    protected override void Update()
    {
        if (objetivo == null) return;

        // Si está muerto o en fatality, no hace nada (la corrutina de muerte maneja el resto)
        if (estaMuerto || fatalityEjecutada) return;

        HandleTimers();

        // Máquina de estados simple para movimiento
        if (enRetroceso)
        {
            // Movimiento forzado por el golpe
            if (rb) rb.linearVelocity = vectorRetroceso; 
        }
        else if (enStun)
        {
            // Quieto
            if (rb) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            // Comportamiento normal
            if (!JugadorIntocable())
            {
                HandleMovement();
                HandleShooting();
            }
            else
            {
                Detener();
            }
        }
    }

    // ===================== POOLING =====================

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();

        // Reset de estados
        enRetroceso = false;
        enStun = false;
        fatalityEjecutada = false;
        
        timerDisparo = enfriamientoDisparo; // Pequeño delay inicial
        timerStrafe = cambioDireccionTiempo;
        
        // Limpiar fuerzas previas
        if (rb) rb.linearVelocity = Vector2.zero;
    }

    public override void OnDespawnToPool()
    {
        base.OnDespawnToPool();
        StopAllCoroutines(); // Importante limpiar corrutinas al guardar
    }

    // ===================== LOGICA =====================

    void HandleTimers()
    {
        timerDisparo -= Time.deltaTime;
        timerStrafe -= Time.deltaTime;

        if (timerStrafe <= 0f)
        {
            dirStrafe = (Random.value > 0.5f) ? 1 : -1;
            timerStrafe = cambioDireccionTiempo;
        }
    }

    void HandleMovement()
    {
        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;
        Vector2 movimientoDeseado = Vector2.zero;

        // 1. Lógica de Distancia (Acercarse / Alejarse)
        if (dist > distanciaIdeal + 0.5f)
        {
            movimientoDeseado += dir * velocidadMovimiento;
        }
        else if (dist < distanciaIdeal - 0.5f)
        {
            movimientoDeseado -= dir * velocidadMovimiento;
        }

        // 2. Lógica de Strafe (Moverse de lado)
        // Obtenemos la tangente (-y, x)
        Vector2 tangente = new Vector2(-dir.y, dir.x);
        movimientoDeseado += tangente * (dirStrafe * velocidadStrafe);

        // 3. Aplicar al Rigidbody
        if (rb)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, movimientoDeseado, suavizadoMovimiento * Time.deltaTime);
        }

        // 4. Flip Visual
        if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.1f)
            spriteRenderer.flipX = dir.x < 0; // Ajustar según tu sprite
    }

    void HandleShooting()
    {
        float dist = Vector2.Distance(transform.position, objetivo.position);

        if (dist <= rangoDisparo && timerDisparo <= 0f)
        {
            StartCoroutine(RutinaDisparo());
        }
    }

    IEnumerator RutinaDisparo()
    {
        timerDisparo = enfriamientoDisparo;
        
        // Animación (usando SetTrigger para compatibilidad con Animator)
        if (animator) animator.SetTrigger("atacando");

        // Esperar al frame del disparo
        yield return new WaitForSeconds(0.5f); // Ajustar según tu animación

        // Validar que seguimos vivos y el jugador sigue ahí
        if (estaMuerto || enStun || objetivo == null) yield break;

        // --- SPAWN DESDE POOL ---
        Vector3 origen = puntoDisparo ? puntoDisparo.position : transform.position;
        GameObject bala = PoolManager.Instance.SpawnFromPool(bulletTag, origen, Quaternion.identity);

        if (bala != null)
        {
            Vector2 dir = (objetivo.position - transform.position).normalized;
            
            // Rotar bala
            float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bala.transform.rotation = Quaternion.Euler(0, 0, angulo);

            // Impulsar
            Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
            if (rbBala)
            {
                rbBala.linearVelocity = Vector2.zero; // Reset velocidad previa del pool
                rbBala.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
            }
        }
    }

    // ===================== DAÑO Y ESTADOS =====================

    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || fatalityEjecutada) return;

        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            StartCoroutine(EjecutarFatality());
            return;
        }

        // Animación de herido
        if (animator) animator.SetTrigger("daño"); // Asumiendo que tienes trigger "daño" o "defensa_1"

        // Calcular retroceso
        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        vectorRetroceso = dir * fuerzaRetroceso;

        // Iniciar secuencia de stun
        StartCoroutine(SecuenciaDaño());
    }

    IEnumerator SecuenciaDaño()
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
        
        // Detener movimiento físico
        if (rb) rb.linearVelocity = Vector2.zero;
        if (col) col.enabled = false;

        // Animación
        bool fatalityDerecha = objetivo.position.x < transform.position.x;
        // Asumiendo que tu animator tiene estos triggers
        string triggerAnim = fatalityDerecha ? "fatality_izquierda" : "fatality_derecha";
        if (animator) animator.SetTrigger(triggerAnim);

        yield return new WaitForSeconds(duracionFatalityAnim);

        // Retornar al Pool
        PoolManager.Instance.ReturnToPool(enemyPoolTag, gameObject);
    }

    // ===================== UTILIDADES =====================

    bool JugadorIntocable()
    {
        if (objetivo == null) return false;
        VidaJugador v = objetivo.GetComponent<VidaJugador>();
        return v != null && v.EsIntocable();
    }

    void Detener()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
    }
}