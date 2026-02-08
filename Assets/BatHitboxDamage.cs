using UnityEngine;

public class BatHitboxDamage : MonoBehaviour
{
    public float daño = 10f;
    public GameObject hitEffectPrefab;

    private void OnTriggerEnter2D(Collider2D other)
    {
        enemigo_base enemigo = other.GetComponent<enemigo_base>();
        if (enemigo == null) return;

        Vector2 hitPos = other.ClosestPoint(transform.position);

        if (hitEffectPrefab != null)
            Debug.Log("se instancio el objeto");
            Instantiate(hitEffectPrefab, hitPos, Quaternion.identity);

        enemigo.RecibirDaño(daño);
    }
}
