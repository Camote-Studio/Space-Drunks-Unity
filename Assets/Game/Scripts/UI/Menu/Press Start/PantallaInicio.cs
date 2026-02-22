using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 
using UnityEngine.InputSystem.Utilities;

public class PantallaInicio : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Escribe exactamente el nombre de tu escena del menú principal")]
    public string nombreEscenaMenu = "Menu";

    // Guardamos la conexión del evento para poder "apagarla" al cambiar de escena
    private System.IDisposable escuchadorBotones;

    void Start()
    {
        // Esta línea mágica le dice a Unity: "Avísame si presionan cualquier cosa"
        escuchadorBotones = InputSystem.onAnyButtonPress.Call(control => CargarMenu());
    }

    void CargarMenu()
    {
        Debug.Log("¡Botón presionado! Viajando al menú...");
        
        // 1. Nos desconectamos del evento para evitar errores de memoria
        escuchadorBotones?.Dispose();

        // 2. Cargamos la escena
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    private void OnDestroy()
    {
        // Medida de seguridad extra por si el objeto se destruye de otra forma
        escuchadorBotones?.Dispose();
    }
}