using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float vidaActual;

    public float VidaMaxima => vidaMaxima;
    public float VidaActual => vidaActual;

    // Eventos para escuchar daño y muerte
    public event Action<string> OnDamaged; // Fuente de daño
    public event Action OnDeath;

    // Invulnerabilidad temporal opcional
    private bool intocable = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    /// <summary>
    /// Método principal para recibir daño
    /// </summary>
    /// <param name="cantidad">Cuánto daño recibe</param>
    /// <param name="fuente">Quién causó el daño</param>
    public void RecibirDanio(float cantidad, string fuente = "")
    {
        if (EsIntocable()) return; // Ignora daño si es intocable

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        OnDamaged?.Invoke(fuente);

        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    /// <summary>
    /// Método de compatibilidad para enemigos que llamen "RecibirDaño"
    /// </summary>
    /// <param name="cantidad"></param>
    public void RecibirDaño(float cantidad)
    {
        RecibirDanio(cantidad);
    }

    /// <summary>
    /// Determina si el jugador es intocable
    /// </summary>
    /// <returns></returns>
    public bool EsIntocable()
    {
        return intocable;
    }

    /// <summary>
    /// Pone al jugador intocable temporalmente
    /// </summary>
    /// <param name="duracion">Tiempo en segundos</param>
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

    /// <summary>
    /// Lógica de muerte del jugador
    /// </summary>
    private void Morir()
    {
        Debug.Log("Jugador muerto");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
