using UnityEngine;
using UnityEngine.AI;

public class DebugNavMesh : MonoBehaviour
{
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Hacemos que este script se ejecute cada 1 segundo para no llenar la consola
        InvokeRepeating(nameof(ReportarEstado), 1f, 1f);
    }

    void ReportarEstado()
    {
        if (agent == null) return;

        string estado = $"[DEBUG {name}] ";
        estado += $"OnNavMesh: {agent.isOnNavMesh} | "; // ¿Está pisando el mapa azul?
        estado += $"HasPath: {agent.hasPath} | ";       // ¿Calculó un camino hacia el jugador?
        estado += $"IsStopped: {agent.isStopped} | ";   // ¿Está frenado manualmente?
        estado += $"Velocity: {agent.velocity.magnitude.ToString("F2")}"; // ¿Qué velocidad intenta aplicar?

        Debug.Log(estado);
    }
}