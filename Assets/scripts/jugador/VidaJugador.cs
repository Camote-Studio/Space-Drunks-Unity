using UnityEngine;
using System;
using TMPro; // o UnityEngine.UI si usas Text normal

public class VidaJugador : MonoBehaviour
{
    // ================== EXPERIENCIA ==================
    [Header("Experiencia")]
    [SerializeField] private int nivel = 1;
    [SerializeField] private int experienciaActual = 0;
    [SerializeField] private int experienciaParaSubir = 20;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float vidaActual;

    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;
    public float VidaMaxima => vidaMaxima;
    public float VidaActual => vidaActual;

    // ================== MONEDAS ==================
    [Header("Monedas")]
    [SerializeField] private int monedas = 0;

    [Tooltip("1 = Contador jugador 1 | 2 = Contador jugador 2")]
    public int idJugador = 1;

    [SerializeField] private TextMeshProUGUI textoMonedas;
    // si usas Text normal cambia a: public Text textoMonedas;

    [SerializeField] private HealthBarUI barraVidaUI;
    [SerializeField] private GameObject[] corazones;

    // ================== EVENTOS ==================
    public event Action<string> OnDamaged;
    public event Action OnDeath;

    private bool intocable = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        ActualizarUI();
        ActualizarBarraVida();
        ActualizarCorazones();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            RecibirDanio(10);
            Debug.Log("Presiona");
        }
    }

    // ================== VIDA ==================

    public void RecibirDanio(float cantidad, string fuente = "")
    {
        if (EsIntocable()) return;
        if (playerMovement != null && playerMovement.IsJumping) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        ActualizarBarraVida();
        ActualizarCorazones();
        OnDamaged?.Invoke(fuente);

        if (vidaActual <= 0f)
            Die();
    }

    private void ActualizarBarraVida()
    {
        float v = vidaActual / vidaMaxima;
        Debug.Log("Update barra a: " + v);
        barraVidaUI.Set01(v);
    }

    private void ActualizarCorazones()
    {
        float porcentaje = vidaActual / vidaMaxima;

        int corazonesActivos = 0;

        if (porcentaje > 0.66f)
            corazonesActivos = 3;
        else if (porcentaje > 0.33f)
            corazonesActivos = 2;
        else if (porcentaje > 0f)
            corazonesActivos = 1;
        else
            corazonesActivos = 0;

        for (int i = 0; i < corazones.Length; i++)
        {
            corazones[i].SetActive(i < corazonesActivos);
        }
    }

    public void RecibirDaño(float cantidad)
    {
        RecibirDanio(cantidad);
    }

    public bool EsIntocable()
    {
        return intocable;
    }

    public void ActivarIntocable(float duracion)
    {
        if (!intocable)
            StartCoroutine(IntocableCoroutine(duracion));
    }

    private System.Collections.IEnumerator IntocableCoroutine(float duracion)
    {
        intocable = true;
        yield return new WaitForSeconds(duracion);
        intocable = false;
    }

    private void Die()
    {
        Debug.Log($"Player {idJugador} dead");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    // ================== MONEDAS ==================

    public void AgregarMonedas(int cantidad)
    {
        monedas += cantidad;
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoMonedas != null)
            textoMonedas.text = monedas.ToString();
    }

    public void AgregarExperiencia(int cantidad)
    {
        experienciaActual += cantidad;

        while (experienciaActual >= experienciaParaSubir)
        {
            experienciaActual -= experienciaParaSubir;
            SubirNivel();
        }
    }


    private void SubirNivel()
    {
        nivel++;
        experienciaParaSubir += 10; // Escalado simple

        Debug.Log("¡Subiste a nivel " + nivel + "!");
    }


}
