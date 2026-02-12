using UnityEngine;

public class monedas : MonoBehaviour
{
    [SerializeField] private int valor = 1;
    [SerializeField] private float tiempoDeVida = 10f;

    private void Start()
    {
        // Si nadie la recoge, se elimina sola
        Destroy(gameObject, tiempoDeVida);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        VidaJugador jugador = other.GetComponent<VidaJugador>();

        if (jugador != null)
        {
            jugador.AgregarMonedas(valor);
            Destroy(gameObject); // Se elimina al recogerla
        }
    }
}
