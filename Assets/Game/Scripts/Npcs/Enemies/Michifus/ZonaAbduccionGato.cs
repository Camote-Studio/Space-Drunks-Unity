using UnityEngine;
using System.Collections;

public class ZonaAbduccionGato : MonoBehaviour
{
    [Header("Configuración de Forcejeo")]
    public float velocidadForcejeo = 1.5f;

    [Header("Salida del jugador (Mareo)")]
    public float duracionSalida = 1.5f;
    public float velocidadGiro = 720f;

    [Header("Enfriamiento (Cooldown)")]
    public float cooldownGato = 12f;
    public float inmunidadGlobalJugador = 2.5f;

    /* == SECCIÓN COMENTADA PARA EL FUTURO SISTEMA DE VIDA ==
    [Header("Daño de Abducción")]
    public float dañoPorSegundo = 10f; 
    */

    private enemigo_gato gato;
    private Transform jugador;
    private Rigidbody2D rbJugador;
    private MonoBehaviour movimientoJugador; 
    
    // Capturaremos tu PolygonCollider
    private Collider2D miCollider; 

    private bool enProceso;
    private float proximoUsoPermitido = 0f; 
    private static float finInmunidadGlobal = 0f; 

    private void Awake()
    {
        miCollider = GetComponent<Collider2D>();
    }

    public void Configurar(enemigo_gato g)
    {
        gato = g;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (enProceso) return;
        
        if (Time.time < proximoUsoPermitido) return; 
        if (Time.time < finInmunidadGlobal) return;  

        jugador = other.transform;
        rbJugador = jugador.GetComponent<Rigidbody2D>();
        movimientoJugador = jugador.GetComponent("PlayerMovement") as MonoBehaviour; 

        if (rbJugador == null || movimientoJugador == null) return;

        StartCoroutine(SecuenciaAbduccion());
    }

    private IEnumerator SecuenciaAbduccion()
    {
        enProceso = true;
        finInmunidadGlobal = Time.time + 999f; 

        // Bloqueamos al jugador instantáneamente (ya tocó la luz amarilla)
        movimientoJugador.enabled = false;
        rbJugador.linearVelocity = Vector2.zero;

        // ==========================================================
        // 1. FORCEJEO (Dictado por la animación)
        // ==========================================================
        while (miCollider.enabled)
        {
            if (gato.estaMuerto) { LiberarJugadorEmergencia(); yield break; }

            /* == LÓGICA DE DAÑO COMENTADA PARA EL FUTURO ==
            var scriptVida = jugador.GetComponent<ScriptVidaJugador>(); 
            if (scriptVida != null)
            {
                scriptVida.RecibirDaño(dañoPorSegundo * Time.deltaTime);
            }
            */

            float movX = Input.GetAxisRaw("Horizontal");
            float movY = Input.GetAxisRaw("Vertical");
            Vector2 direccionForcejeo = new Vector2(movX, movY).normalized;

            Vector2 nuevaPosicion = rbJugador.position + (direccionForcejeo * velocidadForcejeo * Time.deltaTime);
            rbJugador.MovePosition(nuevaPosicion);

            yield return null; 
        }

        // ==========================================================
        // 2. MAREO (El rayo amarillo ya se apagó en la animación)
        // ==========================================================
        float tiempoActual = 0f;
        while (tiempoActual < duracionSalida)
        {
            if (gato.estaMuerto) { LiberarJugadorEmergencia(); yield break; }

            jugador.Rotate(Vector3.forward, velocidadGiro * Time.deltaTime);
            tiempoActual += Time.deltaTime;
            yield return null;
        }

        LiberarJugadorEmergencia();
    }

    public void LiberarJugadorEmergencia()
    {
        if (jugador != null)
        {
            if (rbJugador != null) rbJugador.linearVelocity = Vector2.zero;
            if (movimientoJugador != null) movimientoJugador.enabled = true;
            jugador.rotation = Quaternion.identity; 
        }

        proximoUsoPermitido = Time.time + cooldownGato; 
        finInmunidadGlobal = Time.time + inmunidadGlobalJugador; 

        enProceso = false;
        jugador = null;
    }

    private void OnDisable() { if (enProceso) LiberarJugadorEmergencia(); }
}