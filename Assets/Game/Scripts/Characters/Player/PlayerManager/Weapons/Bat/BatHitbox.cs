using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BatHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        enemigo_base enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy != null)
        {
            enemy.RecibirDaño(damage);
            Debug.Log($"Bat hit at {other.name} by {damage} damage");
        }
    }
}
