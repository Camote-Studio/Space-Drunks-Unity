using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ShopCategory
{
    Skin,
    Baile,
    Poder
}

public class ShopManager : MonoBehaviour
{
    [SerializeField] private PlayerSkinManager playerSkinManager;
    [Header("Panel Item Seleccionado")]
    [SerializeField] private Transform panelItemSeleccionado;

    [Header("UI")]
    [SerializeField] private RectTransform content;
    [SerializeField] private TextMeshProUGUI categoryText;

    [Header("Prefabs por categoría")]
    [SerializeField] private List<GameObject> skins;
    [SerializeField] private List<GameObject> bailes;
    [SerializeField] private List<GameObject> poderes;

    [Header("Grid Settings")]
    [SerializeField] private int columnas = 3;
    [SerializeField] private int filasVisibles = 2;
    [SerializeField] private float altoFila = 191f;
    [SerializeField] private float offsetYInicial = -30f;
    [SerializeField] private float[] posicionesX = { -539f, -16f, 500f };

    [Header("Selección Visual")]
    [SerializeField] private float escalaSeleccionado = 1.2f;

    [Header("Colores")]
    [SerializeField] private Color colorSeleccion = new Color(1f, 0.82f, 0.05f, 1f);
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAura = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private float auraSize = 10f;

    private ShopCategory categoriaActual = ShopCategory.Skin;
    private List<GameObject> listaActual;
    private int indiceSeleccionado = 0;
    private int filaBase = 0;

    private void Start()
    {
        CargarCategoria();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) CambiarCategoria(1);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) CambiarCategoria(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MoverSeleccion(-1);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MoverSeleccion(1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            SeleccionarItem();
    }

    public void BotonCategoriaIzquierda() => CambiarCategoria(-1);
    public void BotonCategoriaDerecha() => CambiarCategoria(1);

    private void CambiarCategoria(int dir)
    {
        categoriaActual = (ShopCategory)Mathf.Repeat(
            (int)categoriaActual + dir,
            System.Enum.GetValues(typeof(ShopCategory)).Length
        );

        indiceSeleccionado = 0;
        filaBase = 0;

        CargarCategoria();
    }

    private void CargarCategoria()
    {
        categoryText.text = categoriaActual.ToString();

        // Limpiar hijos correctamente
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(content.GetChild(i).gameObject);
        }

        listaActual = ObtenerListaActual();

        for (int i = 0; i < listaActual.Count; i++)
        {
            GameObject item = Instantiate(listaActual[i], content, false);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
        }

        indiceSeleccionado = 0;
        filaBase = 0;

        ActualizarVista();
    }

    private List<GameObject> ObtenerListaActual()
    {
        return categoriaActual switch
        {
            ShopCategory.Skin => skins,
            ShopCategory.Baile => bailes,
            ShopCategory.Poder => poderes,
            _ => skins
        };
    }

    private void MoverSeleccion(int dir)
    {
        if (listaActual == null || listaActual.Count == 0) return;

        indiceSeleccionado += dir;

        if (indiceSeleccionado < 0)
            indiceSeleccionado = listaActual.Count - 1;

        if (indiceSeleccionado >= listaActual.Count)
            indiceSeleccionado = 0;

        int filaSeleccion = indiceSeleccionado / columnas;

        if (filaSeleccion >= filaBase + filasVisibles)
            filaBase = filaSeleccion - filasVisibles + 1;
        else if (filaSeleccion < filaBase)
            filaBase = filaSeleccion;

        filaBase = Mathf.Max(0, filaBase);

        ActualizarVista();
    }

    private void ActualizarVista()
    {
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform rt = content.GetChild(i).GetComponent<RectTransform>();
            Image img = rt.GetComponent<Image>();

            int filaReal = i / columnas;
            int columna = i % columnas;
            int filaVisible = filaReal - filaBase;

            rt.gameObject.SetActive(filaVisible >= 0 && filaVisible < filasVisibles);

            if (!rt.gameObject.activeSelf) continue;

            rt.anchoredPosition = new Vector2(
                posicionesX[columna],
                offsetYInicial - (filaVisible * altoFila)
            );

            Outline outline = rt.GetComponent<Outline>();
            if (outline == null)
                outline = rt.gameObject.AddComponent<Outline>();

            if (i == indiceSeleccionado)
            {
                rt.localScale = Vector3.one * escalaSeleccionado;

                if (img)
                    img.color = colorSeleccion;

                outline.effectColor = colorAura;
                outline.effectDistance = new Vector2(auraSize, auraSize);
                outline.enabled = true;
            }
            else
            {
                rt.localScale = Vector3.one;

                if (img)
                    img.color = colorNormal;

                outline.enabled = false;
            }
        }
    }

    private void SeleccionarItem()
    {
        if (listaActual == null || listaActual.Count == 0) return;

        GameObject prefabSeleccionado = listaActual[indiceSeleccionado];

        // Limpiar el panel
        for (int i = panelItemSeleccionado.childCount - 1; i >= 0; i--)
        {
            Destroy(panelItemSeleccionado.GetChild(i).gameObject);
        }

        // Instanciar correctamente como UI
        GameObject nuevoItem = Instantiate(prefabSeleccionado);
        nuevoItem.transform.SetParent(panelItemSeleccionado, false);

        RectTransform rt = nuevoItem.GetComponent<RectTransform>();

        // 🔥 HACERLO MÁS GRANDE
        float escalaPreview = 3f;   // puedes ajustar este valor
        rt.localScale = Vector3.one * escalaPreview;

        // Centrarlo en el panel
        rt.anchoredPosition = Vector2.zero;

        // Guardar si es skin
        if (categoriaActual == ShopCategory.Skin)
        {
            PlayerPrefs.SetInt("SkinSeleccionada", indiceSeleccionado);
            PlayerPrefs.Save();
            Debug.Log("Skin guardada: " + indiceSeleccionado);
        }
    }




}
