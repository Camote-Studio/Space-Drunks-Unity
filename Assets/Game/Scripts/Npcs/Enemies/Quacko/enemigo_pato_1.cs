using UnityEngine;
using System.Collections;

public class enemigo_pato_1 : enemigo_base
{
    private enemigo_sound sound;

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

    private enemigo_animacion anim;
    private Collider2D colLocal;
    private enemigo_ataque sistemaAtaque;

    // ===================== UNITY =====================

    protected override void Awake()
    {
        base.Awake();

        sound = GetComponent<enemigo_sound>();
        anim = GetComponentInChildren<enemigo_animacion>();
        colLocal = GetComponent<Collider2D>();
        sistemaAtaque = GetComponent<enemigo_ataque>();
    }

    protected override void Update()
    {
        if (estaMuerto || fatalityEjecutada)
        {
            sound?.StopMovimiento();
            return;
        }

        if (enRetroceso)
        {
            sound?.StopMovimiento();
            transform.position += (Vector3)(velocidadRetroceso * Time.deltaTime);
            return;
        }

        if (enStun)
        {
            sound?.StopMovimiento();
            return;
        }

        // 🔥 Movimiento normal
        sound?.StartMovimiento();

        base.Update();
    }

    // ===================== POOL =====================

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();

        comboActual = 0;

        enRetroceso = false;
        enStun = false;
        fatalityEjecutada = false;

        velocidadRetroceso = Vector2.zero;

        if (colLocal != null)
            colLocal.enabled = true;

        sound?.StopMovimiento();
    }

    private void OnDisable()
    {
        sound?.StopMovimiento();
    }

    // ===================== COMBATE =====================

    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto || fatalityEjecutada)
            return;

        

        if (sistemaAtaque != null)
        {
            sistemaAtaque.InterrumpirAtaque();
        }

        comboActual = enStun ? comboActual + 1 : 1;
        float dañoFinal = cantidad + (comboActual - 1) * dañoExtraPorCombo;

        vidaActual -= dañoFinal;

        if (vidaActual <= 0)
        {
            StartCoroutine(EjecutarFatality());
            return;
        }
        Debug.Log("EL ENEMIGO RECIBIÓ DAÑO");
        sound?.PlayDaño(); // 🔊 SONIDO DAÑO
        anim?.PlayTrigger(Random.value <= probDefensa1 ? "defensa_1" : "defensa_2");

        if (objetivo != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
            velocidadRetroceso = dir * fuerzaRetroceso;
        }

        enRetroceso = true;

        CancelInvoke(nameof(FinRetroceso));
        CancelInvoke(nameof(FinStun));

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
        estaMuerto = true;
        fatalityEjecutada = true;

        sound?.StopMovimiento();   // 🔊 DETENER PASOS
        sound?.PlayFatality();     // 🔊 SONIDO FATALITY

        enRetroceso = false;
        enStun = false;

        if (sistemaAtaque != null)
            sistemaAtaque.InterrumpirAtaque();

        if (colLocal != null)
            colLocal.enabled = false;

        if (objetivo != null)
        {
            bool fatalityDerecha = objetivo.position.x < transform.position.x;

            if (fatalityDerecha)
                anim?.PlayTrigger("fatality_izquierda");
            else
                anim?.PlayTrigger("fatality_derecha");
        }

        yield return new WaitForSeconds(duracionFatalityAnim);

        MorirFinal();
    }

    void MorirFinal()
    {
        sound?.PlayMuerte(); // 🔊 SONIDO MUERTE

        base.Morir();
        PoolManager.Instance.ReturnToPool(enemyPoolTag, gameObject);
    }
}