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

    // Estados
    private bool enRetroceso;
    private bool enStun;
    private bool fatalityEjecutada;

    // Movimiento
    private Vector2 velocidadRetroceso;

    private enemigo_animacion anim;
    private Collider2D col;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<enemigo_animacion>(); // por si está en Visual
        col = GetComponent<Collider2D>();
    }

    protected override void Update()
    {
        if (fatalityEjecutada) return;

        if (enRetroceso)
        {
            transform.position += (Vector3)(velocidadRetroceso * Time.deltaTime);
            return;
        }

        if (enStun) return;

        base.Update();
    }
    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || fatalityEjecutada) return;

        comboActual = enStun ? comboActual + 1 : 1;
        float dañoFinal = cantidad + (comboActual - 1) * dañoExtraPorCombo;

        base.RecibirDaño(dañoFinal);

        if (vidaActual > 0)
            ReaccionarAlGolpe();
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

    // 🔥 FATALITY SIN EVENTOS
    IEnumerator EjecutarFatality()
    {
        CancelInvoke();

        estaMuerto = true;
        fatalityEjecutada = true;

        enRetroceso = false;
        enStun = false;

        if (col) col.enabled = false;

        bool fatalityDerecha = objetivo.position.x < transform.position.x;
        anim?.PlayTrigger(
            fatalityDerecha ? "fatality_izquierda" : "fatality_derecha"
        );

        yield return new WaitForSeconds(duracionFatalityAnim);
        Destroy(gameObject);
    }


    protected override void Morir()
    {
        if (fatalityEjecutada) return;
        StartCoroutine(EjecutarFatality());
    }

    void ReaccionarAlGolpe()
    {
        if (estaMuerto || fatalityEjecutada) return;

        anim?.PlayTrigger(Random.value <= probDefensa1 ? "defensa_1" : "defensa_2");

        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        velocidadRetroceso = dir * fuerzaRetroceso;

        enRetroceso = true;
        Invoke(nameof(FinRetroceso), tiempoRetroceso);
    }


}
