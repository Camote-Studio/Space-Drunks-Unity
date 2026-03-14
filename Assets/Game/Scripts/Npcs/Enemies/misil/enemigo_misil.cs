using UnityEngine;

public class enemigo_misil : MonoBehaviour
{
    private enum EstadoMisil
    {
        Flotando,
        Disparado
    }

    private EstadoMisil estadoActual;

    [Header("Referencias")]
    [SerializeField] Transform jugador;
    [SerializeField] Animator animator;

    [Header("Movimiento")]
    [SerializeField] float velocidadFlotando = 1.5f;
    [SerializeField] float velocidadAtaque = 20f;

    [Header("Tiempo")]
    [SerializeField] float tiempoFlotandoMin = 3f;
    [SerializeField] float tiempoFlotandoMax = 5f;

    private float timerFlotando;

    private Vector2 direccionFlotando;
    private Vector2 direccionAtaque;

    private Camera cam;

    private float screenLeft, screenRight, screenTop, screenBottom;

    void Awake()
    {
        cam = Camera.main;

        estadoActual = EstadoMisil.Flotando;

        timerFlotando = Random.Range(tiempoFlotandoMin, tiempoFlotandoMax);

        CalcularBordesCamara();

        // aparecer fuera de cámara
        transform.position = ElegirBordeAleatorio(2f);

        // dirección inicial hacia el centro
        Vector3 camPos = cam.transform.position;
        direccionFlotando = (camPos - transform.position).normalized;

        direccionFlotando += Random.insideUnitCircle * 0.2f;
        direccionFlotando.Normalize();
    }

    void Update()
    {
        // buscar jugador si no existe
        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                jugador = p.transform;
        }

        switch (estadoActual)
        {
            case EstadoMisil.Flotando:
                Flotar();
                break;

            case EstadoMisil.Disparado:
                Disparar();
                VerificarFueraDePantalla();
                break;
        }
    }

    // =========================
    // FLOTACIÓN
    // =========================

    void Flotar()
    {
        transform.position += (Vector3)(direccionFlotando * velocidadFlotando * Time.deltaTime);

        // pequeña variación para que no se vea robótico
        direccionFlotando += Random.insideUnitCircle * 0.02f;
        direccionFlotando.Normalize();

        timerFlotando -= Time.deltaTime;

        if (timerFlotando <= 0f)
        {
            if (jugador != null)
                direccionAtaque = ((Vector2)jugador.position - (Vector2)transform.position).normalized;
            else
                direccionAtaque = direccionFlotando;

            estadoActual = EstadoMisil.Disparado;

            if (animator != null)
                animator.SetBool("ataque", true);
        }
    }

    // =========================
    // DISPARO
    // =========================

    void Disparar()
    {
        float angulo = Mathf.Atan2(direccionAtaque.y, direccionAtaque.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        transform.position += (Vector3)(direccionAtaque * velocidadAtaque * Time.deltaTime);
    }

    // =========================
    // DESTRUIR AL SALIR
    // =========================

    void VerificarFueraDePantalla()
    {
        Vector3 pos = transform.position;

        if (pos.x < screenLeft - 3f ||
            pos.x > screenRight + 3f ||
            pos.y < screenBottom - 3f ||
            pos.y > screenTop + 3f)
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // RECIBIR JUGADOR DESDE SPAWNER
    // =========================

    public void SetJugador(Transform j)
    {
        jugador = j;
    }

    // =========================
    // CALCULAR BORDES CÁMARA
    // =========================

    void CalcularBordesCamara()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        screenLeft = camPos.x - camWidth;
        screenRight = camPos.x + camWidth;
        screenTop = camPos.y + camHeight;
        screenBottom = camPos.y - camHeight;
    }

    // =========================
    // SPAWN FUERA DE CÁMARA
    // =========================

    Vector2 ElegirBordeAleatorio(float distancia)
    {
        int lado = Random.Range(0, 4);

        switch (lado)
        {
            case 0:
                return new Vector2(screenLeft - distancia, Random.Range(screenBottom, screenTop));

            case 1:
                return new Vector2(screenRight + distancia, Random.Range(screenBottom, screenTop));

            case 2:
                return new Vector2(Random.Range(screenLeft, screenRight), screenTop + distancia);

            case 3:
                return new Vector2(Random.Range(screenLeft, screenRight), screenBottom - distancia);
        }

        return Vector2.zero;
    }
}