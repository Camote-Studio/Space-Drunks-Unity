using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MachineGun : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private LayerMask enemyMask;

    [Header("Drop")]
    [SerializeField] private float dropHeight = 6f;     // cuánto arriba aparece
    [SerializeField] private float fallSpeed = 12f;    // velocidad al caer
    [SerializeField] private float groundOffsetY = 0f;  // ajuste fino en Y

    // Exponer DropHeight para que lo use MachineWeapon
    public float DropHeight => dropHeight;

    [Header("Vida / Overheat")]
    [SerializeField] private float maxHp = 40f;  // 35–45
    [SerializeField] private float maxLifetime = 18f;  // vida útil total
    [SerializeField] private float idleDrainPerSecond = 0.5f; // bajada estando idle
    [SerializeField] private float firingDrainPerShot = 1.0f; // bajada extra por disparo

    [Header("Detección")]
    [SerializeField] private float detectionRadius = 14f;
    [SerializeField] private float detectionAngle = 120f; // cono 120°

    [Header("Disparo")]
    [SerializeField] private int bulletsPerBurst = 3;    // 3 balas
    [SerializeField] private float timeBetweenBullets = 0.2f; // dentro de la ráfaga
    [SerializeField] private float burstCooldown = 1.6f; // entre ráfagas

    private float hp;
    private float lifeRemaining;

    private bool isDropping;
    private bool isActive;

    private Vector2 groundPos;

    private float fireTimer;
    private int bulletsLeftInBurst;

    private Animator animator;
    private MachineWeapon owner; // para avisar cuando termine


    public event System.Action<MachineGun> OnTurretEnd;

    void ExplodeOrDie()
    {
        OnTurretEnd?.Invoke(this);
        Destroy(gameObject);
    }

    public void Init(Vector2 groundPosition, MachineWeapon owner)
    {
        this.owner = owner;
        groundPos = groundPosition;

        animator = GetComponentInChildren<Animator>();

        hp = maxHp;
        lifeRemaining = maxLifetime;

        // aparece arriba y cae
        transform.position = new Vector3(
            groundPos.x,
            groundPos.y + dropHeight,
            transform.position.z
        );

        isDropping = true;
        isActive = false;

        fireTimer = 0f;
        bulletsLeftInBurst = 0;

        // si tienes anim de “spawn”
        animator?.SetTrigger("TurretSpawn");
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

        // overheat / vida útil base
        lifeRemaining -= idleDrainPerSecond * Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            lifeRemaining = 0f;
            Explode();
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
            animator?.SetTrigger("TurretLand");
        }
    }

    private Transform FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            enemyMask
        );

        if (hits.Length == 0)
            return null;

        Transform best = null;
        float bestDist = Mathf.Infinity;

        // “forward” = derecha según escala
        float facingX = transform.localScale.x >= 0 ? 1f : -1f;
        Vector2 forward = new Vector2(facingX, 0f);

        foreach (var h in hits)
        {
            Vector2 to = (Vector2)h.transform.position - (Vector2)transform.position;
            float angle = Vector2.Angle(forward, to);

            if (angle > detectionAngle * 0.5f)
                continue;

            float d = to.sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
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
        lifeRemaining -= firingDrainPerShot;

        if (lifeRemaining <= 0f)
        {
            lifeRemaining = 0f;
            Explode();
            return;
        }

        fireTimer = (bulletsLeftInBurst > 0)
            ? timeBetweenBullets
            : burstCooldown;
    }

    private void FireOnce(Transform target)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        // mirar hacia el objetivo
        float dirX = Mathf.Sign(target.position.x - transform.position.x);
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dirX;
        transform.localScale = scale;

        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(dir, false); // bala normal

        animator?.SetTrigger("TurretShoot");
    }

    public void RecibirDaño(float cantidad)
    {
        if (!isActive) return;

        hp -= cantidad;
        if (hp <= 0f)
            Explode();
    }

    private void Explode()
    {
        if (!gameObject.activeInHierarchy)
            return;

        isActive = false;

        animator?.SetTrigger("TurretExplode");

        // avisar al owner para liberar el slot
        owner?.OnMachineGunEnded(this);

        Destroy(gameObject, 0.4f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
