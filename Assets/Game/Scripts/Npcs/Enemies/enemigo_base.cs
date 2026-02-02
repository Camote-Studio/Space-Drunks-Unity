using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public abstract class enemigo_base : MonoBehaviour, IPoolable
{
    [Header("Persecución")]
    public float distanciaParada = 1.2f;

    [Header("Estadísticas")]
    public float vidaMaxima = 100f;
    public float velocidad = 3f;
    public float aceleracion = 8f;

    protected float vidaActual;

    [Header("Pooling")]
    [SerializeField] protected string enemyPoolTag;

    // ===================== ESTADO =====================
    public bool estaMuerto;
    [HideInInspector] public bool estaAtacando;

    // ===================== REFERENCIAS =====================
    protected Transform objetivo;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Collider2D col;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    // ===================== EVENTOS =====================
    public static System.Action OnAnyEnemyDeath;

    // ===================== UNITY =====================
    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        vidaActual = vidaMaxima;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = velocidad;
        }

        ActualizarObjetivoMasCercano();
    }

    protected virtual void Update()
    {
        if (estaMuerto) return;

        if (objetivo == null)
        {
            ActualizarObjetivoMasCercano();
            return;
        }

        if (estaAtacando)
        {
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;
            return;
        }

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;

        MoverHaciaObjetivo();
    }

    // =====================================================
    // 🎯 BUSCAR PLAYER MÁS CERCANO
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

        try { objs = GameObject.FindGameObjectsWithTag(tag); }
        catch { return; }

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
    // 🏃 MOVIMIENTO (NAVMESH)
    // =====================================================
    protected virtual void MoverHaciaObjetivo()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        float dist = Vector2.Distance(transform.position, objetivo.position);

        if (dist <= distanciaParada)
        {
            agent.SetDestination(transform.position);
            return;
        }

        agent.SetDestination(objetivo.position);

        // Flip del sprite
        if (spriteRenderer != null && agent.velocity.x != 0)
            spriteRenderer.flipX = agent.velocity.x < 0;
    }

    // =====================================================
    // ❤️ DAÑO
    // =====================================================
    public virtual void RecibirDaño(float daño)
    {
        if (estaMuerto) return;

        vidaActual -= daño;

        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            spriteRenderer.DOColor(Color.red, 0.1f)
                .OnComplete(() => spriteRenderer.DOColor(Color.white, 0.1f));
        }

        if (vidaActual <= 0)
            Morir();
    }

    protected virtual void Morir()
    {
        estaMuerto = true;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        if (col != null) col.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        OnAnyEnemyDeath?.Invoke();

        PoolManager.Instance.ReturnToPool(enemyPoolTag, gameObject);
    }

    // =====================================================
    // ♻️ POOLING
    // =====================================================
    public virtual void OnSpawnFromPool()
    {
        estaMuerto = false;
        estaAtacando = false;
        vidaActual = vidaMaxima;

        if (col != null) col.enabled = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
        }

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public virtual void OnDespawnToPool()
    {
        transform.DOKill();
        StopAllCoroutines();
    }
}
