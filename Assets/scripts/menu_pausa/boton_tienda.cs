using UnityEngine;

public class BotonTienda : MonoBehaviour
{
    public GameObject panelTienda;
    public GameObject menuPause;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Volver();
        }
    }

    public void Volver()
    {
        panelTienda.SetActive(false);
        menuPause.SetActive(true);
    }
}
