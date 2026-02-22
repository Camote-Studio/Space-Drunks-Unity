using UnityEngine;

public class AutoApagarGo : MonoBehaviour
{
    // Esta función será llamada por tu Animación cuando termine
    public void DesactivarObjeto()
    {
        gameObject.SetActive(false);
    }
}