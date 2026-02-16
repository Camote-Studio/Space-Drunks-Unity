using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonTienda : MonoBehaviour
{
    public void Volver()
    {
        if (SceneContext.volverAlJuego && !string.IsNullOrEmpty(SceneContext.escenaAnterior))
        {
            SceneManager.LoadScene(SceneContext.escenaAnterior);
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}
