using UnityEngine;

public class MachineWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerMovement movement;

    [Header("Lógica de máquina")]
    [SerializeField] private float activeDuration = 18f;
    [SerializeField] private float cooldownDuration = 24f;

    [Header("Disparo")]
    [SerializeField] private int bulletsPerBurst = 8;
    [SerializeField] private float timeBetweenBullets = 0.2f;
    [SerializeField] private float burstCooldown = 1.6f;

    [Header("Apuntado")]
    [SerializeField] private bool useGamepadAim = false;
    [SerializeField] private string aimHorizontalAxis = "AimHorizontal";
    [SerializeField] private string aimVerticalAxis = "AimVertical";

    private bool isActive;
    private float activeTimer;
    private float cooldownTimer;

    private int bulletsLeftInBurst;
    private float shotTimer;
    private bool burstInProgress;
    private bool shootRequested;

    private Vector2 lastShootDirection = Vector2.right;

    private void Awake()
    {
        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();
        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();
    }

    public bool IsReady => !isActive && cooldownTimer <= 0f;
    public bool IsActive => isActive;

    public void SetShootInput(bool fireDown)
    {
        if (!isActive)
            return;

        if (fireDown && !burstInProgress)
            shootRequested = true;
    }

    public void TryActivate()
    {
        if (!IsReady)
            return;

        isActive = true;
        activeTimer = activeDuration;
        bulletsLeftInBurst = 0;
        shotTimer = 0f;
        burstInProgress = false;
        shootRequested = false;

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

        if (!burstInProgress)
        {
            if (!shootRequested)
                return;

            if (shotTimer > 0f)
            {
                shotTimer -= Time.deltaTime;
                return;
            }

            bulletsLeftInBurst = bulletsPerBurst;
            burstInProgress = true;
            shootRequested = false;
        }

        if (shotTimer > 0f)
        {
            shotTimer -= Time.deltaTime;
            return;
        }

        Vector2 dir = GetAimDirection();
        FireOnce(dir);
        lastShootDirection = dir;

        bulletsLeftInBurst--;
        if (bulletsLeftInBurst > 0)
        {
            shotTimer = timeBetweenBullets;
        }
        else
        {
            shotTimer = burstCooldown;
            burstInProgress = false;
        }
    }

    private void EndMachine()
    {
        isActive = false;
        cooldownTimer = cooldownDuration;
        bulletsLeftInBurst = 0;
        burstInProgress = false;
        shootRequested = false;

        if (playerAnimation != null)
            playerAnimation.PlayMachineEnd();
    }

    private Vector2 GetAimDirection()
    {
        Vector2 dir = Vector2.zero;

        if (useGamepadAim)
        {
            float ax = Input.GetAxis(aimHorizontalAxis);
            float ay = Input.GetAxis(aimVerticalAxis);
            dir = new Vector2(ax, ay);
        }

        if (dir.sqrMagnitude < 0.01f)
        {
            if (Camera.main != null && firePoint != null)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 to = mouseWorld - firePoint.position;
                to.z = 0f;
                dir = new Vector2(to.x, to.y);
            }
        }

        if (dir.sqrMagnitude < 0.01f)
            dir = lastShootDirection.sqrMagnitude > 0.01f ? lastShootDirection : Vector2.right;

        return dir.normalized;
    }

    private void FireOnce(Vector2 dir)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(dir, false);

        if (playerAnimation != null)
            playerAnimation.PlayMachineShoot();
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
    }
}
