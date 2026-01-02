using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    //static significa que es compartido entre todas las instancias de la clase
    public static MainMenuManager _; //Singleton instance
    [SerializeField] private bool _debugMode;
    public enum MainMenuButtons { StoryMode, VersuMode, Shop, Medals, Options, Quit };
    public enum SocialButtons {Instagram, ItchIO};
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

    public void MainMenuButtonClicked(MainMenuButtons buttonClicked)
    {
        DebugMessage("Button clicked: " + buttonClicked.ToString());
        switch (buttonClicked)
        {
            case MainMenuButtons.StoryMode:
                DebugMessage("Starting Story Mode...");
                PlayClicked();
                //Agregar logica para iniciar Story Mode
                break;
            case MainMenuButtons.VersuMode:
                DebugMessage("Starting Versu Mode...");
                //Agregar logica para iniciar Versu Mode
                break;
            case MainMenuButtons.Shop:
                DebugMessage("Opening Shop...");
                //Agregar logica para abrir Shop
                break;
            case MainMenuButtons.Medals:
                DebugMessage("Opening Medals...");
                //Agregar logica para abrir Medals
                break;
            case MainMenuButtons.Options:
                DebugMessage("Opening Options...");
                //Agregar logica para abrir Options
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

}
