using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Seleccion_Manager : MonoBehaviour
{
    public static Seleccion_Manager Instance;

    [Header("Sprites disponibles")]
    [SerializeField] private Sprite[] personajesSprites;

    [Header("UI Jugadores")]
    [SerializeField] private Image imagenJugador1;
    [SerializeField] private Image imagenJugador2;

    [Header("Escena a cargar")]
    [SerializeField] private string escenaJuego;

    [Header("Transición")]
    [SerializeField] private TransitionType tipoTransicion = TransitionType.Fade;

    private int seleccionJ1 = 0;
    private int seleccionJ2 = 1;

    private bool j1Confirmado = false;
    private bool j2Confirmado = false;
    private bool bloqueado = false;

    private float cooldown = 0.2f;
    private float tiempo = 0f;

    private Vector3 posInicialJ1;
    private Vector3 posInicialJ2;

    private float saltoAltura = 15f;
    private float saltoVelocidad = 5f;

    public static int personajeFinalJ1;
    public static int personajeFinalJ2;

    // SEGURO PARA EL MANDO (Evita que pase por 20 personajes en un segundo)
    private bool ejeHSuelta = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        posInicialJ1 = imagenJugador1.rectTransform.localPosition;
        posInicialJ2 = imagenJugador2.rectTransform.localPosition;

        ActualizarVisuales();
    }

    private void Update()
    {
        if (bloqueado) return;

        // Leemos la palanca / cruceta del mando
        float h = Input.GetAxisRaw("Horizontal");

        // =========================
        // JUGADOR 1 (FLECHAS O MANDO)
        // =========================
        if (!j1Confirmado)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || (h > 0.5f && ejeHSuelta))
            {
                J1_Derecha();
                ejeHSuelta = false; // Ponemos el seguro
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || (h < -0.5f && ejeHSuelta))
            {
                J1_Izquierda();
                ejeHSuelta = false; // Ponemos el seguro
            }
        }

        // =========================
        // JUGADOR 2 (A y D - TECLADO)
        // =========================
        // (En el sistema clásico de Unity, ambos mandos se mezclan. 
        // Asumimos que J2 usa teclado si juegan en la misma PC sin configuración extra de Input Manager).
        if (!j2Confirmado)
        {
            if (Input.GetKeyDown(KeyCode.D)) J2_Derecha();
            if (Input.GetKeyDown(KeyCode.A)) J2_Izquierda();
        }

        // Quitamos el seguro cuando la palanca del mando vuelve al centro
        if (Mathf.Abs(h) < 0.1f)
        {
            ejeHSuelta = true;
        }

        // =========================
        // CONFIRMAR (ENTER O BOTÓN "A" DEL MANDO)
        // =========================
        tiempo -= Time.deltaTime; // Usamos tiempo para evitar dobles confirmaciones accidentales
        bool confirmar = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);
        
        if (confirmar && tiempo <= 0f)
        {
            if (!j1Confirmado) ConfirmarJ1();
            else if (!j2Confirmado) ConfirmarJ2();
            
            tiempo = cooldown;
        }

        // =========================
        // CANCELAR (BACKSPACE O BOTÓN "B" DEL MANDO)
        // =========================
        bool cancelar = Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton1);
        
        if (cancelar && tiempo <= 0f)
        {
            if (j2Confirmado) j2Confirmado = false;
            else if (j1Confirmado) j1Confirmado = false;

            tiempo = cooldown;
        }

        AnimarSalto();
    }

    // =========================
    // MOVIMIENTO JUGADOR 1
    // =========================
    private void J1_Derecha()
    {
        seleccionJ1 = (seleccionJ1 + 1) % personajesSprites.Length;
        if (j2Confirmado && seleccionJ1 == seleccionJ2)
            seleccionJ1 = (seleccionJ1 + 1) % personajesSprites.Length;
        ActualizarVisuales();
    }

    private void J1_Izquierda()
    {
        seleccionJ1--;
        if (seleccionJ1 < 0) seleccionJ1 = personajesSprites.Length - 1;
        if (j2Confirmado && seleccionJ1 == seleccionJ2)
        {
            seleccionJ1--;
            if (seleccionJ1 < 0) seleccionJ1 = personajesSprites.Length - 1;
        }
        ActualizarVisuales();
    }

    // =========================
    // MOVIMIENTO JUGADOR 2
    // =========================
    private void J2_Derecha()
    {
        seleccionJ2 = (seleccionJ2 + 1) % personajesSprites.Length;
        if (j1Confirmado && seleccionJ2 == seleccionJ1)
            seleccionJ2 = (seleccionJ2 + 1) % personajesSprites.Length;
        ActualizarVisuales();
    }

    private void J2_Izquierda()
    {
        seleccionJ2--;
        if (seleccionJ2 < 0) seleccionJ2 = personajesSprites.Length - 1;
        if (j1Confirmado && seleccionJ2 == seleccionJ1)
        {
            seleccionJ2--;
            if (seleccionJ2 < 0) seleccionJ2 = personajesSprites.Length - 1;
        }
        ActualizarVisuales();
    }

    private void ConfirmarJ1()
    {
        j1Confirmado = true;
        Debug.Log("Jugador 1 confirmado");
        if (seleccionJ2 == seleccionJ1)
        {
            seleccionJ2 = (seleccionJ1 + 1) % personajesSprites.Length;
            ActualizarVisuales();
        }
    }

    private void ConfirmarJ2()
    {
        if (seleccionJ2 == seleccionJ1) return;

        j2Confirmado = true;
        Debug.Log("Jugador 2 confirmado");
        VerificarInicio();
    }

    private void ActualizarVisuales()
    {
        imagenJugador1.sprite = personajesSprites[seleccionJ1];
        imagenJugador2.sprite = personajesSprites[seleccionJ2];
    }

    private void AnimarSalto()
    {
        if (j1Confirmado)
        {
            float y = Mathf.Sin(Time.time * saltoVelocidad) * saltoAltura;
            imagenJugador1.rectTransform.localPosition = posInicialJ1 + new Vector3(0, y, 0);
        }
        else imagenJugador1.rectTransform.localPosition = posInicialJ1;

        if (j2Confirmado)
        {
            float y = Mathf.Sin(Time.time * saltoVelocidad) * saltoAltura;
            imagenJugador2.rectTransform.localPosition = posInicialJ2 + new Vector3(0, y, 0);
        }
        else imagenJugador2.rectTransform.localPosition = posInicialJ2;
    }

    private void VerificarInicio()
    {
        if (j1Confirmado && j2Confirmado)
        {
            bloqueado = true;
            personajeFinalJ1 = seleccionJ1;
            personajeFinalJ2 = seleccionJ2;

            if (TransitionManager.Instance != null)
                TransitionManager.Instance.LoadScene(escenaJuego, tipoTransicion);
            else
                SceneManager.LoadScene(escenaJuego);
        }
    }
}