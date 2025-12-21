using UnityEngine;

public class bala : MonoBehaviour
{
    public float danio = 10f;
    public float tiempoVida = 15f;

    void Start()
    {
        // 🧹 Se destruye sola
        Destroy(gameObject, tiempoVida);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 💥 SOLO daña al jugador
        VidaJugador vida = other.GetComponent<VidaJugador>();
        if (vida != null)
        {
            vida.RecibirDaño(danio);
            Destroy(gameObject);
        }
    }
}
