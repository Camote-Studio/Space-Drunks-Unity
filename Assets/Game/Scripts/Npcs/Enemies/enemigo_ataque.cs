using UnityEngine;

[RequireComponent(typeof(enemigo_base))]
public class enemigo_ataque : MonoBehaviour
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
    private Animator animator;
    private Transform objetivo;

    // ===================== UNITY =====================
    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (enemigo == null || enemigo.estaMuerto) return;

        ActualizarObjetivo();
        if (objetivo == null) return;

        temporizadorAtaque -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, objetivo.position);

        if (!enemigo.estaAtacando &&
            temporizadorAtaque <= 0f &&
            distancia <= rangoAtaque)
        {
            Atacar();
        }
    }

    // ===================== ATAQUE =====================
    void Atacar()
    {
        enemigo.estaAtacando = true;
        temporizadorAtaque = enfriamiento;

        if (animator != null)
            animator.SetTrigger("atacar");

        Invoke(nameof(AplicarDaño), delayImpacto);
        Invoke(nameof(FinAtaque), duracionAnimacion);
    }

    void AplicarDaño()
    {
        if (enemigo.estaMuerto || objetivo == null) return;

        float distancia = Vector2.Distance(transform.position, objetivo.position);
        if (distancia > rangoAtaque + 0.5f) return;

        VidaJugador vida = objetivo.GetComponentInParent<VidaJugador>();
        if (vida != null)
            vida.RecibirDanio(daño);
    }

    void FinAtaque()
    {
        enemigo.estaAtacando = false;
    }

    // ===================== OBJETIVO =====================
    void ActualizarObjetivo()
    {
        Transform masCercano = null;
        float menorDistancia = Mathf.Infinity;

        Buscar("Player", ref masCercano, ref menorDistancia);
        Buscar("Player_2", ref masCercano, ref menorDistancia);

        objetivo = masCercano;
    }

    void Buscar(string tag, ref Transform cercano, ref float distMin)
    {
        GameObject[] objs;
        try { objs = GameObject.FindGameObjectsWithTag(tag); }
        catch { return; }

        foreach (GameObject o in objs)
        {
            float d = Vector2.Distance(transform.position, o.transform.position);
            if (d < distMin)
            {
                distMin = d;
                cercano = o.transform;
            }
        }
    }

    // ===================== DEBUG =====================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
