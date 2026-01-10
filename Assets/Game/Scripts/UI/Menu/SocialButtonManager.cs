using UnityEngine;

public class SocialButtonManager : MonoBehaviour
{
    [SerializeField] MainMenuManager.SocialButtons _buttonType;

    public void ButtonClicked()
    {
        //Llamar al MainMenuManager para notificar que se clickeo un boton (de un tipo especifico)
        MainMenuManager._.SocialButtonClicked(_buttonType);
    }
}
     