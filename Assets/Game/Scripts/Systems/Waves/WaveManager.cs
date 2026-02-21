using UnityEngine;
using System; // Necesario para usar Action
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    // ================= CLASES DE CONFIGURACIÓN =================

    [System.Serializable]
    public class GrupoEnemigos
    {
        public string poolTag;  // Ej: "Pato", "Gato" (Debe coincidir con PoolManager)
        public int cantidad;    // Cuántos de este tipo
        public float tasaAparicion = 1f; // Tiempo entre cada spawn de este grupo
    }

    [System.Serializable]
    public class Oleada
    {
        public string nombre;
        public List<GrupoEnemigos> grupos; // Lista de sub-grupos
    }

    // ================= VARIABLES =================

    [Header("Configuración")]
    public List<Oleada> oleadas;
    public Transform[] puntosDeSpawn; // Arrastra tus spawn points aquí
    public float tiempoEntreOleadas = 5f;
    public float tiempofinZona = 0.5f;

    [Header("Estado (Solo lectura)")]
    public int oleadaActualIndex = 0;
    public int enemigosVivos = 0;
    public bool esperandoSiguienteOleada = false;
    public bool nivelCompletado = false;

    // VARIABLE NUEVA: Aquí guardaremos la función de la Zona que nos llama
    private Action onZonaCompletada;

    // ================= UNITY =================

    void Start()
    {
        // Nos suscribimos al evento de muerte
        //enemigo_base.OnAnyEnemyDeath += EnemigoEliminado;
        
        // ELIMINADO: Ya no iniciamos la oleada automáticamente.
        // Esperaremos a que la ZonaCombate nos dé la orden.
    }

    void OnDestroy()
    {
        // IMPORTANTE: Desuscribirse para evitar errores de memoria
        enemigo_base.OnAnyEnemyDeath -= EnemigoEliminado;
    }

    // ================= CONEXIÓN CON LA ZONA =================

    // ESTA ES LA FUNCIÓN NUEVA QUE LLAMA ZONACOMBATE.CS
    public void IniciarCombateDeZona(Action callbackAlTerminar)
    {
        // Guardamos la función que abre las barreras para llamarla después
        onZonaCompletada = callbackAlTerminar; 

        enemigo_base.OnAnyEnemyDeath += EnemigoEliminado;
        
        // Empezamos la primera oleada de esta zona
        StartCoroutine(IniciarOleada(0)); 
    }

    // ================= LÓGICA PRINCIPAL =================

    IEnumerator IniciarOleada(int index)
    {
        // Si ya no quedan más oleadas en esta zona...
        if (index >= oleadas.Count)
        {
            Debug.Log("¡TODAS LAS OLEADAS DE ESTA ZONA COMPLETADAS!");
            nivelCompletado = true;

            enemigo_base.OnAnyEnemyDeath -= EnemigoEliminado;

            // AVISAMOS A LA ZONA QUE ABRA LAS PUERTAS
            onZonaCompletada?.Invoke(); 
            yield break;
        }

        esperandoSiguienteOleada = false;
        Oleada oleadaActual = oleadas[index];
        Debug.Log($"Iniciando Oleada {index + 1}: {oleadaActual.nombre}");

        // 1. CALCULAR TOTAL ENEMIGOS
        enemigosVivos = 0;
        foreach (var grupo in oleadaActual.grupos)
        {
            enemigosVivos += grupo.cantidad;
        }

        Debug.Log($"Enemigos esperados en esta oleada: {enemigosVivos}");

        // 2. SPAWNEAR ENEMIGOS
        foreach (var grupo in oleadaActual.grupos)
        {
            for (int i = 0; i < grupo.cantidad; i++)
            {
                if (nivelCompletado) yield break;

                SpawnEnemigo(grupo.poolTag);
                yield return new WaitForSeconds(grupo.tasaAparicion);
            }
        }
    }

    void SpawnEnemigo(string tag)
    {
        if (puntosDeSpawn.Length == 0) return;

        Transform punto = puntosDeSpawn[UnityEngine.Random.Range(0, puntosDeSpawn.Length)];
        Vector3 desplazamiento = UnityEngine.Random.insideUnitCircle * 1.0f; 
        Vector3 posicionFinal =  punto.position + desplazamiento;
        
        GameObject enemigo = PoolManager.Instance.SpawnFromPool(tag, posicionFinal, Quaternion.identity);

        if (enemigo == null)
        {
            Debug.LogError($" ERROR CRÍTICO: No se pudo spawnear el tag '{tag}'");
            enemigosVivos--; 
            
            if (enemigosVivos <= 0 && !esperandoSiguienteOleada)
            {
                StartCoroutine(PrepararSiguienteOleada());
            }
        }
    }

    // ================= EVENTO DE MUERTE =================

    void EnemigoEliminado()
    {
        if (nivelCompletado || esperandoSiguienteOleada) return;

        enemigosVivos--;

        if (enemigosVivos <= 0)
        {
            enemigosVivos = 0;
            if (oleadaActualIndex >= oleadas.Count - 1)
            {
                // Sí. Terminamos la zona rápido.
                StartCoroutine(TerminarZonaRapido());
            }
            else
            {
                // No. Aún quedan oleadas, hacemos la espera normal larga.
                StartCoroutine(PrepararSiguienteOleada());
            }
        }
    }

    IEnumerator PrepararSiguienteOleada()
    {
        esperandoSiguienteOleada = true;
        Debug.Log("Oleada completada. Descanso...");

        yield return new WaitForSeconds(tiempoEntreOleadas);

        oleadaActualIndex++;
        StartCoroutine(IniciarOleada(oleadaActualIndex));
    }

    IEnumerator TerminarZonaRapido()
    {
        esperandoSiguienteOleada = true;
        yield return new WaitForSeconds(tiempofinZona);
        nivelCompletado = true;
        enemigo_base.OnAnyEnemyDeath -= EnemigoEliminado;
        onZonaCompletada?.Invoke();
    }
}