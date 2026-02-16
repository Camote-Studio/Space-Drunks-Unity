using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenuButtons : MonoBehaviour
{
    [Header("UI")]
    public GameObject menuPause;
    public Button[] opciones;

    public Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    public Color colorSeleccionado = new Color(1f, 0.83f, 0f);

    private bool juegoPausado = false;
    private int index = 0;

    void Start()
    {
        menuPause.SetActive(false);
        index = 0;
        ActualizarColores();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado) Reanudar();
            else Pausar();
        }

        if (!juegoPausado) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = (index + 1) % opciones.Length;
            SeleccionarActual();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            index--;
            if (index < 0) index = opciones.Length - 1;
            SeleccionarActual();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            opciones[index].onClick.Invoke();
        }
    }

    void SeleccionarActual()
    {
        ActualizarColores();
        opciones[index].Select();
    }

    void ActualizarColores()
    {
        for (int i = 0; i < opciones.Length; i++)
        {
            TextMeshProUGUI txt = opciones[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.color = (i == index) ? colorSeleccionado : colorNormal;
        }
    }

    public void Pausar()
    {
        juegoPausado = true;
        menuPause.SetActive(true);
        Time.timeScale = 0f;

        index = 0; // 🔥 MUY IMPORTANTE
        SeleccionarActual();
    }

    public void Reanudar()
    {
        juegoPausado = false;
        menuPause.SetActive(false);
        Time.timeScale = 1f;
    }
    public void IrATienda()
    {
        SceneContext.escenaAnterior = SceneManager.GetActiveScene().name;
        SceneContext.volverAlJuego = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene("tienda");
    }


}
