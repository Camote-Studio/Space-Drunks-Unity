using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.InputSystem.Utilities;

public class PantallaInicio : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe exactamente el nombre de tu escena del menú principal")]
    public string nombreEscenaMenu = "Menu";

    [Header("Configuración de Transición")]
    [Tooltip("Elige el efecto visual para pasar al menú")]
    public TransitionType tipoDeTransicion = TransitionType.Fade;

    private System.IDisposable escuchadorBotones;
    
    // Variable de seguridad para evitar que el jugador presione 5 botones a la vez y bugee la transición
    private bool cambiandoDeEscena = false; 

    void Start()
    {
        // Esta línea mágica le dice a Unity: "Avísame si presionan cualquier cosa"
        escuchadorBotones = InputSystem.onAnyButtonPress.Call(control => CargarMenu());
    }

    void CargarMenu()
    {
        if (cambiandoDeEscena) return; // Evitamos múltiples llamadas
        cambiandoDeEscena = true; // Bloqueamos futuras llamadas
        
        Debug.Log("¡Botón presionado! Viajando al menú con transición...");
        
        // 1. Nos desconectamos del evento de los botones
        escuchadorBotones?.Dispose();

        // 2. Llamamos a tu Transition Manager
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadScene(nombreEscenaMenu, tipoDeTransicion);
        }
        else
        {
            Debug.LogError("No hay ningún TransitionManager en la escena. Asegúrate de poner tu prefab del TransitionManager aquí.");
        }
    }

    private void OnDestroy()
    {
        // Medida de seguridad extra
        escuchadorBotones?.Dispose();
    }
}