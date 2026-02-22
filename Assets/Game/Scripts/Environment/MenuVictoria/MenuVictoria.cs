using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuVictoria : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Arrastra aquí tus botones (ej. Volver al Menú)")]
    public Button[] opciones;
    
    public Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    public Color colorSeleccionado = new Color(1f, 0.83f, 0f);

    [Header("Escena")]
    public string nombreEscenaMenu = "Menu";

    private int index = 0;
    private bool inputEnEspera = false;

    void Start()
    {
        index = 0;
        ActualizarColores();
    }

    void Update()
    {
        if (opciones == null || opciones.Length == 0) return;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        bool confirmar = Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);

        // Movimiento Hacia Adelante (Abajo o Derecha)
        if (v < -0.5f || h > 0.5f || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!inputEnEspera)
            {
                index = (index + 1) % opciones.Length;
                ActualizarColores();
                inputEnEspera = true;
            }
        }
        // Movimiento Hacia Atrás (Arriba o Izquierda)
        else if (v > 0.5f || h < -0.5f || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!inputEnEspera)
            {
                index--;
                if (index < 0) index = opciones.Length - 1;
                ActualizarColores();
                inputEnEspera = true;
            }
        }

        // Aceptar
        if (confirmar)
        {
            if (opciones[index] != null) opciones[index].onClick.Invoke();
        }

        // Liberar seguro al soltar la palanca
        if (Mathf.Abs(v) < 0.1f && Mathf.Abs(h) < 0.1f)
        {
            inputEnEspera = false;
        }
    }

    void ActualizarColores()
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i] == null) continue;

            Image imagenBoton = opciones[i].GetComponent<Image>();
            if (imagenBoton != null)
            {
                imagenBoton.color = (i == index) ? colorSeleccionado : colorNormal;
            }
        }
    }

    // ==========================================
    // FUNCIÓN PARA EL BOTÓN
    // ==========================================
    public void IrAlMenu()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadScene(nombreEscenaMenu, TransitionType.Fade);
        }
        else
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
    }
}