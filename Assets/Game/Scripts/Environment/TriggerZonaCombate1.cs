using UnityEngine;
using System.Collections.Generic; // Necesario para usar Listas

public class TriggerZonaCombate1 : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("¿Cuántos jugadores deben entrar para cerrar los muros?")]
    public int jugadoresRequeridos = 2; 

    [Header("Muros")]
    public GameObject muroIzquierdo;
    public GameObject muroDerecho;

    // Esta lista recordará quiénes ya entraron
    private List<Collider2D> jugadoresDentro = new List<Collider2D>();
    private bool arenaActivada = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Si la arena ya se cerró, ignoramos todo
        if (arenaActivada) return;

        // Verificamos que sea un jugador
        if (col.CompareTag("Player"))
        {
            // Si el jugador no estaba en la lista, lo agregamos
            if (!jugadoresDentro.Contains(col))
            {
                jugadoresDentro.Add(col);
                Debug.Log("Jugador entró. Faltan: " + (jugadoresRequeridos - jugadoresDentro.Count));
            }

            // Si ya están todos los requeridos, ¡cerramos las puertas!
            if (jugadoresDentro.Count >= jugadoresRequeridos)
            {
                CerrarMuros();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (arenaActivada) return;

        // Si un jugador entra y luego se arrepiente y sale antes de que se cierre, lo borramos de la lista
        if (col.CompareTag("Player"))
        {
            if (jugadoresDentro.Contains(col))
            {
                jugadoresDentro.Remove(col);
            }
        }
    }

    private void CerrarMuros()
    {
        arenaActivada = true;
        
        if (muroIzquierdo != null) muroIzquierdo.SetActive(true);
        if (muroDerecho != null) muroDerecho.SetActive(true);

        Debug.Log("¡FIGHT! Muros cerrados y bloqueados.");
        
        // Aquí puedes agregar la línea para que empiecen a salir tus OVNIS o enemigos
    }
}