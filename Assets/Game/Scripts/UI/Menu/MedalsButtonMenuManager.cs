using UnityEngine;

public class MedalsButtonMenuManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.MedalsButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.MedalsButtonsClicked(_buttonType);
    }
}
