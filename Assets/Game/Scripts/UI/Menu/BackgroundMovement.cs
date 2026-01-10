using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    [Tooltip("Qué tan lejos se mueve hacia los lados desde el centro.")]
    public float distancia = 50f; // Rango de movimiento

    [Tooltip("Qué tan rápido se mueve.")]
    public float velocidad = 1f;

    private RectTransform _rectTransform;
    private Vector2 _posicionInicial;

    void Start()
    {
        // Obtenemos el RectTransform del objeto
        _rectTransform = GetComponent<RectTransform>();
        
        // Guardamos la posición donde empieza para usarla de referencia central
        _posicionInicial = _rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Mathf.Sin crea una onda que va de -1 a 1 suavemente.
        // Multiplicamos por la velocidad para controlar el tiempo.
        float ondaSenoidal = Mathf.Sin(Time.time * velocidad);

        // Calculamos la nueva posición X:
        // Posición inicial X + (el valor de la onda entre -1 y 1 * la distancia deseada)
        float nuevoX = _posicionInicial.x + (ondaSenoidal * distancia);

        // Aplicamos la nueva posición manteniendo la Y original
        _rectTransform.anchoredPosition = new Vector2(nuevoX, _posicionInicial.y);
    }
}
