using UnityEngine;

public class MachineWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;       
    [SerializeField] private GameObject bulletPrefab;  
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMovement movement;

    [Header("Lógica de máquina")]
    [SerializeField] private float activeDuration   = 18f; 
    [SerializeField] private float cooldownDuration = 24f; 

    [Header("Disparo automático")]
    [SerializeField] private float detectionRadius = 14f;   
    [SerializeField] private float detectionAngle  = 120f;  
    [SerializeField] private int   bulletsPerBurst = 3;     
    [SerializeField] private float timeBetweenBullets = 0.2f; 
    [SerializeField] private float burstCooldown = 1.6f;     

    private bool  isActive;
    private float activeTimer;
    private float cooldownTimer;

    private int   bulletsLeftInBurst;
    private float shotTimer;

    private void Awake()
    {
        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();

        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();
    }

    public bool IsReady => !isActive && cooldownTimer <= 0f;

    public void TryActivate()
    {
        if (!IsReady) return;

        isActive     = true;
        activeTimer  = activeDuration;
        bulletsLeftInBurst = 0;
        shotTimer    = 0f;

        if (playerAnimation != null)
            playerAnimation.PlayMachineStart();
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!isActive)
            return;

        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            EndMachine();
            return;
        }

        if (shotTimer > 0f)
        {
            shotTimer -= Time.deltaTime;
            return;
        }

        Transform target = FindTarget();
        if (target == null)
        {
            bulletsLeftInBurst = 0; 
            return;
        }

        if (bulletsLeftInBurst <= 0)
            bulletsLeftInBurst = bulletsPerBurst;

        FireOnce(target);

        bulletsLeftInBurst--;

        if (bulletsLeftInBurst > 0)
            shotTimer = timeBetweenBullets; 
        else
            shotTimer = burstCooldown;      
    }

    private void EndMachine()
    {
        isActive = false;
        cooldownTimer = cooldownDuration;
        bulletsLeftInBurst = 0;

        if (playerAnimation != null)
            playerAnimation.PlayMachineEnd();
    }

    private Transform FindTarget()
    {
        if (firePoint == null) return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, detectionRadius);

        Transform best = null;
        float bestDistSq = Mathf.Infinity;

        Vector2 forward = Vector2.right;
        if (movement != null && movement.MoveInput.x < 0f)
            forward = Vector2.left;

        foreach (var h in hits)
        {
            var enemy = h.GetComponentInParent<enemigo_base>();
            if (enemy == null || enemy.estaMuerto) continue;

            Vector2 to = (Vector2)h.transform.position - (Vector2)firePoint.position;
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

    private void FireOnce(Transform target)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Vector2 dir = ((Vector2)target.position - (Vector2)firePoint.position).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {

            bullet.Initialize(dir, false);
        }
        if (playerAnimation != null)
            playerAnimation.PlayMachineShoot();
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(firePoint.position, detectionRadius);
    }
}
