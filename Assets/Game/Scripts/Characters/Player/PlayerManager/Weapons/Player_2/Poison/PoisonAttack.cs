using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PoisonAttack : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float travelDuration = 0.35f;
    [SerializeField] private float travelDistance = 3f;
    [SerializeField] private float arcHeight = 0.7f;
    [SerializeField] private float maxLifetime = 25f;

    [Header("Area y daño")]
    [SerializeField] private float armTime = 0.6f;
    [SerializeField] private float areaRadius = 2f;
    [SerializeField] private float damagePerSecond = 8f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Visuales")]
    [SerializeField] private Transform flyingVisual;
    [SerializeField] private Transform groundVisual;

    private Rigidbody2D rb;
    private Collider2D col;

    private float lifeTimer;
    private float travelTimer;
    private float armTimer;

    private bool isMoving;
    private bool isArmed;
    private bool hasLanded;

    private Vector2 moveDir;
    private Vector3 startPos;

    private Vector3 baseFlyingLocalPos;
    private Vector3 baseGroundLocalPos;

    private Collider2D[] overlap = new Collider2D[32];
    private readonly HashSet<enemigo_base> uniqueEnemies = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.isTrigger = true;

        if (flyingVisual != null) baseFlyingLocalPos = flyingVisual.localPosition;
        if (groundVisual != null) baseGroundLocalPos = groundVisual.localPosition;

        SetVisualState(true);
    }

    public void Launch(Vector2 dir)
    {
        Launch(dir, transform.position);
    }

    public void Launch(Vector2 dir, Vector3 worldStartPos)
    {
        transform.position = worldStartPos;
        startPos = worldStartPos;

        moveDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;

        lifeTimer = 0f;
        travelTimer = 0f;
        armTimer = 0f;

        isMoving = true;
        hasLanded = false;
        isArmed = false;

        if (flyingVisual != null) flyingVisual.localPosition = baseFlyingLocalPos;
        if (groundVisual != null) groundVisual.localPosition = baseGroundLocalPos;

        SetVisualState(true);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        lifeTimer += dt;

        if (isMoving)
        {
            travelTimer += dt;
            float t = Mathf.Clamp01(travelTimer / travelDuration);

            Vector3 planePos = startPos + (Vector3)(moveDir * (travelDistance * t));
            float h = 4f * arcHeight * t * (1f - t);

            transform.position = planePos;

            if (flyingVisual != null)
                flyingVisual.localPosition = baseFlyingLocalPos + Vector3.up * h;

            if (t >= 1f)
            {
                isMoving = false;
                hasLanded = true;

                if (flyingVisual != null)
                    flyingVisual.localPosition = baseFlyingLocalPos;

                SetVisualState(false);
            }
        }

        if (hasLanded && !isArmed)
        {
            armTimer += dt;
            if (armTimer >= armTime)
                isArmed = true;
        }

        if (isArmed)
            ApplyDamage(dt);

        if (lifeTimer >= maxLifetime)
            Destroy(gameObject);
    }

    private void ApplyDamage(float dt)
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, areaRadius, overlap, enemyMask);

        if (count == overlap.Length)
        {
            overlap = new Collider2D[overlap.Length * 2];
            count = Physics2D.OverlapCircleNonAlloc(transform.position, areaRadius, overlap, enemyMask);
        }

        uniqueEnemies.Clear();

        for (int i = 0; i < count; i++)
        {
            Collider2D c = overlap[i];
            if (c == null) continue;

            enemigo_base enemy = c.GetComponentInParent<enemigo_base>();
            if (enemy != null)
                uniqueEnemies.Add(enemy);
        }

        foreach (var enemy in uniqueEnemies)
        {
            if (enemy != null)
                enemy.RecibirDaño(damagePerSecond * dt);
        }
    }

    private void SetVisualState(bool flying)
    {
        if (flyingVisual != null) flyingVisual.gameObject.SetActive(flying);
        if (groundVisual != null) groundVisual.gameObject.SetActive(!flying);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}
