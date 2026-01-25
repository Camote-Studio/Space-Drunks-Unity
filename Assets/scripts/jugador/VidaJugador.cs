using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float vidaActual;

    public float VidaMaxima => vidaMaxima;
    public float VidaActual => vidaActual;

    public event Action<string> OnDamaged; // 👈 con fuente
    public event Action OnDeath;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDanio(float cantidad, string fuente = "")
    {
        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        OnDamaged?.Invoke(fuente); // 👈 pasamos la fuente

        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log("Jugador muerto");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
