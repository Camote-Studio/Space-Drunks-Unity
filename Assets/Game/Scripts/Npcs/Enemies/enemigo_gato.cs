using UnityEngine;

public class enemigo_gato : enemigo_base
{
    [Header("Configuración de Proyectil")]
    [SerializeField] private string bulletTag = "Bala"; // IMPORTANTE: El tag definido en el PoolManager
    [SerializeField] private Transform puntoDisparo;    // Asigna un objeto hijo en la boca del gato

    [Header("Movimiento en Órbita")]
    public float distanciaMin = 3.5f;
    public float distanciaMax = 6f;
    public float velocidadOrbita = 4f;
    public float correccionRadial = 3f;
    public float cambioDireccionTiempo = 5f;
    public float suavizadoMovimiento = 5f; // Reemplaza la 'aceleración' que faltaba

    [Header("Ataque a Distancia")]
    public float velocidadBala = 8f;
    public float rangoDisparo = 7f;
    public float enfriamientoDisparo = 2f;
    
    // Variables de estado
    private float temporizadorDisparo;
    private float temporizadorOrbita;
    private int direccionOrbita = 1;
    //private Rigidbody2D rb; // Referencia local para física

    // ===================== UNITY =====================

    protected override void Awake()
    {
        base.Awake();
        // Obtenemos el RB. Si no existe en el padre, lo agregamos o buscamos.
        //rb = GetComponent<Rigidbody2D>();
        
        // Si usas NavMeshAgent en el padre pero quieres orbitar con física,
        // es recomendable desactivar el update del agente para que no pelee con el RB.
        if(agent != null) 
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }

    protected override void Update()
    {
        if (estaMuerto || objetivo == null) return;

        ManejarTemporizadores();
        MoverHaciaObjetivo(); // Sobreescribe la lógica base
        IntentarDisparar();
    }

    // ===================== POOLING =====================

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        
        // Reset específico del Gato
        temporizadorDisparo = enfriamientoDisparo; // Dar un pequeño respiro al aparecer
        temporizadorOrbita = cambioDireccionTiempo;
        direccionOrbita = (Random.value > 0.5f) ? 1 : -1; // Dirección aleatoria al nacer
    }

    // ===================== LÓGICA INTERNA =====================

    void ManejarTemporizadores()
    {
        temporizadorDisparo -= Time.deltaTime;
        temporizadorOrbita -= Time.deltaTime;

        if (temporizadorOrbita <= 0f)
        {
            direccionOrbita *= -1;
            temporizadorOrbita = cambioDireccionTiempo;
        }
    }

    protected override void MoverHaciaObjetivo()
    {
        // Vector hacia el jugador
        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;

        // 1. Calcular vector Tangente (para orbitar)
        Vector2 tangente = new Vector2(-dir.y, dir.x) * direccionOrbita;

        // 2. Calcular Corrección Radial (acercarse o alejarse)
        Vector2 correccion = Vector2.zero;
        if (dist > distanciaMax)
            correccion = dir * correccionRadial;     // Acércate
        else if (dist < distanciaMin)
            correccion = -dir * correccionRadial;    // Aléjate

        // 3. Vector final deseado
        Vector2 movimientoDeseado = (tangente * velocidadOrbita) + correccion;

        // 4. Aplicar física (Suavizado)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, movimientoDeseado, suavizadoMovimiento * Time.deltaTime);
        }
        else
        {
            // Fallback si no hay Rigidbody (movimiento directo)
            transform.position += (Vector3)movimientoDeseado * Time.deltaTime;
        }

        // 5. Visual Flip (Usando la variable spriteRenderer de la base)
        if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.1f)
        {
            spriteRenderer.flipX = dir.x < 0; // Ajustar según tu sprite original
        }
    }

    void IntentarDisparar()
    {
        float distancia = Vector2.Distance(transform.position, objetivo.position);

        // Condiciones para NO disparar
        if (distancia > rangoDisparo) return;
        if (temporizadorDisparo > 0f) return;

        // Iniciar secuencia de disparo
        temporizadorDisparo = enfriamientoDisparo;
        
        if (animator != null)
        {
            // Usamos SetTrigger en lugar de PlayTrigger si es el Animator nativo de Unity,
            // o tu wrapper 'enemigo_animacion' si lo mantienes.
            animator.SetTrigger("atacando"); 
        }

        // Delay para sincronizar con la animación
        Invoke(nameof(DisparoReal), 0.5f); 
    }

    void DisparoReal()
    {
        if (estaMuerto || objetivo == null) return;

        // Usar la posición del punto de disparo si existe, sino el centro del gato
        Vector3 origen = (puntoDisparo != null) ? puntoDisparo.position : transform.position;

        // 1. SOLICITAR AL POOL MANAGER (Aquí está la magia)
        GameObject balaObj = PoolManager.Instance.SpawnFromPool(bulletTag, origen, Quaternion.identity);

        if (balaObj != null)
        {
            Vector2 dir = (objetivo.position - transform.position).normalized;
            
            // Rotar la bala hacia el objetivo (opcional, visual)
            float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            balaObj.transform.rotation = Quaternion.Euler(0, 0, angulo);

            // 2. IMPULSAR LA BALA
            // Asumimos que la bala tiene Rigidbody2D o su propio script de movimiento
            Rigidbody2D balaRB = balaObj.GetComponent<Rigidbody2D>();
            if (balaRB != null)
            {
                balaRB.linearVelocity = Vector2.zero; // Resetear velocidad anterior del pool
                balaRB.AddForce(dir * velocidadBala, ForceMode2D.Impulse);
            }
        }
    }
}