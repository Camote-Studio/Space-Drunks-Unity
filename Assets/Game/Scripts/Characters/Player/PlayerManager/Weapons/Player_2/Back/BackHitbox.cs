using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 15f;
    [SerializeField] private float knockbackForce = 6f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy == null) return;

        enemy.RecibirDaño(damage);

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        Debug.Log($"BackHitbox: hit {enemy.name} for {damage}");
    }
}
