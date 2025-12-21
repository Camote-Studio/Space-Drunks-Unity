using UnityEngine;

public class enemigo_pato_1 : enemigo_base
{
    [Header("Retroceso")]
    public float fuerzaRetroceso = 8f;
    public float tiempoRetroceso = 0.2f;
    [Header("Defensas (solo pato_1)")]
    [Range(0f, 1f)] public float probDefensa1 = 0.3f; // 30%
    [Range(0f, 1f)] public float probDefensa2 = 0.7f; // 70%
    private bool enRetroceso;
    private Vector2 velocidadRetroceso;
    private enemigo_animacion anim;
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<enemigo_animacion>();
    }
    protected override void Update()
    {
        if (enRetroceso)
        {
            transform.position += (Vector3)(velocidadRetroceso * Time.deltaTime);
            return;
        }

        base.Update();
    }
    public override void RecibirDaño(float cantidad)
    {
        Debug.Log("enemigo muerto");

        if (estaMuerto) return;
        // 🎲 DECISIÓN EXCLUSIVA DEL PATO
        float r = Random.value;
        if (r <= probDefensa1) {

            anim?.PlayTrigger("defensa_1");
            Debug.Log("defensa_1"); }
        else { 
        anim?.PlayTrigger("defensa_2");
        Debug.Log("defensa_2");
    }
        // ❤️ daño base
        base.RecibirDaño(cantidad);

        if (estaMuerto || objetivo == null) return;

        // 💥 retroceso
        Vector2 dir = ((Vector2)transform.position - (Vector2)objetivo.position).normalized;
        velocidadRetroceso = dir * fuerzaRetroceso;
        Debug.Log("enemigo retrocede");

        enRetroceso = true;
        CancelInvoke(nameof(FinRetroceso));
        Invoke(nameof(FinRetroceso), tiempoRetroceso);
    }

    void FinRetroceso()
    {
        enRetroceso = false;
        velocidadRetroceso = Vector2.zero;
    }
}
