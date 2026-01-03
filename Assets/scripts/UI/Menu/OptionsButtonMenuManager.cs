using UnityEngine;

public class OptionsButtonMenuManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.OptionsButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.OptionsButtonsClicked(_buttonType);
    }
}
