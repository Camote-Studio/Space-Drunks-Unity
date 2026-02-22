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

    void Start()
    {
        index = 0;
        ActualizarColores();
    }

    void Update()
    {
        if (opciones == null || opciones.Length == 0) return;

        // Permite usar flechas Arriba/Abajo o Izquierda/Derecha
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            index = (index + 1) % opciones.Length;
            ActualizarColores();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            index--;
            if (index < 0) index = opciones.Length - 1;
            ActualizarColores();
        }

        // Presionar Enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (opciones[index] != null) 
            {
                opciones[index].onClick.Invoke();
            }
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