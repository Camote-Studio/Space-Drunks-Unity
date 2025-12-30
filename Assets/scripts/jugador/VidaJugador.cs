using UnityEngine;
using System;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    public event Action OnDamaged;

    private void Awake()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;

        OnDamaged?.Invoke();    

        if (vidaActual <= 0f)
        {
            vidaActual = 0f;
            Morir();
        }
    }

    private void Morir()
    {
        Debug.Log("Player muerto");
    }
}
