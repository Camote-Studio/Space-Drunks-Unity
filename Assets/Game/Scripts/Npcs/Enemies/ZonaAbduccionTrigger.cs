using UnityEngine;

public class ZonaAbduccionGato : MonoBehaviour
{
    [Header("Visual")]
    public Animator visualAnimator; // objeto hijo "visual"

    private enemigo_gato gato;
    private bool jugadorDentro;
    private bool laserActivo;

    // ===================== CONFIG =====================
    public void Configurar(enemigo_gato g)
    {
        gato = g;
        Resetear();
    }

    public void Resetear()
    {
        jugadorDentro = false;
        laserActivo = false;

        if (visualAnimator != null)
        {
            visualAnimator.ResetTrigger("laser_inicio");
            visualAnimator.ResetTrigger("laser_fin");
            visualAnimator.SetBool("laser_activo", false);
            visualAnimator.Play("idle");
        }

        gameObject.SetActive(false);
    }

    // ===================== CONTROL DESDE EL GATO =====================
    public void IniciarLaser()
    {
        gameObject.SetActive(true);

        if (visualAnimator == null) return;

        visualAnimator.ResetTrigger("laser_fin");
        visualAnimator.Play("idle");
        visualAnimator.SetTrigger("laser_inicio");
        visualAnimator.SetBool("laser_activo", true);

        laserActivo = true;
    }

    public void FinalizarLaser()
    {
        if (!laserActivo) return;

        laserActivo = false;

        if (visualAnimator != null)
        {
            visualAnimator.SetBool("laser_activo", false);
            visualAnimator.SetTrigger("laser_fin");
        }

        Invoke(nameof(DesactivarZona), 0.2f);
    }

    void DesactivarZona()
    {
        gameObject.SetActive(false);
        jugadorDentro = false;
    }

    // ===================== TRIGGER =====================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gato.PuedeAbducir()) return;

        estado_jugador jugador = other.GetComponent<estado_jugador>();
        if (jugador == null) return;

        if (jugadorDentro) return;
        jugadorDentro = true;

        gato.IniciarAbduccion();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        estado_jugador jugador = other.GetComponent<estado_jugador>();
        if (jugador == null) return;

        jugadorDentro = false;
        gato.FinalizarAbduccion();
    }
}
