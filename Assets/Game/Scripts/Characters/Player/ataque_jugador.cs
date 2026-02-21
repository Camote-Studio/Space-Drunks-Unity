using UnityEngine;
using UnityEngine.InputSystem;

public class ataque_jugador : MonoBehaviour
{
    public float daño = 10f;
    public float rango = 1.5f;

    public GameObject hitEffectPrefab; // HIT SPARK

    [Header("Filtros de Ataque")]
    [Tooltip("Selecciona aquí la capa (Layer) donde están tus enemigos o sus Hitboxes")]
    public LayerMask capaEnemigos; // <-- ¡NUEVO! Optimización profesional

    void Golpear()
    {
        // 1. ESCANEO OPTIMIZADO: Solo buscamos en la capa de Enemigos, ignorando muros y al propio jugador.
        Collider2D[] enemigos = Physics2D.OverlapCircleAll(transform.position, rango, capaEnemigos);
        
        foreach (Collider2D col in enemigos)
        {
            // 2. REGLA DE HITBOX: Si el colisionador NO es un Trigger (ej. son los pies sólidos), lo ignoramos.
            if (!col.isTrigger)
            {
                continue;
            }

            // 3. BUSCAR EN EL PADRE: Como golpeamos al hijo (Hitbox_Cuerpo), 
            // le decimos a Unity que busque el script de vida en el objeto principal (el padre).
            enemigo_base enemigo = col.GetComponentInParent<enemigo_base>();
            
            // 4. VALIDACIÓN: ¿Encontramos al enemigo? ¿Está vivo?
            if (enemigo == null || enemigo.estaMuerto)
            {
                continue;
            }

            // --- EFECTO VISUAL ---
            Vector2 hitPos = col.ClosestPoint(transform.position);
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);
            }    
            
            // --- APLICAR DAÑO ---
            enemigo.RecibirDaño(daño);

            break; // Solo golpea a un enemigo por ataque
        }
    }
}