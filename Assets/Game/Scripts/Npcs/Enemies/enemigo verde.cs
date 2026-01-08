using UnityEngine;

public class enemigoverde : enemigo_base
{
    private enemigo_animacion anim;

    [Header("Ataque a Distancia")]
    public GameObject balaPrefab;
    public float velocidadBala = 8f;
    public float rangoDisparo = 6f;
    public float distanciaIdeal = 4.5f;
    public float enfriamientoDisparo = 2f;
    [Header("Movimiento en Combate")]

    public float velocidadStrafe = 2f;
    public float cambioDireccionTiempo = 1.5f;

    private float temporizadorDisparo;
    private float temporizadorStrafe;
    private int direccionStrafe = 1;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<enemigo_animacion>();
        temporizadorStrafe = cambioDireccionTiempo;
    }

    protected override void Update()
    {
        if (estaMuerto || objetivo == null) return;

        temporizadorDisparo -= Time.deltaTime;
        temporizadorStrafe -= Time.deltaTime;

        if (temporizadorStrafe <= 0f)
        {
            direccionStrafe = Random.value > 0.5f ? 1 : -1;
            temporizadorStrafe = cambioDireccionTiempo;
        }

        float distancia = Vector2.Distance(transform.position, objetivo.position);
        Vector2 toTarget = objetivo.position - transform.position;
        Vector2 dir = toTarget.normalized;
        Vector2 movimiento = Vector2.zero;
        if (distancia > distanciaIdeal + 0.5f)
        {
            movimiento = dir * velocidad;
        }
        else if (distancia < distanciaIdeal - 0.5f)
        {
            movimiento = -dir * velocidad;
        }
        else
        {
            Vector2 tangente = new Vector2(-dir.y, dir.x);
            movimiento = tangente * direccionStrafe * velocidadStrafe;
        }
        velocidadActual = Vector2.Lerp(
            velocidadActual,
            movimiento,
            aceleracion * Time.deltaTime
        );
        AplicarMovimiento();
        if (sprite && Mathf.Abs(dir.x) > 0.05f)
            sprite.flipX = dir.x > 0;
        if (distancia <= rangoDisparo && temporizadorDisparo <= 0f)
        {
            Disparar();
        }
    }
    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;
        anim?.PlayTrigger("defensa_1");
        base.RecibirDaño(cantidad);
    }

    void Disparar()
    {
        if (balaPrefab == null) return;

        estaAtacando = true;
        temporizadorDisparo = enfriamientoDisparo;
        anim?.PlayTrigger("atacando");
        Invoke(nameof(DisparoReal), 0.7f);
    }
    void DisparoReal()
    {
        if (objetivo == null || estaMuerto) return;
        GameObject bala = Instantiate(
            balaPrefab,
            transform.position,
            Quaternion.identity
        );
        Vector2 dir = (objetivo.position - transform.position).normalized;
        bala.GetComponent<Rigidbody2D>()?.AddForce(
            dir * velocidadBala,
            ForceMode2D.Impulse
        );
        estaAtacando = false;
    }
}
