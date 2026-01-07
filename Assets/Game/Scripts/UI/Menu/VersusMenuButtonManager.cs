using UnityEngine;

public class VersusMenuButtonManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.VersusButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.VersusButtonsClicked(_buttonType);
    }
}
