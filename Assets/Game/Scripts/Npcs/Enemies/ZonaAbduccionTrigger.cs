using UnityEngine;

public class ZonaAbduccionGato : MonoBehaviour
{
    private enemigo_gato gato;

    [Header("Visual Láser")]
    public Animator visualAnimator;

    private bool laserActivo = false;

    private void Awake()
    {
        gato = GetComponentInParent<enemigo_gato>();

        if (visualAnimator == null)
        {
            Debug.LogWarning("⚠️ Falta asignar Animator del Visual");
        }
    }

    // =====================================================
    // 👽 CUANDO EL JUGADOR ENTRA A LA ZONA
    // =====================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ej = other.GetComponent<estado_jugador>();
        if (ej == null) return;

        // 🔒 El láser YA DEBE ESTAR ENCENDIDO
        if (gato.estadoActual != enemigo_gato.EstadoGato.IntentandoAbducir)
            return;

        gato.ActivarAbduccion(ej);
    }

    // =====================================================
    // 🎬 ANIMACIONES
    // =====================================================
    public void IniciarLaser()
    {
        if (laserActivo) return;

        laserActivo = true;
        visualAnimator.ResetTrigger("laser_fin");
        visualAnimator.SetTrigger("laser_inicio");

        Debug.Log("👽 Láser iniciado");
    }

    public void MantenerLaser()
    {
        visualAnimator.SetBool("laser_activo", true);
    }

    public void DetenerLaser()
    {
        if (!laserActivo) return;

        laserActivo = false;
        visualAnimator.SetBool("laser_activo", false);
        visualAnimator.SetTrigger("laser_fin");

        Debug.Log("❌ Láser detenido");
    }
}

