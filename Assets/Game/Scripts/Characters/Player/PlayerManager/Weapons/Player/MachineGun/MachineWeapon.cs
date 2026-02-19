using UnityEngine;

public class MachineWeapon : WeaponBase
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Máquina")]
    [SerializeField] private float activeDuration = 18f;
    [SerializeField] private float cooldownDuration = 24f;
    [SerializeField] private float setupDuration = 1.0f;

    [Header("Disparo")]
    [SerializeField] private int bulletsPerBurst = 8;
    [SerializeField] private float timeBetweenBullets = 0.1f;
    [SerializeField] private float burstCooldown = 0.5f;

    private bool isActive;
    private bool isSettingUp;
    private float activeTimer;
    private float cooldownTimer;
    private float setupTimer;

    private int bulletsLeftInBurst;
    private float shotTimer;
    private bool burstInProgress;

    private float lastFacingX = 1f;

    private PlayerMovement movement;
    private Camera cam;

    public override bool IsActive => isActive;
    public bool CanActivate => !isActive && cooldownTimer <= 0f;

    private void Awake()
    {
        cam = Camera.main;
        movement = GetComponentInParent<PlayerMovement>();

        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();

        if (firePoint == null)
        {
            Transform fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
            else
            {
                foreach (Transform t in GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "FirePoint")
                    {
                        firePoint = t;
                        break;
                    }
                }
            }
        }
    }

    public void TryActivate()
    {
        if (!CanActivate) return;

        isActive = true;
        isSettingUp = true;
        setupTimer = setupDuration;

        activeTimer = activeDuration;
        bulletsLeftInBurst = 0;
        shotTimer = 0f;
        burstInProgress = false;

        playerAnimation?.PlayMachineStart();
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!isActive)
        {
            if (fireDown)
                TryActivate();

            return;
        }

        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            EndMachine();
            return;
        }

        if (isSettingUp)
        {
            setupTimer -= Time.deltaTime;
            if (setupTimer <= 0f) isSettingUp = false;
            else return;
        }

        if (shotTimer > 0f)
        {
            shotTimer -= Time.deltaTime;
            return;
        }

        if (!burstInProgress)
        {
            if (!fireDown && !fireHeld) return;
            bulletsLeftInBurst = bulletsPerBurst;
            burstInProgress = true;
        }

        Vector2 dir = GetFacingDirection();
        FireOnce(dir);

        bulletsLeftInBurst--;
        shotTimer = (bulletsLeftInBurst > 0) ? timeBetweenBullets : burstCooldown;
        if (bulletsLeftInBurst <= 0) burstInProgress = false;
    }

    private void EndMachine()
    {
        isActive = false;
        isSettingUp = false;
        cooldownTimer = cooldownDuration;
        bulletsLeftInBurst = 0;
        burstInProgress = false;

        playerAnimation?.PlayMachineEnd();
    }

    private Vector2 GetFacingDirection()
    {
        float facingX = lastFacingX;

        if (movement != null)
            facingX = Mathf.Sign(movement.FacingX == 0f ? lastFacingX : movement.FacingX);

        lastFacingX = facingX;
        return new Vector2(facingX, 0f);
    }

    private void FireOnce(Vector2 dir)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        if (bulletGO.TryGetComponent(out MachineBullet bullet))
            bullet.Initialize(dir);

        playerAnimation?.PlayMachineShoot();
    }
}
