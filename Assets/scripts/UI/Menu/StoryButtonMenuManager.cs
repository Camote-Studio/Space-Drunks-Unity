using UnityEngine;

public class StoryButtonMenuManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.StoryButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.StoryButtonsClicked(_buttonType);
    }
}
