using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager _; // Singleton instance
    [SerializeField] private bool _debugMode;

    public enum MainMenuButtons { StoryMode, VersuMode, Shop, Medals, Options, Quit };
    public enum SocialButtons { Instagram, ItchIO };
    public enum StoryButtons { back, newGame, versusGame };
    public enum VersusButtons { back };
    public enum ShopButtons { back };
    public enum MedalsButtons { back };
    public enum OptionsButtons { back };

    [Header("Contenedores de UI")]
    [SerializeField] GameObject _MainMenuContainer;
    [SerializeField] GameObject _StoryMenuContainer;
    [SerializeField] GameObject _VersusMenuContainer;
    [SerializeField] GameObject _ShopMenuContainer;
    [SerializeField] GameObject _MedalsMenuContainer;
    [SerializeField] GameObject _OptionsMenuContainer;

    [SerializeField] private string _sceneToLoadAfterClickingPlay;

    [Header("Navegación Arcade (Botones y Sliders)")]
    public Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    public Color colorSeleccionado = new Color(1f, 0.83f, 0f);

    public Selectable[] elementosMain;
    public Selectable[] elementosStory;
    public Selectable[] elementosVersus;
    public Selectable[] elementosShop;
    public Selectable[] elementosMedals;
    public Selectable[] elementosOptions;

    // Índices independientes para recordar dónde te quedaste en cada pantalla
    private int idxMain = 0, idxStory = 0, idxVersus = 0, idxShop = 0, idxMedals = 0, idxOptions = 0;
    private bool inputEnEspera = false; // <-- EL SEGURO PARA EL MANDO

    public void Awake()
    {
        if (_ == null) _ = this;
        else Debug.LogError("Hay más de un MainMenuManager en la escena");
    }

    public void Start()
    {
        OpenMenu(_MainMenuContainer);
    }

    void Update()
    {
        // Detectamos qué menú está abierto para saber qué lista navegar
        if (_MainMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosMain, ref idxMain);
        else if (_StoryMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosStory, ref idxStory);
        else if (_VersusMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosVersus, ref idxVersus);
        else if (_ShopMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosShop, ref idxShop);
        else if (_MedalsMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosMedals, ref idxMedals);
        else if (_OptionsMenuContainer.activeInHierarchy) ProcesarNavegacion(elementosOptions, ref idxOptions);
    }

    // ==========================================
    // LÓGICA DE NAVEGACIÓN ARCADE
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
        // ESCUDO 1: Si la lista está vacía o no existe, salimos inmediatamente sin romper el juego
        if (lista == null || lista.Length == 0) return;

        ActualizarColores(lista, indiceActual);
        
        // ESCUDO 2: Nos aseguramos de que el índice sea válido antes de seleccionarlo
        if (indiceActual >= 0 && indiceActual < lista.Length && lista[indiceActual] != null)
        {
            lista[indiceActual].Select();
        }
    }

    void ActualizarColores(Selectable[] lista, int indiceActual)
    {
        if (lista == null) return;

        for (int i = 0; i < lista.Length; i++)
        {
            if (lista[i] == null) continue;

            Image imagenAPintar = null;

            if (lista[i] is Button)
                imagenAPintar = lista[i].GetComponent<Image>();
            else if (lista[i] is Slider)
            {
                Transform handle = lista[i].transform.Find("Handle Slide Area/Handle");
                if (handle != null) imagenAPintar = handle.GetComponent<Image>();
            }

            if (imagenAPintar != null)
                imagenAPintar.color = (i == indiceActual) ? colorSeleccionado : colorNormal;
        }
    }

    // ==========================================
    // TUS FUNCIONES ORIGINALES (Ligeramente ajustadas)
    // ==========================================

    public void OpenMenu(GameObject menuToOpen)
    {
        _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        _StoryMenuContainer.SetActive(menuToOpen == _StoryMenuContainer);
        _VersusMenuContainer.SetActive(menuToOpen == _VersusMenuContainer);
        _ShopMenuContainer.SetActive(menuToOpen == _ShopMenuContainer);
        _MedalsMenuContainer.SetActive(menuToOpen == _MedalsMenuContainer);
        _OptionsMenuContainer.SetActive(menuToOpen == _OptionsMenuContainer);

        // Al abrir un nuevo menú, pintamos la opción [0] para que aparezca seleccionada al instante
        if (menuToOpen == _MainMenuContainer) { idxMain = 0; SeleccionarActual(elementosMain, idxMain); }
        else if (menuToOpen == _StoryMenuContainer) { idxStory = 0; SeleccionarActual(elementosStory, idxStory); }
        else if (menuToOpen == _VersusMenuContainer) { idxVersus = 0; SeleccionarActual(elementosVersus, idxVersus); }
        else if (menuToOpen == _ShopMenuContainer) { idxShop = 0; SeleccionarActual(elementosShop, idxShop); }
        else if (menuToOpen == _MedalsMenuContainer) { idxMedals = 0; SeleccionarActual(elementosMedals, idxMedals); }
        else if (menuToOpen == _OptionsMenuContainer) { idxOptions = 0; SeleccionarActual(elementosOptions, idxOptions); }
    }

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button clicked: " + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.StoryMode:
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => PlayClicked());
                break;
            case MainMenuButtons.VersuMode:
                TransitionManager.Instance.LocalTransition(TransitionType.CircleExpand, () => OpenVersusMenu());
                break;
            case MainMenuButtons.Shop:
                TransitionManager.Instance.LocalTransition(TransitionType.CircleExpand, () => OpenShopMenu());
                break;
            case MainMenuButtons.Medals:
                OpenMedalsMenu();
                break;
            case MainMenuButtons.Options:
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => OpenOptionsMenu());
                break;
            case MainMenuButtons.Quit:
                QuitGame();
                break;
        }
    }

    public void SocialButtonClicked(SocialButtons buttonClicked)
    {
        string websiteLink = "";
        switch (buttonClicked)
        {
            case SocialButtons.Instagram: websiteLink = "https://www.instagram.com/camotestudiogames/"; break;
            case SocialButtons.ItchIO: websiteLink = "https://unlucky-alpaca.itch.io/space-drunks"; break;
        }
        if (websiteLink != "") Application.OpenURL(websiteLink);
    }

    public void ReturnToMainMenu() { OpenMenu(_MainMenuContainer); }
    public void OpenStoryMenu() { OpenMenu(_StoryMenuContainer); }
    public void OpenVersusMenu() { OpenMenu(_VersusMenuContainer); }
    public void OpenShopMenu() { OpenMenu(_ShopMenuContainer); }
    public void OpenMedalsMenu() { OpenMenu(_MedalsMenuContainer); }
    public void OpenOptionsMenu() { OpenMenu(_OptionsMenuContainer); }

    public void StoryButtonsClicked(StoryButtons buttonClicked)
    {
        switch (buttonClicked)
        {
            case StoryButtons.back: TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => ReturnToMainMenu()); break;
            case StoryButtons.newGame: PlayClicked(); break;
            case StoryButtons.versusGame: OpenVersusMenu(); break;
        }
    }

    public void VersusButtonsClicked(VersusButtons buttonClicked) { if (buttonClicked == VersusButtons.back) ReturnToMainMenu(); }
    public void ShopButtonsClicked(ShopButtons buttonClicked) { if (buttonClicked == ShopButtons.back) ReturnToMainMenu(); }
    public void MedalsButtonsClicked(MedalsButtons buttonClicked) { if (buttonClicked == MedalsButtons.back) ReturnToMainMenu(); }
    public void OptionsButtonsClicked(OptionsButtons buttonClicked)
    {
        if (buttonClicked == OptionsButtons.back) TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => ReturnToMainMenu());
    }

    private void DebugMessage(string message) { if (_debugMode) Debug.Log(message); }
    public void PlayClicked() { SceneManager.LoadScene(_sceneToLoadAfterClickingPlay); }

    public void QuitGame()
    {
        DebugMessage("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}