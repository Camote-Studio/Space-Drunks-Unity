using UnityEngine;
public class VidaJugador : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float vidaActual;
    void Awake()
    {
        vidaActual = vidaMaxima;
    }
    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;
        if (vidaActual <= 0)
        {
            Morir();
        }
    }
    void Morir()
    {
        Destroy(gameObject);
        Debug.Log("Jugador muerto");
    }
}
