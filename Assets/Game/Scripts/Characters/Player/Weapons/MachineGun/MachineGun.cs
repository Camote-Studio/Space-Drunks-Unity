using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MachineGun : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask enemyMask;

    [Header("Drop")]
    [SerializeField] private float dropHeight = 6f;
    [SerializeField] private float fallSpeed = 12f;
    [SerializeField] private float groundOffsetY = 0f;

    [Header("Vida / Overheat")]
    [SerializeField] private float maxHp = 40f;
    [SerializeField] private float maxLifetime = 18f;
    [SerializeField] private float idleDrainPerSecond = 0.5f;
    [SerializeField] private float firingDrainPerShot = 1.0f;

    [Header("Disparo")]
    [SerializeField] private float detectionRadius = 14f;
    [SerializeField] private float detectionAngle = 120f;
    [SerializeField] private int bulletsPerBurst = 3;
    [SerializeField] private float timeBetweenBullets = 0.2f;
    [SerializeField] private float burstCooldown = 1.6f;

    public event Action<MachineGun> OnMachineGunEnded;

    private float hp;
    private float lifeRemaining;
    private bool isDropping;
    private bool isActive;
    private Vector2 groundPos;

    private float fireTimer;
    private int bulletsLeftInBurst;

    private Animator animator;

    // Llamado justo después de instanciar la torreta
    public void Init(Vector2 groundPosition)
    {
        animator = GetComponentInChildren<Animator>();

        hp = maxHp;
        lifeRemaining = maxLifetime;

        groundPos = groundPosition;
        isDropping = true;
        isActive = false;

        // la posición X/Y inicial ya la define el prefab al instanciarlo
    }

    private void Start()
    {
        // Por si se instanció sin llamar Init, al menos no crashea
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (lifeRemaining <= 0f)
            lifeRemaining = maxLifetime;
        if (hp <= 0f)
            hp = maxHp;
    }

    private void Update()
    {
        if (isDropping)
        {
            UpdateDrop();
            return;
        }

        if (!isActive)
            return;

        // vida útil / sobrecarga base
        lifeRemaining -= idleDrainPerSecond * Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            lifeRemaining = 0f;
            EndMachine();
            return;
        }

        Transform target = FindTarget();
        if (target != null)
            HandleShooting(target);
        else
            bulletsLeftInBurst = 0;
    }

    private void UpdateDrop()
    {
        Vector3 targetPos = new Vector3(
            groundPos.x,
            groundPos.y + groundOffsetY,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            fallSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) <= 0.05f)
        {
            isDropping = false;
            isActive = true;

            animator?.SetTrigger("Machine_Land");
        }
    }

    private Transform FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            enemyMask
        );

        Transform best = null;
        float bestDistSq = Mathf.Infinity;
        Vector2 forward = Vector2.right; // mira a la derecha

        foreach (var h in hits)
        {
            Vector2 to = (Vector2)h.transform.position - (Vector2)transform.position;
            float angle = Vector2.Angle(forward, to);
            if (angle > detectionAngle * 0.5f)
                continue;

            float dSq = to.sqrMagnitude;
            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                best = h.transform;
            }
        }

        return best;
    }

    private void HandleShooting(Transform target)
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
            return;
        }

        if (bulletsLeftInBurst <= 0)
            bulletsLeftInBurst = bulletsPerBurst;

        FireOnce(target);

        bulletsLeftInBurst--;
        lifeRemaining -= firingDrainPerShot;  // se calienta más al disparar

        if (lifeRemaining <= 0f)
        {
            lifeRemaining = 0f;
            EndMachine();
            return;
        }

        if (bulletsLeftInBurst > 0)
            fireTimer = timeBetweenBullets;
        else
            fireTimer = burstCooldown;
    }

    private void FireOnce(Transform target)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(dir, false);   // bala normal

        animator?.SetTrigger("Machine_Shoot");
    }

    public void RecibirDaño(float cantidad)
    {
        if (!isActive) return;

        hp -= cantidad;
        if (hp <= 0f)
            EndMachine();
    }

    private void EndMachine()
    {
        if (!gameObject.activeInHierarchy)
            return;

        isActive = false;

        animator?.SetTrigger("Machine_Destroy");

        OnMachineGunEnded?.Invoke(this);

        Destroy(gameObject, 0.4f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
