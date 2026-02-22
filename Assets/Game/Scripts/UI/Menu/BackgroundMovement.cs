using UnityEngine;
using UnityEngine.UI; 

public class BackgroundMovement : MonoBehaviour
{
    [Header("Configuración del Movimiento")]
    public float distancia = 50f;
    public float velocidad = 1f;

    [Header("Configuración de Cambio de Fondo")]
    public Sprite[] fondos;
    public float tiempoEntreCambios = 5f;

    private RectTransform _rectTransform;
    private Vector2 _posicionInicial;
    
    private Image _imagenFondo;
    private float _timer;
    private int _indiceActual = 0;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _imagenFondo = GetComponent<Image>();
        
        // ESCUDO 1: Solo guardamos la posición si realmente hay un RectTransform
        if (_rectTransform != null)
        {
            _posicionInicial = _rectTransform.anchoredPosition;
        }
        else
        {
            Debug.LogWarning("Falta un componente RectTransform en " + gameObject.name);
        }

        _timer = tiempoEntreCambios;

        // ESCUDO 2: Verificamos que la lista 'fondos' exista y tenga al menos 1 imagen
        if (fondos != null && fondos.Length > 0 && _imagenFondo != null)
        {
            if (fondos[_indiceActual] != null) // Evita errores si hay un espacio vacío en la lista
            {
                _imagenFondo.sprite = fondos[_indiceActual];
            }
        }
    }

    void Update()
    {
        // ==========================================
        // 1. MOVIMIENTO SEGURO
        // ==========================================
        if (_rectTransform != null)
        {
            float ondaSenoidal = Mathf.Sin(Time.time * velocidad);
            float nuevoX = _posicionInicial.x + (ondaSenoidal * distancia);
            _rectTransform.anchoredPosition = new Vector2(nuevoX, _posicionInicial.y);
        }

        // ==========================================
        // 2. CAMBIO DE IMAGEN SEGURO
        // ==========================================
        // ESCUDO 3: Solo cambiamos de imagen si la lista existe, tiene más de 1 fondo y tenemos un componente Image
        if (fondos != null && fondos.Length > 1 && _imagenFondo != null)
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                CambiarFondo();
                _timer = tiempoEntreCambios;
            }
        }
    }

    void CambiarFondo()
    {
        _indiceActual++;

        if (_indiceActual >= fondos.Length)
        {
            _indiceActual = 0;
        }

        // ESCUDO 4: Nos aseguramos de que no hayas dejado un hueco vacío en el Inspector
        if (fondos[_indiceActual] != null)
        {
            _imagenFondo.sprite = fondos[_indiceActual];
        }
    }
}