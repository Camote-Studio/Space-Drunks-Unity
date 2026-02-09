using UnityEngine;

[RequireComponent(typeof(enemigo_base))]
public class enemigo_ataque : MonoBehaviour, IPoolable
{
    [Header("Configuración de Ataque")]
    public float rangoAtaque = 1.5f;
    public float daño = 10f;
    public float enfriamiento = 2f;
    
    [Header("Sincronización")]
    [Tooltip("Tiempo desde que inicia la animación hasta que se aplica el daño")]
    public float delayImpacto = 0.3f; 
    [Tooltip("Duración total de la animación de ataque")]
    public float duracionAnimacion = 0.8f;

    private float temporizadorAtaque;
    private enemigo_base enemigo;
    private Animator animator; // Referencia directa o usa tu wrapper enemigo_animacion
    private Transform objetivo;

    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
        animator = GetComponentInChildren<Animator>();
        objetivo = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Validaciones de seguridad
        if (enemigo == null || objetivo == null) return;
        if (enemigo.estaMuerto) return;

        temporizadorAtaque -= Time.deltaTime;

        // Verificar distancia
        float distancia = Vector2.Distance(transform.position, objetivo.position);

        if (!enemigo.estaAtacando && temporizadorAtaque <= 0f && distancia <= rangoAtaque)
        {
            IniciarAtaque();
        }
    }

    // ===================== POOLING =====================
    // Este script necesita reiniciar sus propios timers cuando el enemigo vuelve del pool
    public void OnSpawnFromPool()
    {
        temporizadorAtaque = 0.5f; // Pequeño delay inicial para que no ataque instantáneo al spawnear
        enemigo.estaAtacando = false;
        CancelInvoke(); // Limpiar ataques pendientes
    }

    public void OnDespawnToPool()
    {
        CancelInvoke();
    }

    // ===================== LÓGICA DE ATAQUE =====================

    void IniciarAtaque()
    {
        enemigo.estaAtacando = true;
        temporizadorAtaque = enfriamiento;

        // 1. Iniciar Animación
        if (animator != null)
        {
            animator.SetTrigger("atacando"); // Usa Trigger, es mejor que Bool para golpes
        }

        // 2. Programar el daño (para que coincida con el golpe visual)
        Invoke(nameof(AplicarDaño), delayImpacto);

        // 3. Programar el fin del estado de ataque (para volver a moverse)
        Invoke(nameof(FinalizarAtaque), duracionAnimacion);
    }

    void AplicarDaño()
    {
        // Verificar si el enemigo murió o fue stuneado durante el delay
        if (enemigo.estaMuerto) return;

        // Verificar si el jugador sigue en rango (opcional, si quieres que el ataque pueda fallar)
        float distancia = Vector2.Distance(transform.position, objetivo.position);
        if (distancia > rangoAtaque + 0.5f) return; // +0.5f de margen de error

        var vida = objetivo.GetComponentInParent<VidaJugador>();
        if (vida != null)
        {
            vida.RecibirDanio(daño);
        }
    }

    void FinalizarAtaque()
    {
        enemigo.estaAtacando = false;
    }

    // ===================== DEBUG =====================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}