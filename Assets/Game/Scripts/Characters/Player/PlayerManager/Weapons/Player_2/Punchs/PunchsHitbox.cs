using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PunchsHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private readonly HashSet<enemigo_base> hitThisSwing = new();

    private void OnEnable()
    {
        hitThisSwing.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy == null) return;

        if (hitThisSwing.Contains(enemy)) return;
        hitThisSwing.Add(enemy);

        enemy.RecibirDaño(damage);
        Debug.Log($"PunchsHitbox: hit {enemy.name} for {damage}");
    }
}
