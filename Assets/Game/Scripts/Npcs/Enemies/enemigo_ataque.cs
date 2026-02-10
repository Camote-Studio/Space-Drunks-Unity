using UnityEngine;
using System.Collections;

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
        if (enemigo == null || objetivo == null || enemigo.estaMuerto) return;

        temporizadorAtaque -= Time.deltaTime;

        // Verificar distancia
        float distancia = Vector2.Distance(transform.position, objetivo.position);

        // Calcula si el jugador está frente al enemigo
        Vector2 dirAlJugador = (objetivo.position - transform.position).normalized;
        float mirandoHacia = transform.localScale.x; // 1 o -1 según tu lógica de escala

        // Si el enemigo mira a la derecha (1) y el jugador está a la izquierda (-), no ataca
        bool estaDeFrente = (mirandoHacia < 0 && dirAlJugador.x < 0) || (mirandoHacia > 0 && dirAlJugador.x > 0);

        if (!enemigo.estaAtacando && temporizadorAtaque <= 0f && distancia <= rangoAtaque && estaDeFrente)
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
        StopAllCoroutines(); // Asegura que no haya secuencias de ataque corriendo

        enemigo.estaAtacando = true;
        temporizadorAtaque = enfriamiento;

        // 1. Iniciar Animación
        if (animator != null)
        {
            animator.SetTrigger("atacando"); // Usa Trigger, es mejor que Bool para golpes
        }
        StartCoroutine(SecuenciaAtaque());
    }

    IEnumerator SecuenciaAtaque()
    {
        // Esperar el momento del impacto
        yield return new WaitForSeconds(delayImpacto);

        if (enemigo != null && !enemigo.estaMuerto)
        {

            float distanciaReal = Vector2.Distance(transform.position, objetivo.position);

            if (distanciaReal <= rangoAtaque + 0.3f) 
            {
                AplicarDaño();
            }
            else 
            {
                Debug.Log("El jugador esquivó el ataque!");
            }
        }

        // Esperar a que termine la animación antes de permitir otro ataque
        yield return new WaitForSeconds(duracionAnimacion - delayImpacto);
        FinalizarAtaque();
    }
    void AplicarDaño()
    {
        if (enemigo == null || enemigo.estaMuerto) return;

            // 1. Validación de Distancia (con un margen pequeño)
            float distancia = Vector2.Distance(transform.position, objetivo.position);
            if (distancia > rangoAtaque + 0.3f) return;

            // 2. Validación de Dirección (Evitar daño si el jugador te pasó de largo)
            Vector2 dirAlJugador = (objetivo.position - transform.position).normalized;
            
            // Obtenemos hacia dónde mira el pato basándonos en su escala
            // Si escala X es negativa, mira a un lado; si es positiva, al otro.
            float mirandoHacia = Mathf.Sign(transform.localScale.x);

            // Ajustamos según tu configuración de 'spriteMiraALaIzquierdaPorDefecto'
            // Si el pato mira a la izquierda y el jugador está a la derecha, el ataque falla.
            bool jugadorEstaEnFrente = (mirandoHacia < 0 && dirAlJugador.x < 0) || (mirandoHacia > 0 && dirAlJugador.x > 0);

            if (!jugadorEstaEnFrente) return;

            // 3. Aplicar Daño
            var vida = objetivo.GetComponentInParent<VidaJugador>();
            if (vida != null)
            {
                vida.RecibirDanio(daño);
            }
    }

        public void InterrumpirAtaque()
    {
        StopAllCoroutines();
        enemigo.estaAtacando = false;
        // Opcional: Volver a idle en el animator
        animator.Play("idle"); 
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