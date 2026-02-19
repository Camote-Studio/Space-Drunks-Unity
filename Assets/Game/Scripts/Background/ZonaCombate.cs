using UnityEngine;
using System;
using Unity.Cinemachine;

public class ZonaCombate : MonoBehaviour
{
    [Header("Referencias")]
    public CinemachineCamera camaraZona;
    public GameObject barreraEntrada;
    public GameObject barreraSalida;
    public WaveManager spawner; // Tu gestor de oleadas

    private bool zonaCompletada = false;
    private bool combateIniciado = false;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        // Cuando el jugador toca el trigger invisible
        if (other.CompareTag("Player") && !zonaCompletada && !combateIniciado) 
        {
            combateIniciado = true;
            EmpezarCombate();
        }  
    }

    void EmpezarCombate()
    {
        Debug.Log("Combate Iniciado: Bloqueando cámara y salida");
        
        // 1. Bloqueamos cámara y encerramos
        camaraZona.Priority = 20;
        // 2. ENCENDEMOS AMBAS BARRERAS
        if(barreraSalida != null) barreraSalida.SetActive(true);
        if(barreraEntrada != null) barreraEntrada.SetActive(true); // <-- LA CERRAMOS

        if(spawner != null) spawner.IniciarCombateDeZona(AlTerminarCombate);
    }

    private void Update()
    {
        // BOTÓN DE TRAMPA PARA PRUEBAS RÁPIDAS
        // Presiona la tecla 'T' para simular que todos los enemigos murieron
        if (Input.GetKeyDown(KeyCode.T) && !zonaCompletada && camaraZona.Priority == 20)
        {
            Debug.Log("Trampa activada: Forzando el final del combate");
            AlTerminarCombate();
        }
    }

    public void AlTerminarCombate()
    {   
        Debug.Log("Combate Terminado: Liberando zona");
        zonaCompletada = true;
        
        // Liberamos cámara y abrimos puerta
        camaraZona.Priority = 0;
        if(barreraSalida != null) barreraSalida.SetActive(false);
        if(barreraEntrada != null) barreraEntrada.SetActive(false);
    }
}