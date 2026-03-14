using UnityEngine;

public class MisilSpawner : MonoBehaviour
{
    public GameObject misilPrefab;
    public Transform jugador;

    public float spawnInterval = 8f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnMisil), 2f, spawnInterval);
    }

    void SpawnMisil()
    {
        GameObject misil = Instantiate(misilPrefab);

        enemigo_misil script = misil.GetComponent<enemigo_misil>();

        if (script != null)
            script.SetJugador(jugador);
    }
}