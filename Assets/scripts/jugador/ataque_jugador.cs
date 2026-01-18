using UnityEngine;
using UnityEngine.InputSystem;

public class ataque_jugador : MonoBehaviour
{
    public float daño = 10f;
    public float rango = 1.5f;

    public GameObject hitEffectPrefab; // HIT SPARK

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("[ATAQUE] ENTER presionado");
            Golpear();
        }
    }

    void Golpear()
    {
        Debug.Log("[ATAQUE] Buscando enemigos...");

        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, rango);
        Debug.Log("[ATAQUE] Colliders detectados: " + enemigos.Length);

        foreach (Collider2D col in enemigos)
        {
            Debug.Log("[ATAQUE] Collider encontrado: " + col.name);

            enemigo_base enemigo = col.GetComponent<enemigo_base>();

            if (enemigo == null)
            {
                Debug.Log("[ATAQUE] No es enemigo_base");
                continue;
            }

            Debug.Log("[ATAQUE] Enemigo válido: " + col.name);

            // POSICIÓN DEL IMPACTO
            Vector2 hitPos = col.ClosestPoint(transform.position);
            Debug.Log("[ATAQUE] HitPos: " + hitPos);

            // HIT EFFECT
            if (hitEffectPrefab != null)
            {
                GameObject hit = Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
                Debug.Log("[ATAQUE] HitEffect instanciado: " + hit.name);
            }
            else
            {
                Debug.LogError("[ATAQUE] hitEffectPrefab NO asignado");
            }

            // DAÑO
            Debug.Log("[ATAQUE] Aplicando daño: " + daño);
            enemigo.RecibirDaño(daño);

            break; // solo un enemigo
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rango);
    }
}
