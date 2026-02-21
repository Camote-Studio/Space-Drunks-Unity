using UnityEngine;
using System;
using Unity.Cinemachine;
using System.Collections;

public class ZonaCombate : MonoBehaviour
{
    [Header("Referencias")]
    public CinemachineCamera camaraZona;
    public GameObject barreraEntrada;
    public GameObject barreraSalida;
    public WaveManager spawner; 

    [Header("Señal GO! y Tiempos")]
    public GameObject indicadorGo; 
    public float velocidadParpadeo = 0.25f;
    [Tooltip("Tiempo que tarda en abrirse la zona tras matar al último enemigo")]
    public float tiempoEsperaApertura = 2f; // <-- NUEVA VARIABLE: 2 segundos de pausa

    private bool zonaCompletada = false;
    private bool combateIniciado = false;

    private void Start()
    {
        if (indicadorGo != null) indicadorGo.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player") && !zonaCompletada && !combateIniciado) 
        {
            combateIniciado = true;
            EmpezarCombate();
        }  
    }

    void EmpezarCombate()
    {
        Debug.Log("Combate Iniciado: Bloqueando cámara y salida");
        
        camaraZona.Priority = 20;
        
        if(barreraSalida != null) barreraSalida.SetActive(true);
        if(barreraEntrada != null) barreraEntrada.SetActive(true); 

        if(spawner != null) spawner.IniciarCombateDeZona(AlTerminarCombate);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !zonaCompletada && camaraZona.Priority == 20)
        {
            Debug.Log("Trampa activada: Forzando el final del combate");
            AlTerminarCombate();
        }
    }

    // ==========================================
    // NUEVA SECUENCIA DE VICTORIA CON PAUSA
    // ==========================================
    public void AlTerminarCombate()
    {   
        Debug.Log("Combate Terminado: Iniciando pausa antes de abrir...");
        zonaCompletada = true;
        
        // Iniciamos la corrutina que cuenta los 2 segundos
        StartCoroutine(SecuenciaApertura());
    }

    IEnumerator SecuenciaApertura()
    {
        // 3. APARECE EL LETRERO "GO!" Y EMPIEZA A PARPADEAR
        if (indicadorGo != null)
        {
            indicadorGo.SetActive(true);
            StartCoroutine(ParpadearGo());
        }
        // 1. ESPERA DRAMÁTICA (2 segundos)
        yield return new WaitForSeconds(tiempoEsperaApertura);

        // 2. SE ABREN LAS PUERTAS Y SE LIBERA LA CÁMARA
        camaraZona.Priority = 0;
        if(barreraSalida != null) barreraSalida.SetActive(false);
        if(barreraEntrada != null) barreraEntrada.SetActive(false);

        indicadorGo.SetActive(false);
    }

    // ==========================================
    // EFECTO VISUAL ARCADE
    // ==========================================
    IEnumerator ParpadearGo()
    {
        SpriteRenderer[] sprites = indicadorGo.GetComponentsInChildren<SpriteRenderer>();
        
        while (true) 
        {
            foreach (var sr in sprites) sr.enabled = false;
            yield return new WaitForSeconds(velocidadParpadeo);
            
            foreach (var sr in sprites) sr.enabled = true;
            yield return new WaitForSeconds(velocidadParpadeo);
        }
    }
}