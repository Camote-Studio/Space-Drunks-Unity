using UnityEngine;

public class enemigo_armadillo : enemigo_base
{
    private enum EstadoArmadillo
    {
        MoviendoAPosicion,
        Disparando,
        Descansando,
        Retirandose,
        Muerto
    }

    private EstadoArmadillo estadoActual;

    private enemigo_sound sound;

    [Header("Movimiento")]
    [SerializeField] float velocidadMovimiento = 6f;
    private Vector2 posicionObjetivo;

    [Header("Disparo")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform puntoDisparo;
    [SerializeField] float velocidadBala = 9f;
    [SerializeField] float tiempoEntreDisparos = 0.8f;

    [Header("Ciclo de ataque")]
    [SerializeField] float duracionAtaque = 2f;
    [SerializeField] float duracionDescanso = 3.5f;

    [Header("Tiempo en pantalla")]
    [SerializeField] float tiempoEnPantalla = 10f;

    float timerDisparo;
    float timerEstado;
    float timerVida;

    EnemySpawner spawner;
    int carril;

    protected override void Awake()
    {
        base.Awake();
        sound = GetComponent<enemigo_sound>();
    }

    protected override void Update()
    {
        base.Update();

        if (estaMuerto)
        {
            estadoActual = EstadoArmadillo.Muerto;
            return;
        }

        if (objetivo == null) return;

        MirarAlJugador();

        if (estadoActual != EstadoArmadillo.MoviendoAPosicion && estadoActual != EstadoArmadillo.Retirandose)
        {
            timerVida -= Time.deltaTime;

            if (timerVida <= 0f)
            {
                estadoActual = EstadoArmadillo.Retirandose;
            }
        }

        switch (estadoActual)
        {
            case EstadoArmadillo.MoviendoAPosicion:
                MoverAPosicion();
                break;

            case EstadoArmadillo.Disparando:
                EjecutarDisparo();
                break;

            case EstadoArmadillo.Descansando:
                EjecutarDescanso();
                break;

            case EstadoArmadillo.Retirandose:
                Retirarse();
                break;
        }
    }

    //==========================================================
    // POSICIONAMIENTO
    //==========================================================

    public void SetPosition(Vector2 pos)
    {
        posicionObjetivo = pos;
        estadoActual = EstadoArmadillo.MoviendoAPosicion;
    }

    void MoverAPosicion()
    {
        Vector2 dir = (posicionObjetivo - (Vector2)transform.position).normalized;

        transform.position += (Vector3)(dir * velocidadMovimiento * Time.deltaTime);

        float dist = Vector2.Distance(transform.position, posicionObjetivo);

        if (dist < 0.15f)
        {
            estadoActual = EstadoArmadillo.Disparando;
            timerEstado = duracionAtaque;
            timerDisparo = 0f;
            timerVida = tiempoEnPantalla;
        }
    }

    //==========================================================
    // DISPARO
    //==========================================================

    void EjecutarDisparo()
    {
        timerEstado -= Time.deltaTime;
        timerDisparo -= Time.deltaTime;

        if (timerDisparo <= 0f)
        {
            DisparoReal();
            timerDisparo = tiempoEntreDisparos;
        }

        if (timerEstado <= 0f)
        {
            estadoActual = EstadoArmadillo.Descansando;
            timerEstado = duracionDescanso;
        }
    }

    void DisparoReal()
    {
        if (objetivo == null || bulletPrefab == null) return;

        sound?.PlayAtaque();

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;

        GameObject bala = Instantiate(bulletPrefab, origen, Quaternion.identity);

        Vector2 dir = (objetivo.position - origen).normalized;

        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bala.transform.rotation = Quaternion.Euler(0, 0, angulo);

        Rigidbody2D rb = bala.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = dir * velocidadBala;
        }
    }

    //==========================================================
    // DESCANSO
    //==========================================================

    void EjecutarDescanso()
    {
        timerEstado -= Time.deltaTime;

        if (timerEstado <= 0f)
        {
            estadoActual = EstadoArmadillo.Disparando;
            timerEstado = duracionAtaque;
            timerDisparo = 0f;
        }
    }

    //==========================================================
    // RETIRADA
    //==========================================================

    void Retirarse()
    {
        transform.position += Vector3.right * velocidadMovimiento * Time.deltaTime;

        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;
        float camX = Camera.main.transform.position.x;

        if (transform.position.x > camX + camWidth + 2f)
        {
            spawner?.LiberarCarril(carril);
            gameObject.SetActive(false);
        }
    }

    //==========================================================
    // UTILIDADES
    //==========================================================

    void MirarAlJugador()
    {
        if (spriteRenderer != null && objetivo != null)
        {
            spriteRenderer.flipX = objetivo.position.x > transform.position.x;
        }
    }

    //==========================================================
    // CARRIL SPAWNER
    //==========================================================

    public void SetCarrilSpawner(EnemySpawner s, int c)
    {
        spawner = s;
        carril = c;
    }

    //==========================================================
    // DAÑO
    //==========================================================

    public override void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;

        sound?.PlayDaño();

        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            sound?.PlayMuerte();
            spawner?.LiberarCarril(carril);
            Morir();
        }
    }
}