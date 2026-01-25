using UnityEngine;

public class bala : MonoBehaviour
{
    public float danio = 10f;
    public float tiempoVida = 15f;

    [Header("Tipo de Bala")]
    public string tipo_bala = "bala_gravedad";

    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        VidaJugador vida = other.GetComponent<VidaJugador>();
        if (vida != null)
        {
            //vida.RecibirDaño(danio, tipo_bala); //<------- ORIGINAL
            vida.RecibirDanio(danio); // <------- MODIFICADO: quitar tipo_bala
            Destroy(gameObject);
        }
    }
}
