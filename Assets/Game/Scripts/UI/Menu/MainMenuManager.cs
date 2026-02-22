using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    //static significa que es compartido entre todas las instancias de la clase
    public static MainMenuManager _; //Singleton instance
    [SerializeField] private bool _debugMode;

    public enum MainMenuButtons { StoryMode, VersuMode, Shop, Medals, Options, Quit };
    public enum SocialButtons {Instagram, ItchIO};
    //AGREGAR MÁS BOTONES EN CASO SER NECESARIO.
    public enum StoryButtons {back, newGame, versusGame};
    public enum VersusButtons {back};
    public enum ShopButtons {back};
    public enum MedalsButtons {back};
    public enum OptionsButtons {back};

    [SerializeField] GameObject _MainMenuContainer;
    [SerializeField] GameObject _StoryMenuContainer;
    [SerializeField] GameObject _VersusMenuContainer;
    [SerializeField] GameObject _ShopMenuContainer;
    [SerializeField] GameObject _MedalsMenuContainer;
    [SerializeField] GameObject _OptionsMenuContainer;

    [SerializeField] private string _sceneToLoadAfterClickingPlay; //Nombre de la escena a cargar al hacer click en Play

    public void Awake()
    {
        //Verificar que singleton este seteado
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("Hay más de un MainMenuManager en la escena");
        }

    }

    //Una vez que todo hay sido inicializado bien en awake, se puede llamar a start
    public void Start ()
    {
        OpenMenu(_MainMenuContainer);

        
    }

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button clicked: " + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.StoryMode:
                DebugMessage("Starting Story Mode...");
                //Agregar logica para iniciar Story Mode
                //OpenStoryMenu();
                //TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => OpenStoryMenu());
                //Con transición ir a la ecena _sceneToLoadAfterClickingPlay
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => PlayClicked());
                break;
            case MainMenuButtons.VersuMode:
                DebugMessage("Starting Versu Mode...");
                //Agregar logica para iniciar Versu Mode
                //OpenVersusMenu();
                TransitionManager.Instance.LocalTransition(TransitionType.CircleExpand, () => OpenVersusMenu());
                break;
            case MainMenuButtons.Shop:
                DebugMessage("Opening Shop...");
                //Agregar logica para abrir Shop
                //OpenShopMenu();
                TransitionManager.Instance.LocalTransition(TransitionType.CircleExpand, () => OpenShopMenu());
                break;
            case MainMenuButtons.Medals:
                DebugMessage("Opening Medals...");
                //Agregar logica para abrir Medals
                OpenMedalsMenu();
                break;
            case MainMenuButtons.Options:
                DebugMessage("Opening Options...");
                //Agregar logica para abrir Options
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => OpenOptionsMenu());
                break;
            case MainMenuButtons.Quit:
                QuitGame();
                break;
            default:
                DebugMessage("Unknown button clicked.");
                break;
        } 
        
    }

    public void SocialButtonClicked(SocialButtons buttonClicked)
    {
        string websiteLink = "";
        switch (buttonClicked)
        {
            case SocialButtons.Instagram:
                websiteLink = "https://www.instagram.com/camotestudiogames/"; 
                break;
            case SocialButtons.ItchIO:
                websiteLink = "https://unlucky-alpaca.itch.io/space-drunks";
                break;
            default: 
                Debug.Log("Botón de red social no implementado en SocialButtonClicked method");
                break; 
        }

        if (websiteLink != "")
        {
            Application.OpenURL(websiteLink);
        }
    }

    public void ReturnToMainMenu()
    {
        OpenMenu(_MainMenuContainer);   
    }

    /// ----------------------------------------------------------------------------------
    public void OpenStoryMenu()
    {
        OpenMenu(_StoryMenuContainer);
    }

    public void OpenVersusMenu()
    {
        OpenMenu(_VersusMenuContainer);
    }

    public void OpenShopMenu()
    {
        OpenMenu(_ShopMenuContainer);
    }

    public void OpenMedalsMenu()
    {
        OpenMenu(_MedalsMenuContainer);
    }

    public void OpenOptionsMenu()
    {
        OpenMenu(_OptionsMenuContainer);
    }

    //-------------------------------------------->
    public void StoryButtonsClicked (StoryButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case StoryButtons.back:
                //Transición
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => ReturnToMainMenu());
                break;
            case StoryButtons.newGame:
                PlayClicked();
                break;
            case StoryButtons.versusGame:
                OpenVersusMenu();
                break;
        }
    }

    public void VersusButtonsClicked (VersusButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case VersusButtons.back:
                ReturnToMainMenu();
                break;
        }
    }    

    public void ShopButtonsClicked (ShopButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case ShopButtons.back:
                ReturnToMainMenu();
                break;
        }
    }

    public void MedalsButtonsClicked (MedalsButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case MedalsButtons.back:
                ReturnToMainMenu();
                break;
        }
    }

    public void OptionsButtonsClicked (OptionsButtons buttonClicked)
    {
        switch(buttonClicked)
        {
            case OptionsButtons.back:
                TransitionManager.Instance.LocalTransition(TransitionType.Fade, () => ReturnToMainMenu());
                break;
        }
    }

    /// ---------------------------------------------------------------------------
    
    private void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }

    public void PlayClicked()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }

    public void QuitGame()
    {
        DebugMessage("Quitting game..."); 
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }

    public void OpenMenu(GameObject menuToOpen)
    {
        _MainMenuContainer.SetActive(menuToOpen == _MainMenuContainer);
        _StoryMenuContainer.SetActive(menuToOpen == _StoryMenuContainer);
        _VersusMenuContainer.SetActive(menuToOpen == _VersusMenuContainer);
        _ShopMenuContainer.SetActive(menuToOpen == _ShopMenuContainer);
        _MedalsMenuContainer.SetActive(menuToOpen == _MedalsMenuContainer);
        _OptionsMenuContainer.SetActive(menuToOpen == _OptionsMenuContainer);
    }

}
