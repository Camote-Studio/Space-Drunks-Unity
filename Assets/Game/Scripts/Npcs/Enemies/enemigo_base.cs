using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System;

public abstract class enemigo_base : MonoBehaviour, IPoolable
{

    //EVENTO GLOBAL: script pueden suscribirse para saber cuando un enemigo muere
    public static event Action OnAnyEnemyDeath;
    [Header("XP Drop")]
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private int cantidadOrbes = 15;


    // ===================== VARIABLES CONFIGURABLES =====================
    [Header("Configuración Visual")]
    [SerializeField] public bool spriteMiraALaIzquierdaPorDefecto = true; // Ajusta esto en el Inspector

    [Header("Stats")]
    public float vidaMaxima = 100f;
    protected float vidaActual;

    [Header("Pooling")]
    [SerializeField] protected string enemyPoolTag;

    // ===================== VARIABLES PUBLICAS (CAMBIOS AQUÍ) =====================

    // CAMBIO 1: De 'protected' a 'public' para que enemigo_ataque pueda leerlo
    public bool estaMuerto;

    // CAMBIO 2: Agregamos esta variable nueva que faltaba
    [HideInInspector] public bool estaAtacando;

    // ===================== VARIABLES INTERNAS =====================
    protected Transform objetivo;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;

    // Variable para física (Agregada para soportar el movimiento de enemigoverde/gato)
    protected Rigidbody2D rb;


    // ===================== UNITY =====================

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // Referencia a RB

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            objetivo = player.transform;
    }

    protected virtual void Update()
    {

        if (estaMuerto || objetivo == null) return;

        OrientarHaciaObjetivo();

        float distanciaAlJugador = Vector3.Distance(transform.position, objetivo.position);

        if (estaAtacando && distanciaAlJugador > 2.5f)
        {
            estaAtacando = false; // Dejar de atacar si el jugador se aleja demasiado
        }

        if (estaAtacando)
        {
            //Si no nos movemos, frenamos al agente
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;

        }

        //Reactivar el movimiento
        if (agent != null && agent.isOnNavMesh) agent.isStopped = false;

        // Lógica base
        MoverHaciaObjetivo();

    }

    // ===================== POOL =====================

    public virtual void OnSpawnFromPool()
    {
        // Reiniciar variables vitales
        estaMuerto = false;
        estaAtacando = false; // <--- Importante resetear esto
        vidaActual = vidaMaxima;

        if (col != null) col.enabled = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Reset NavMesh si existe
        if (agent != null)
        {
            // Primero aseguramos que el componente esté encendido pero sin calcular nada aún
            agent.enabled = true;
            agent.updateRotation = false;
            agent.updateUpAxis = false;

            // INTENTO DE COLOCACIÓN SEGURA
            // Buscamos el punto de NavMesh más cercano en un radio de 3 unidades
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 3.0f, NavMesh.AllAreas))
            {
                // ¡ÉXITO! Encontramos suelo azul cerca.
                // 1. Lo teletransportamos a esa posición válida (hit.position)
                agent.Warp(hit.position);

                // 2. Ahora que sabemos que está en el suelo, es seguro llamar a estos métodos:
                agent.ResetPath();
                agent.isStopped = false;
            }
            else
            {
                // FALLO: No hay NavMesh cerca (Spawn point en el vacío o muy lejos)
                Debug.LogError($"[Enemigo] ERROR CRÍTICO: SpawnPoint en {transform.position} está demasiado lejos del NavMesh. El enemigo no se moverá.");

                // Apagamos el agente para evitar que lance errores en el Update
                agent.enabled = false;
            }
        }

        // Reset Rigidbody si existe
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        ResetVisuals();

        //Vector3 currentPos = transform.position;
        //currentPos.z = UnityEngine.Random.Range(-0.05f, 0.05f);
        //transform.position = currentPos;
    }

    public virtual void OnDespawnToPool()
    {
        transform.DOKill();
        StopAllCoroutines(); // Buena práctica al guardar
    }

    // ===================== COMBATE =====================

    public virtual void RecibirDaño(float daño)
    {
        if (estaMuerto) return;

        vidaActual -= daño;
        GetComponent<enemigo_ataque>()?.StopAllCoroutines();
        estaAtacando = false;

        // Feedback visual simple
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.DOColor(Color.red, 0.1f)
                .OnComplete(() => { spriteRenderer.DOColor(Color.white, 0.1f); });
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }
    protected virtual void Morir()
    {
        estaMuerto = true;

        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 🔥 GENERAR ORBES DE EXPERIENCIA
        if (xpOrbPrefab != null)
        {
            for (int i = 0; i < cantidadOrbes; i++)
            {
                Vector3 spawnPos = transform.position;

                GameObject orb = Instantiate(xpOrbPrefab, spawnPos, Quaternion.identity);

                Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
                if (orbRb != null)
                {
                    Vector2 randomForce = new Vector2(
                        UnityEngine.Random.Range(-2f, 2f),
                        UnityEngine.Random.Range(1f, 3f)
                    );

                    orbRb.AddForce(randomForce, ForceMode2D.Impulse);
                }
            }
        }

        // Avisar sistema que un enemigo murió
        OnAnyEnemyDeath?.Invoke();

        PoolManager.Instance.ReturnToPool(enemyPoolTag, gameObject);
    }



    // ===================== VISUAL =====================

    protected virtual void ResetVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
        }
    }

    // ===================== MOVIMIENTO =====================

    protected virtual void MoverHaciaObjetivo()
    {
        if (agent != null && objetivo == null) return;

        if (agent.isOnNavMesh)
        {
            if (Vector3.Distance(transform.position, objetivo.position) > 0.3f)
            {
                // Esta sola línea hace todo el Pathfinding (A*)
                agent.SetDestination(objetivo.position);
            }
        }
        // else
        // {
        //     NavMeshHit hit;
        //     if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
        //     {
        //         agent.Warp(transform.position);
        //     }
        // }
    }

    protected virtual void OrientarHaciaObjetivo()
    {
        if (objetivo != null)
        {
            float diferenciaX = objetivo.position.x - transform.position.x;
            if (Mathf.Abs(diferenciaX) > 0.2f)
            {
                bool jugadorALaDerecha = diferenciaX > 0;

                float newEscalaX = spriteMiraALaIzquierdaPorDefecto ? (jugadorALaDerecha ? -1f : 1f) : (jugadorALaDerecha ? 1f : -1f);
                // Aplicamos a la escala del objeto (o del hijo que tiene el sprite)
                transform.localScale = new Vector3(newEscalaX, 1f, 1f);
            }
        }
    }
}