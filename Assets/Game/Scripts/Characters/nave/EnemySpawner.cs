using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform player;

    [Header("Spawn")]
    [SerializeField] float spawnOffset = 1.5f;
    [SerializeField] float spawnDelay = 3f;

    [Header("Carriles")]
    [SerializeField] int numeroCarriles = 5;

    Camera cam;
    float[] carrilesY;
    bool[] carrilOcupado;

    void Start()
    {
        cam = Camera.main;

        CalcularCarriles();

        carrilOcupado = new bool[numeroCarriles];

        InvokeRepeating(nameof(SpawnArmadillo), 2f, spawnDelay);
    }

    void CalcularCarriles()
    {
        float camHeight = cam.orthographicSize;
        float camCenterY = cam.transform.position.y;

        float bottom = camCenterY - camHeight;
        float top = camCenterY + camHeight;

        carrilesY = new float[numeroCarriles];

        for (int i = 0; i < numeroCarriles; i++)
        {
            float t = (i + 0.5f) / numeroCarriles;
            carrilesY[i] = Mathf.Lerp(bottom, top, t);

            Debug.Log("Carril " + i + " Y: " + carrilesY[i]);
        }
    }
    void SpawnArmadillo()
    {
        if (PoolManager.Instance == null) return;

        int carril = ObtenerCarrilLibre();
        if (carril == -1) return;

        carrilOcupado[carril] = true;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float y = carrilesY[carril];

        Vector2 spawnPos = new Vector2(
            camPos.x + camWidth + spawnOffset,
            y
        );

        Vector2 attackPos = new Vector2(
            camPos.x + camWidth - 1.5f,
            y
        );

        GameObject enemy = PoolManager.Instance.SpawnFromPool(
            "armadillo",
            spawnPos,
            Quaternion.identity
        );

        if (enemy == null)
        {
            carrilOcupado[carril] = false;
            return;
        }
        Debug.Log("Armadillo spawn en carril: " + carril + " | Y: " + y);

        enemigo_armadillo armadillo = enemy.GetComponent<enemigo_armadillo>();

        if (armadillo != null)
        {
            armadillo.SetPosition(attackPos);
            armadillo.SetCarrilSpawner(this, carril);
        }
    }

    int ObtenerCarrilLibre()
    {
        int intentos = 20;

        while (intentos > 0)
        {
            int carril = Random.Range(0, numeroCarriles);

            if (!carrilOcupado[carril])
                return carril;

            intentos--;
        }

        return -1;
    }

    public void LiberarCarril(int carril)
    {
        if (carril >= 0 && carril < carrilOcupado.Length)
            carrilOcupado[carril] = false;
    }
}