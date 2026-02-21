using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalNivel : MonoBehaviour
{
    [Header("Validación de Zonas")]
    [Tooltip("Arrastra aquí los 4 objetos WaveManager de tus zonas")]
    public WaveManager[] todasLasZonas; 

    [Header("Transición")]
    [Tooltip("El nombre exacto de la escena del siguiente nivel")]
    public string nombreSiguienteNivel;

    [Header("Feedback Visual (Opcional)")]
    [Tooltip("Texto en la UI que diga 'Aún quedan enemigos'")]
    public GameObject mensajeBloqueado; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificamos que sea el jugador quien toca el portal
        if (!other.CompareTag("Player")) return;

        // 2. Comprobamos el estado de todas las zonas
        if (EstanTodasLasZonasCompletadas())
        {
            Debug.Log("¡Todas las zonas despejadas! Viajando al siguiente nivel...");
            AvanzarDeNivel();
        }
        else
        {
            Debug.Log("Acceso denegado: Aún quedan zonas sin completar.");
            MostrarMensajeBloqueo();
        }
    }

    // ==========================================
    // LÓGICA DE VERIFICACIÓN
    // ==========================================
    private bool EstanTodasLasZonasCompletadas()
    {
        // Si por error la lista está vacía, evitamos bugs y permitimos pasar
        if (todasLasZonas.Length == 0) 
        {
            Debug.LogWarning("El portal no tiene zonas asignadas. Pasando de nivel automáticamente.");
            return true;
        }

        // Revisamos una por una
        foreach (WaveManager zona in todasLasZonas)
        {
            // Recuerda: en tu script WaveManager, "nivelCompletado" significa que esa zona terminó
            if (!zona.nivelCompletado) 
            {
                return false; // Encontramos al menos una zona incompleta. El portal se bloquea.
            }
        }

        return true; // Si el bucle termina, todas las zonas están en true.
    }

    // ==========================================
    // ACCIONES
    // ==========================================
    private void AvanzarDeNivel()
    {
        // Aquí podrías agregar una animación de fundido a negro (Fade Out) en el futuro
        
        if (!string.IsNullOrEmpty(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
        else
        {
            Debug.LogError("No has escrito el nombre del siguiente nivel en el Inspector del Portal.");
        }
    }

    private void MostrarMensajeBloqueo()
    {
        if (mensajeBloqueado != null)
        {
            mensajeBloqueado.SetActive(true);
            
            // Oculta el mensaje automáticamente después de 2 segundos
            CancelInvoke(nameof(OcultarMensaje)); 
            Invoke(nameof(OcultarMensaje), 2f);
        }
    }

    private void OcultarMensaje()
    {
        if (mensajeBloqueado != null)
        {
            mensajeBloqueado.SetActive(false);
        }
    }
}