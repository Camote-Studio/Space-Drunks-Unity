using UnityEngine;

public class ShopButtonMenuManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.ShopButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.ShopButtonsClicked(_buttonType);
    }
}
