using UnityEngine;
using UnityEngine.InputSystem;

public class ataque_jugador : MonoBehaviour
{
    public float daño = 10f;
    public float rango = 1.5f;

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Golpear();
        }
    }

    void Golpear()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(
            transform.position,
            rango
        );

        foreach (Collider2D col in enemigos)
        {
            enemigo_base enemigo = col.GetComponent<enemigo_base>();

            if (enemigo != null)
            {
                enemigo.RecibirDaño(daño);
            }
        }
    }

    // (Opcional) Ver el rango en escena
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rango);
    }
}
