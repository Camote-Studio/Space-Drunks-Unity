using UnityEngine;

public class GunWeapon : WeaponBase
{
    [Header("Refs")]
    [SerializeField] private Transform visual;       
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Normal Bullet")]
    [SerializeField] private int maxBullets = 20;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private float reloadTime = 1.2f;

    [Header("Big Bullet")]
    [SerializeField] private int maxBigBullets = 8;
    [SerializeField] private float bigChargeDuration = 1.2f;
    [SerializeField] private float fireBigInterval = 0.6f;
    [SerializeField] private float reloadBigTime = 6f;

    private int normalBullets;
    private int bigBullets;
    private float normalCooldown;
    private float bigCooldown;

    private bool isReloadingNormal;
    private float reloadNormalTimer;

    private bool isReloadingBig;
    private float reloadBigTimer;

    private bool isChargingBig;
    private float bigChargeTimer;

    private void Awake()
    {
        normalBullets = maxBullets;
        bigBullets = maxBigBullets;
    }

    public override void OnSelected()
    {
        base.OnSelected();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        isChargingBig = false;
        bigChargeTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (normalCooldown > 0f) normalCooldown -= Time.deltaTime;
        if (bigCooldown > 0f) bigCooldown -= Time.deltaTime;

        HandleNormalReload();
        HandleBigReload();

        if (isReloadingNormal && normalBullets <= 0)
            return;

        HandleFireInput(fireDown, fireHeld, fireUp);
    }

    private void HandleNormalReload()
    {
        if (isReloadingNormal)
        {
            reloadNormalTimer -= Time.deltaTime;
            if (reloadNormalTimer <= 0f)
            {
                isReloadingNormal = false;
                normalBullets = maxBullets;
            }
        }
        else if (normalBullets <= 0)
        {
            isReloadingNormal = true;
            reloadNormalTimer = reloadTime;
        }
    }

    private void HandleBigReload()
    {
        if (isReloadingBig)
        {
            reloadBigTimer -= Time.deltaTime;
            if (reloadBigTimer <= 0f)
            {
                isReloadingBig = false;
                bigBullets = maxBigBullets;
            }
        }
        else if (bigBullets <= 0)
        {
            isReloadingBig = true;
            reloadBigTimer = reloadBigTime;
        }
    }

    private void HandleFireInput(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (fireDown)
        {
            isChargingBig = true;
            bigChargeTimer = 0f;
        }

        if (isChargingBig && fireHeld)
            bigChargeTimer += Time.deltaTime;

        if (isChargingBig && fireUp)
        {
            if (bigChargeTimer >= bigChargeDuration &&
                bigBullets > 0 &&
                bigCooldown <= 0f)
            {
                FireBig();
            }
            else if (normalBullets > 0 && normalCooldown <= 0f)
            {
                FireNormal();
            }

            isChargingBig = false;
            bigChargeTimer = 0f;
        }
    }

    private void FireNormal()
    {
        SpawnBullet(false);
        normalBullets--;
        normalCooldown = fireInterval;
    }

    private void FireBig()
    {
        SpawnBullet(true);
        bigBullets--;
        bigCooldown = fireBigInterval;
    }

    private void SpawnBullet(bool isBig)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        float facingX = 1f;
        if (visual != null && Mathf.Abs(visual.localScale.x) > 0.01f)
            facingX = Mathf.Sign(visual.localScale.x);

        Vector2 dir = new Vector2(facingX, 0f);

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(dir, isBig);
    }
}
