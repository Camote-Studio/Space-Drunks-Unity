using UnityEngine;
using UnityEngine.InputSystem;

public class ataque_jugador : MonoBehaviour
{
    public float daño = 10f;
    public float rango = 1.5f;

    public GameObject hitEffectPrefab; // HIT SPARK

    void Golpear()
    {
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, rango);
        foreach (Collider2D col in enemigos)
        {
            enemigo_base enemigo = col.GetComponent<enemigo_base>();
            if (enemigo == null)
            {
                continue;
            }
            Vector2 hitPos = col.ClosestPoint(transform.position);
            if (hitEffectPrefab != null)
            {
                GameObject hit = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
            }     
            // DAÑO
            enemigo.RecibirDaño(daño);

            break; // solo un enemigo
        }
    }

}
