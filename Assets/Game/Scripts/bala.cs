using UnityEngine;

public class bala : MonoBehaviour, IPoolable
{
    [Header("Configuración")]
    public float danio = 10f;
    public float tiempoVida = 5f; // 15s es mucho, 5s suele bastar
    public string poolTag = "Bala"; // Debe coincidir con el tag en PoolManager

    [Header("Tipo de Bala")]
    public string tipo_bala = "bala_gravedad";

    private TrailRenderer tr;

    void Awake()
    {
        // Cacheamos el componente una sola vez para ahorrar rendimiento
        tr = GetComponent<TrailRenderer>();
    }

    // NO usamos Start() porque en Pooling Start solo se ejecuta la primera vez que se crea el objeto.
    // Usamos OnSpawnFromPool para inicializar cada disparo.

    public void OnSpawnFromPool()
    {
        // 1. Reiniciar Trail Renderer (Corrección de sintaxis)
        if (tr != null)
        {
            tr.Clear(); 
        }

        // 2. Programar la "muerte" automática si no golpea nada
        Invoke(nameof(Desactivar), tiempoVida);
    }

    public void OnDespawnToPool()
    {
        // Cancelar el Invoke para que no intente desactivarse dos veces
        CancelInvoke();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos por Tag primero (es más rápido que GetComponent)
        if (other.CompareTag("Player")) 
        {

            VidaJugador vida = other.GetComponent<VidaJugador>();
            if (vida != null)
            {
                vida.RecibirDanio(danio);
                Desactivar(); // <--- IMPORTANTE: No Destroy
            }
        }
        else if (other.CompareTag("Muro") || other.CompareTag("Suelo"))
        {
            // Opcional: Que la bala se destruya si choca con paredes
            Desactivar();
        }
    }

    void Desactivar()
    {
        // Devolvemos la bala al Pool en lugar de destruirla
        PoolManager.Instance.ReturnToPool(poolTag, gameObject);
    }
}