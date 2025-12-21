using UnityEngine;
using UnityEngine.InputSystem;
public class ataque_jugador : MonoBehaviour
{
    public float daño = 20f;
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
            enemigo_pato_1 enemigo = col.GetComponent<enemigo_pato_1>();
            if (enemigo != null)
            {
                // 🔥 AHORA SÍ PASAS EL ORIGEN
                enemigo.RecibirDaño(daño);
            }
        }
    }

}