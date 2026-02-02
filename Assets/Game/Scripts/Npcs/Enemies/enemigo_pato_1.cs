using UnityEngine;
using System.Collections;

public class enemigo_pato_1 : enemigo_base  
{
    [Header("Combo (solo daño)")]
    public int comboActual = 0;
    public float dañoExtraPorCombo = 10f;

    [Header("Fatality")]
    [Tooltip("Duración REAL de la animación fatality")]
    public float duracionFatalityAnim = 1.4f;

    [Header("Retroceso")]
    public float fuerzaRetroceso = 4f;
    public float tiempoRetroceso = 0.08f;

    [Header("Stun")]
    public float tiempoStun = 0.25f;

    [Header("Defensas")]
    [Range(0f, 1f)] public float probDefensa1 = 0.3f;
    [Range(0f, 1f)] public float probDefensa2 = 0.7f;

    // ===================== ESTADOS =====================
    private bool enRetroceso;
    private bool enStun;
    private bool fatalityEjecutada;

    // ===================== MOVIMIENTO =====================
    private Vector2 velocidadRetroceso;

    // Última posición válida del golpe
    private Vector2 ultimaPosicionGolpe;

    private enemigo_animacion anim;
    private Collider2D colLocal;

    // ===================== UNITY =====================

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<enemigo_animacion>();

        colLocal = GetComponent<Collider2D>();
    }

    protected override void Update()
    {
        if (estaMuerto || fatalityEjecutada)
            return;

        if (enRetroceso)
        {
            transform.position += (Vector3)(velocidadRetroceso * Time.deltaTime);
            return;
        }

        if (enStun)
            return;

        base.Update();
    }


    // ===================== POOL =====================

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool(); // Reinicio general (vida, navmesh, anim)

        // Reset específico del pato
        comboActual = 0;

        enRetroceso = false;
        enStun = false;
        fatalityEjecutada = false;

        velocidadRetroceso = Vector2.zero;

        if (colLocal != null)
            colLocal.enabled = true;
    }

    // ===================== COMBATE =====================

    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || fatalityEjecutada)
            return;

        // Guardar posición del golpe (aunque el player muera luego)
        if (objetivo != null)
            ultimaPosicionGolpe = objetivo.position;
        else
            ultimaPosicionGolpe = transform.position;

        comboActual = enStun ? comboActual + 1 : 1;
        float dañoFinal = cantidad + (comboActual - 1) * dañoExtraPorCombo;

        base.RecibirDaño(dañoFinal);

        if (vidaActual > 0)
            ReaccionarAlGolpe();
    }

    // ===================== RETROCESO =====================
    void ReaccionarAlGolpe()
    {
        if (estaMuerto || fatalityEjecutada) return;

        // Defensa aleatoria
        anim?.PlayTrigger(Random.value <= probDefensa1 ? "defensa_1" : "defensa_2");


        // Retroceso
        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        velocidadRetroceso = dir * fuerzaRetroceso;

        enRetroceso = true;
        Invoke(nameof(FinRetroceso), tiempoRetroceso);
    }

    void FinRetroceso()
    {
        enRetroceso = false;
        velocidadRetroceso = Vector2.zero;

        enStun = true;
        Invoke(nameof(FinStun), tiempoStun);
    }

    void FinStun()
    {
        enStun = false;
        comboActual = 0;
    }


    // ===================== FATALITY =====================

    IEnumerator EjecutarFatality()
    {
        CancelInvoke();

        estaMuerto = true;
        fatalityEjecutada = true;

        enRetroceso = false;
        enStun = false;


        if (colLocal != null)
            colLocal.enabled = false;

        // Dirección del fatality
        bool fatalityDerecha = objetivo.position.x < transform.position.x;

        if (fatalityDerecha)
            anim?.PlayTrigger("fatality_izquierda");
        else
            anim?.PlayTrigger("fatality_derecha");

        yield return new WaitForSeconds(duracionFatalityAnim);

        MorirFinal();
    }

    void MorirFinal()
    {
        base.Morir();
        PoolManager.Instance.ReturnToPool(enemyPoolTag, gameObject);
    }
}
