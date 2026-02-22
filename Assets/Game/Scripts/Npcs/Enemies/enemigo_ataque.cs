using UnityEngine;
using System.Collections;

[RequireComponent(typeof(enemigo_base))]
public class enemigo_ataque : MonoBehaviour, IPoolable
{
    [Header("Configuración de Ataque y Combo")]
    public float rangoAtaque = 1.5f;
    public float dañoPorGolpe = 10f;
    public float enfriamiento = 2f;
    
    [Header("Secuencia de Combo")]
    [Tooltip("Cuántos golpes da el enemigo en su combo")]
    public int cantidadGolpesCombo = 3;
    [Tooltip("Tiempo entre cada puñetazo del combo")]
    public float tiempoEntreGolpes = 0.5f;
    [Tooltip("Tiempo desde que inicia la animación de un golpe hasta que hace el daño")]
    public float delayImpacto = 0.2f;

    [Header("Efectos en el Jugador")]
    public float tiempoDerribo = 1.5f; // Cuánto tiempo queda tumbado el jugador al final

    private float temporizadorAtaque;
    private enemigo_base enemigo;
    private Animator animator;
    private Transform objetivo;

    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
        animator = GetComponentInChildren<Animator>();
        objetivo = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (enemigo == null || objetivo == null || enemigo.estaMuerto) return;

        if (temporizadorAtaque > 0) temporizadorAtaque -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, objetivo.position);

        // Si el cooldown terminó, no estamos atacando y el jugador está en rango: INICIAMOS COMBO
        if (!enemigo.estaAtacando && temporizadorAtaque <= 0f && distancia <= rangoAtaque)
        {
            IniciarCombo();
        }
    }

    public void OnSpawnFromPool()
    {
        temporizadorAtaque = 0.5f; 
        if (enemigo != null) enemigo.estaAtacando = false;
        StopAllCoroutines();
    }

    public void OnDespawnToPool()
    {
        StopAllCoroutines();
    }

    // ===================== LÓGICA DE COMBO =====================

    void IniciarCombo()
    {
        StopAllCoroutines();
        enemigo.estaAtacando = true;
        StartCoroutine(RutinaCombo());
    }

    IEnumerator RutinaCombo()
    {
        for (int i = 0; i < cantidadGolpesCombo; i++)
        {
            // Validar que el enemigo siga vivo antes de cada golpe
            if (enemigo.estaMuerto) yield break;

            // 1. Disparar animación (asegúrate de que tu Animator pueda repetir este trigger rápido)
            if (animator != null) animator.SetTrigger("atacando");

            var sound = GetComponent<enemigo_sound>();
            sound?.PlayAtaque();   // 🔊 SONIDO DE ATAQUE
            // 2. Esperar el "wind-up" (tiempo hasta que el puño conecta)
            yield return new WaitForSeconds(delayImpacto);

            // 3. Validar si el jugador sigue en rango para recibir EL DAÑO
            if (objetivo != null)
            {
                float distanciaReal = Vector2.Distance(transform.position, objetivo.position);
                
                // Le damos un pequeño margen (+0.3f) por si el jugador se está moviendo
                if (distanciaReal <= rangoAtaque + 0.3f) 
                {
                    bool esUltimoGolpe = (i == cantidadGolpesCombo - 1);
                    AplicarEfectosAlJugador(esUltimoGolpe);
                }
            }

            // 4. Esperar el resto del tiempo de este golpe antes de lanzar el siguiente
            yield return new WaitForSeconds(tiempoEntreGolpes - delayImpacto);
        }

        // --- FIN DEL COMBO ---
        FinalizarAtaque();
    }

    void AplicarEfectosAlJugador(bool esDerribo)
    {
        if (objetivo == null) return;

        var vidaJugador = objetivo.GetComponentInParent<VidaJugador>();
        if (vidaJugador != null)
        {
            // Aplicamos el daño estándar
            //vidaJugador.RecibirDanio(dañoPorGolpe);

            // Si es el último golpe, lo tumbamos. Si no, solo lo aturdimos brevemente.
            //if (esDerribo)
            //{
            //    // Deberás crear este método en tu script VidaJugador (ver paso 2)
            //    vidaJugador.SufrirDerribo(tiempoDerribo);
            //}
            //else
            //{
            //    // Lo aturdimos solo por lo que dura el tiempo entre golpes para que no escape
            //    vidaJugador.SufrirStunLigero(tiempoEntreGolpes); 
            //}
        }//
    }

    public void InterrumpirAtaque()
    {
        StopAllCoroutines();
        if (enemigo != null) enemigo.estaAtacando = false;
        animator?.Play("idle"); 
    }

    void FinalizarAtaque()
    {
        enemigo.estaAtacando = false;
        temporizadorAtaque = enfriamiento; // El cooldown real empieza cuando termina el combo
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}