using UnityEngine;
using System.Collections;

public class ZonaAbduccionGato : MonoBehaviour
{
    [Header("Salida del jugador")]
    public float duracionSalida = 1.5f;
    public float alturaSalida = 0.8f;
    public float velocidadGiro = 360f;

    private enemigo_gato gato;

    private Transform jugador;
    private Rigidbody2D rbJugador;
    private PlayerMovement movimientoJugador;

    private bool enProceso;

    // =============================
    // CONFIGURACIÓN
    // =============================
    public void Configurar(enemigo_gato g)
    {
        gato = g;
    }

    // =============================
    // DETECCIÓN
    // =============================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (enProceso) return;
        if (!gato.PuedeAbducir()) return;

        jugador = other.transform;
        rbJugador = jugador.GetComponent<Rigidbody2D>();
        movimientoJugador = jugador.GetComponent<PlayerMovement>();

        if (rbJugador == null || movimientoJugador == null) return;

        StartCoroutine(SecuenciaAbduccion());
    }

    // =============================
    // SECUENCIA COMPLETA
    // =============================
    private IEnumerator SecuenciaAbduccion()
    {
        enProceso = true;

        // 1. Bloquear jugador (sin moverlo)
        movimientoJugador.enabled = false;
        rbJugador.linearVelocity = Vector2.zero;

        // 2. Iniciar animación del gato
        gato.IniciarAbduccion();

        // 3. Esperar a que termine la animación
        // IMPORTANTE: este tiempo debe coincidir con la animación
        yield return new WaitForSeconds(1f);

        gato.FinalizarAbduccion();

        // 4. Elevar y girar al jugador
        yield return StartCoroutine(SalidaGirando());

        // 5. Restaurar jugador
        rbJugador.linearVelocity = Vector2.zero;
        rbJugador.gravityScale = 1f;
        jugador.rotation = Quaternion.identity;
        movimientoJugador.enabled = true;

        enProceso = false;
        jugador = null;
    }

    // =============================
    // SALIDA VISUAL
    // =============================
    private IEnumerator SalidaGirando()
    {
        float tiempo = 0f;

        rbJugador.gravityScale = 0f;

        Vector3 posicionInicial = jugador.position;
        Vector3 posicionFinal = posicionInicial + Vector3.up * alturaSalida;

        while (tiempo < duracionSalida)
        {
            float t = tiempo / duracionSalida;

            // Movimiento suave hacia arriba
            jugador.position = Vector3.Lerp(posicionInicial, posicionFinal, t);

            // Giro visual
            jugador.Rotate(Vector3.forward, velocidadGiro * Time.deltaTime);

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurar posición final exacta
        jugador.position = posicionFinal;
    }

}
