using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonTienda : MonoBehaviour
{
    void Update()
    {
        // Detecta cuando se presiona Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Volver();
        }
    }

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
