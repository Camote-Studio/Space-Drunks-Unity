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
            Debug.Log($"Bate peg� a {other.name} por {damage} de da�o");
        }
    }
}
