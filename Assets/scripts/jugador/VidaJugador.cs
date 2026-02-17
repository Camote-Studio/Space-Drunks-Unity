using UnityEngine;
using System;
using TMPro; // o UnityEngine.UI si usas Text normal

public class VidaJugador : MonoBehaviour
{
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

    // ================== EVENTOS ==================
    public event Action<string> OnDamaged;
    public event Action OnDeath;

    private bool intocable = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;
        ActualizarUI();
    }

    // ================== VIDA ==================

    public void RecibirDanio(float cantidad, string fuente = "")
    {
        if (EsIntocable()) return;
        if (playerMovement != null && playerMovement.IsJumping) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        OnDamaged?.Invoke(fuente);

        if (vidaActual <= 0f)
            Die();
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
}
