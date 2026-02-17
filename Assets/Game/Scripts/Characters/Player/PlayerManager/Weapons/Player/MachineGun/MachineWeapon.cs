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

    [Header("Dirección (solo izquierda/derecha)")]
    [SerializeField] private float aimDeadzone = 0.2f;
    [SerializeField] private bool useGamepadAim = false;
    [SerializeField] private string aimHorizontalAxis = "AimHorizontal";

    private bool isActive;
    private bool isSettingUp;
    private float activeTimer;
    private float cooldownTimer;
    private float setupTimer;

    private int bulletsLeftInBurst;
    private float shotTimer;
    private bool burstInProgress;

    private float lastDirX = 1f;
    private Camera cam;

    public bool IsActive => isActive;
    public bool CanActivate => !isActive && cooldownTimer <= 0f;

    private void Awake()
    {
        cam = Camera.main;

        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();

        // ✅ Asegura que use el firePoint del MISMO arma (MachineVisual)
        if (firePoint == null)
        {
            // 1) Busca primero en hijos directos del arma
            Transform fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
            else
            {
                // 2) Busca por nombre en TODO el subárbol del arma
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

        if (firePoint == null)
            Debug.LogWarning("[MachineWeapon] No encontró FirePoint en este arma. Asigna firePoint en el Inspector o crea un hijo llamado 'FirePoint'.");
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
            if (fireDown && CanActivate) TryActivate();
            else return;
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

        Vector2 dir = GetHorizontalAimDirection();
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

    private Vector2 GetHorizontalAimDirection()
    {
        float x;

        if (useGamepadAim)
        {
            x = Input.GetAxis(aimHorizontalAxis);
        }
        else
        {
            x = 0f;
            if (cam != null && firePoint != null)
            {
                Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                x = mouseWorld.x - firePoint.position.x;
            }
        }

        if (Mathf.Abs(x) < aimDeadzone)
            x = lastDirX;
        else
            x = Mathf.Sign(x);

        lastDirX = x;
        return new Vector2(x, 0f);
    }

    private void FireOnce(Vector2 dir)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // ✅ Instancia desde el firePoint del MachineVisual
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, rot);

        if (bulletGO.TryGetComponent(out MachineBullet bullet))
            bullet.Initialize(dir);
        else
            Debug.LogWarning("[MachineWeapon] El prefab no tiene MachineBullet.");

        playerAnimation?.PlayMachineShoot();
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
    }
}
