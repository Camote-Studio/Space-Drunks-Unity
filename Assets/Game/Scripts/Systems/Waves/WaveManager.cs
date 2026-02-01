using UnityEngine;
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
        public List<GrupoEnemigos> grupos; // Lista de sub-grupos (ej: 5 patos y luego 3 gatos)
    }

    // ================= VARIABLES =================

    [Header("Configuración")]
    public List<Oleada> oleadas;
    public Transform[] puntosDeSpawn; // Arrastra tus spawn points aquí
    public float tiempoEntreOleadas = 5f;

    [Header("Estado (Solo lectura)")]
    public int oleadaActualIndex = 0;
    public int enemigosVivos = 0;
    public bool esperandoSiguienteOleada = false;
    public bool nivelCompletado = false;

    // ================= UNITY =================

    void Start()
    {
        // Nos suscribimos al evento de muerte
        enemigo_base.OnAnyEnemyDeath += EnemigoEliminado;
        
        // Iniciar la primera oleada
        StartCoroutine(IniciarOleada(oleadaActualIndex));
    }

    void OnDestroy()
    {
        // IMPORTANTE: Desuscribirse para evitar errores de memoria
        enemigo_base.OnAnyEnemyDeath -= EnemigoEliminado;
    }

    // ================= LÓGICA PRINCIPAL =================

    IEnumerator IniciarOleada(int index)
    {
        if (index >= oleadas.Count)
        {
            Debug.Log("¡JUEGO COMPLETADO!");
            nivelCompletado = true;
            yield break;
        }

        esperandoSiguienteOleada = false;
        Oleada oleadaActual = oleadas[index];
        Debug.Log($"Iniciando Oleada {index + 1}: {oleadaActual.nombre}");

        // 1. CALCULAR TOTAL ENEMIGOS
        // Sumamos todos los enemigos de todos los grupos para saber cuántos esperar
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

        // Elegir un punto aleatorio
        Transform punto = puntosDeSpawn[Random.Range(0, puntosDeSpawn.Length)];

        // Generar una posición aleatoria alrededor del punto (Radio de 1 metro)
        // Esto evita que nazcan todos en el pixel exacto
        Vector3 desplazamiento = Random.insideUnitCircle * 1.0f; 
        Vector3 posicionFinal =  punto.position + desplazamiento;
        GameObject enemigo = PoolManager.Instance.SpawnFromPool(tag, posicionFinal, Quaternion.identity);

        if (enemigo == null)
        {
            Debug.LogError($" ERROR CRÍTICO: No se pudo spawnear el tag '{tag}'. ¿Está bien escrito en el WaveManager y en el PoolManager?");
            
            // RESTAMOS AL ENEMIGO FANTASMA para que el juego no se quede trabado
            enemigosVivos--; 
            
            // Verificamos si al restar este error, la oleada terminó instantáneamente
            if (enemigosVivos <= 0 && !esperandoSiguienteOleada)
            {
                StartCoroutine(PrepararSiguienteOleada());
            }
        }
    }

    // ================= EVENTO DE MUERTE =================

    void EnemigoEliminado()
    {
        // Si ya ganamos o estamos esperando, ignorar
        if (nivelCompletado || esperandoSiguienteOleada) return;

        enemigosVivos--;

        if (enemigosVivos <= 0)
        {
            enemigosVivos = 0;
            StartCoroutine(PrepararSiguienteOleada());
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
}