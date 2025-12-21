using UnityEngine;

public class enemigo_gato : enemigo_base
{
    private enemigo_animacion anim;

    [Header("Movimiento en Órbita")]
    public float distanciaMin = 10f;
    public float distanciaMax = 4.5f;
    public float velocidadOrbita = 4f;
    public float correccionRadial = 3f;
    public float cambioDireccionTiempo =5f;
    [Header("Ataque a Distancia")]
    public GameObject balaPrefab;
    public float velocidadBala = 8f;
    public float rangoDisparo = 6f;
    public float enfriamientoDisparo = 2f;
    private float temporizadorDisparo;
    private float temporizadorOrbita;
    private int direccionOrbita = 1;
    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<enemigo_animacion>();
    }
    protected override void Update()
    {
        if (estaMuerto || objetivo == null) return;
        temporizadorDisparo -= Time.deltaTime;
        temporizadorOrbita -= Time.deltaTime;
        if (temporizadorOrbita <= 0f)
        {
            direccionOrbita *= -1;
            temporizadorOrbita = cambioDireccionTiempo;
        }
        MoverHaciaObjetivo();
        Disparar();
    }
    protected override void MoverHaciaObjetivo()
    {
        Vector2 toTarget = objetivo.position - transform.position;
        float dist = toTarget.magnitude;
        Vector2 dir = toTarget.normalized;
        // Tangente (órbita)
        Vector2 tangente = new Vector2(-dir.y, dir.x) * direccionOrbita;
        // Corrección radial
        Vector2 correccion = Vector2.zero;
        if (dist > distanciaMax)
            correccion = dir * correccionRadial;
        else if (dist < distanciaMin)
            correccion = -dir * correccionRadial;
        Vector2 movimiento = (tangente * velocidadOrbita) + correccion;
        velocidadActual = Vector2.Lerp(
            velocidadActual,
            movimiento,
            aceleracion * Time.deltaTime
        );
        AplicarMovimiento();
        // 🔁 Flip
        if (sprite && Mathf.Abs(dir.x) > 0.05f)
            sprite.flipX = dir.x > 0;
    }
    void Disparar()
    {
        float distancia = Vector2.Distance(transform.position, objetivo.position);
        if (distancia > rangoDisparo) return;
        if (temporizadorDisparo > 0f) return;
        if (balaPrefab == null) return;
        temporizadorDisparo = enfriamientoDisparo;
        anim?.PlayTrigger("atacando");
        Invoke(nameof(DisparoReal), 0.7f);
    }
    void DisparoReal()
    {
        if (objetivo == null) return;
        GameObject bala = Instantiate(balaPrefab, transform.position, Quaternion.identity);
        Vector2 dir = (objetivo.position - transform.position).normalized;
        bala.GetComponent<Rigidbody2D>()?.AddForce(
            dir * velocidadBala,
            ForceMode2D.Impulse
        );
    }
}
