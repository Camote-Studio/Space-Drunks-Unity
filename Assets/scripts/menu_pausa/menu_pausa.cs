using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuButtons : MonoBehaviour
{
    [Header("UI Principal")]
    public GameObject menuPause;
    public Selectable[] opciones; // <-- AHORA ACEPTA BOTONES Y SLIDERS

    [Header("UI Opciones")]
    public GameObject panelOpciones;
    public Selectable[] elementosOpciones; // <-- AQUÍ PONDRÁS TUS SLIDERS Y TU BOTÓN BACK

    [Header("UI Tienda")]
    public GameObject panelTienda;

    [Header("Colores")]
    public Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    public Color colorSeleccionado = new Color(1f, 0.83f, 0f);
    
    [Header("Escenas")]
    public string nombreEscenaMenu = "Menu";

    private bool juegoPausado = false;
    private int index = 0;
    private int indexOpciones = 0;
    private bool inputEnEspera = false; // 


    void Start()
    {
        menuPause.SetActive(false);
        if (panelTienda != null) panelTienda.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        
        index = 0;
        indexOpciones = 0;
        ActualizarColores(opciones, index);
    }

    void Update()
    {
        // 1. RED DE CAPTURA PARA EL BOTÓN START / ESCAPE
        bool presionaPausa = Input.GetKeyDown(KeyCode.Escape) || 
                             Input.GetKeyDown(KeyCode.JoystickButton7);

        if (presionaPausa)
        {
            if (juegoPausado) Reanudar();
            else Pausar();
        }

        if (!juegoPausado) return;

        // ==========================================
        // ESTADO 1: NAVEGANDO EL MENÚ PRINCIPAL
        // ==========================================
        if (menuPause.activeInHierarchy)
        {
            ProcesarNavegacion(opciones, ref index);
        }
        // ==========================================
        // ESTADO 2: NAVEGANDO EL PANEL DE OPCIONES
        // ==========================================
        else if (panelOpciones != null && panelOpciones.activeInHierarchy)
        {
            ProcesarNavegacion(elementosOpciones, ref indexOpciones);
        }
    }

    // ==========================================
    // LÓGICA DE NAVEGACIÓN (BOTONES Y SLIDERS)
    // ==========================================
void ProcesarNavegacion(Selectable[] lista, ref int indiceActual)
    {
        if (lista == null || lista.Length == 0) return;

        // Leemos tanto el teclado como el mando de forma universal
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        
        // "Submit" detecta Enter, Espacio y el botón A (Xbox) / X (PlayStation)
        bool confirmar = Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);

        // ==========================================
        // 1. MOVIMIENTO VERTICAL (Subir y Bajar)
        // ==========================================
        if (v < -0.5f || Input.GetKeyDown(KeyCode.DownArrow)) // ABAJO
        {
            if (!inputEnEspera)
            {
                indiceActual = (indiceActual + 1) % lista.Length;
                SeleccionarActual(lista, indiceActual);
                inputEnEspera = true; // Ponemos el seguro
            }
        }
        else if (v > 0.5f || Input.GetKeyDown(KeyCode.UpArrow)) // ARRIBA
        {
            if (!inputEnEspera)
            {
                indiceActual--;
                if (indiceActual < 0) indiceActual = lista.Length - 1;
                SeleccionarActual(lista, indiceActual);
                inputEnEspera = true; // Ponemos el seguro
            }
        }

        // ==========================================
        // 2. INTERACCIÓN (Sliders y Botones)
        // ==========================================
        Selectable elementoActual = lista[indiceActual];
        if (elementoActual == null) return;

        if (elementoActual is Slider slider)
        {
            float paso = (slider.maxValue - slider.minValue) * 0.1f;
            if (h < -0.5f || Input.GetKeyDown(KeyCode.LeftArrow)) // IZQUIERDA
            {
                if (!inputEnEspera) { slider.value -= paso; inputEnEspera = true; }
            }
            else if (h > 0.5f || Input.GetKeyDown(KeyCode.RightArrow)) // DERECHA
            {
                if (!inputEnEspera) { slider.value += paso; inputEnEspera = true; }
            }
        }
        else if (elementoActual is Button boton)
        {
            if (confirmar) boton.onClick.Invoke(); // Aceptar
        }

        // ==========================================
        // 3. QUITAR EL SEGURO
        // ==========================================
        // Si la palanca del mando o las flechas se sueltan (vuelven al centro), quitamos el seguro
        if (Mathf.Abs(v) < 0.1f && Mathf.Abs(h) < 0.1f)
        {
            inputEnEspera = false;
        }
    }

    void SeleccionarActual(Selectable[] lista, int indiceActual)
    {
        ActualizarColores(lista, indiceActual);
        if (lista[indiceActual] != null) lista[indiceActual].Select();
    }

    void ActualizarColores(Selectable[] lista, int indiceActual)
    {
        if (lista == null) return;

        for (int i = 0; i < lista.Length; i++)
        {
            if (lista[i] == null) continue;

            Image imagenAPintar = null;

            if (lista[i] is Button)
            {
                // Si es botón, pintamos su propia imagen
                imagenAPintar = lista[i].GetComponent<Image>();
            }
            else if (lista[i] is Slider)
            {
                // Si es slider, buscamos el "Handle" (la bolita que se mueve) y la pintamos
                Transform handle = lista[i].transform.Find("Handle Slide Area/Handle");
                if (handle != null) imagenAPintar = handle.GetComponent<Image>();
            }

            if (imagenAPintar != null)
            {
                imagenAPintar.color = (i == indiceActual) ? colorSeleccionado : colorNormal;
            }
        }
    }

    // ==========================================
    // FUNCIONES PÚBLICAS (Pausar, Reanudar, Salir)
    // ==========================================
    public void Pausar()
    {
        juegoPausado = true;
        menuPause.SetActive(true);
        Time.timeScale = 0f;
        index = 0; 
        SeleccionarActual(opciones, index);
    }

    public void Reanudar()
    {
        juegoPausado = false;
        menuPause.SetActive(false);
        if (panelTienda != null) panelTienda.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        Time.timeScale = 1f;
    }

    public void AbrirOpciones()
    {
        menuPause.SetActive(false);
        if (panelOpciones != null) 
        {
            panelOpciones.SetActive(true);
            indexOpciones = 0; 
            SeleccionarActual(elementosOpciones, indexOpciones);
        }
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        menuPause.SetActive(true);
        index = 0;
        SeleccionarActual(opciones, index);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f; 
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.LoadScene(nombreEscenaMenu, TransitionType.Fade);
        else
            SceneManager.LoadScene(nombreEscenaMenu);
    }
}