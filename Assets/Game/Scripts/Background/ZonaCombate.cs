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
    [Tooltip("Ajusta la Y para subirlo, o la X para moverlo a los lados")]
    public Vector3 offsetGo = new Vector3(3.5f, 5.0f, 0f); // <-- AQUÍ CONTROLAS LA ALTURA
    public float velocidadParpadeo = 0.3f;
    [Tooltip("Tiempo que tarda en abrirse la zona tras matar al último enemigo")]
    public float tiempoEsperaApertura = 3f; // <-- NUEVA VARIABLE: 2 segundos de pausa

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
        Debug.Log("Combate Terminado: Mostrando GO! al instante...");
        zonaCompletada = true;
        
        // Iniciamos la nueva secuencia
        StartCoroutine(SecuenciaVictoria());
    }

    IEnumerator SecuenciaVictoria()
    {
        // ==========================================
        // PASO 1: APARECE EL "GO!" AL INSTANTE
        // ==========================================
        if (indicadorGo != null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            
            if (jugador != null)
            {
                indicadorGo.transform.position = jugador.transform.position + offsetGo;
            }

            // Encendemos el letrero. Su Animator empezará el loop automáticamente.
            indicadorGo.SetActive(true);
        }

        // ==========================================
        // PASO 2: EL TIEMPO QUE DURA LA ANIMACIÓN
        // ==========================================
        // Dejamos que el jugador vea el "GO!" haciendo su loop durante 2 segundos
        yield return new WaitForSeconds(tiempoEsperaApertura); 

        // ==========================================
        // PASO 3: SE APAGA Y SE ABREN LAS PUERTAS
        // ==========================================
        if (indicadorGo != null)
        {
            indicadorGo.SetActive(false);
        }

        camaraZona.Priority = 0;
        if(barreraSalida != null) barreraSalida.SetActive(false);
        if(barreraEntrada != null) barreraEntrada.SetActive(false);
    }
    
    // (Hemos borrado la corrutina ParpadearGo porque ahora lo controlas tú con tu propia animación)

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