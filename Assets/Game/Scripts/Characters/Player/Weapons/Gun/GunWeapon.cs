using UnityEngine;

public class GunWeapon : WeaponBase
{
    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject gunVisual;

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

    private PlayerMovement movement;
    private PlayerAnimation playerAnim;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        playerAnim = GetComponentInParent<PlayerAnimation>();

        normalBullets = maxBullets;
        bigBullets = maxBigBullets;
    }

    public override void OnSelected()
    {
        if (gunVisual != null) gunVisual.SetActive(true);
        isChargingBig = false;
        bigChargeTimer = 0f;
    }

    public override void OnDeselected()
    {
        if (gunVisual != null) gunVisual.SetActive(false);
        isChargingBig = false;
        bigChargeTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {

        if (normalCooldown > 0f) normalCooldown -= Time.deltaTime;
        if (bigCooldown > 0f) bigCooldown -= Time.deltaTime;

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

        if (fireDown)
        {
            isChargingBig = true;
            bigChargeTimer = 0f;
        }

        if (isChargingBig && fireHeld)
        {
            bigChargeTimer += Time.deltaTime;
        }

        if (isChargingBig && fireUp)
        {
            bool fired = false;

            if (bigChargeTimer >= bigChargeDuration &&
                bigBullets > 0 &&
                bigCooldown <= 0f &&
                !isReloadingBig)
            {
                FireBig();
                fired = true;
            }
            else if (normalBullets > 0 &&
                     normalCooldown <= 0f &&
                     !isReloadingNormal)
            {
                FireNormal();
                fired = true;
            }

            if (fired)
                playerAnim?.PlayShoot();  

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
        if (bulletPrefab == null || firePoint == null) return;

        Transform body = movement != null ? movement.transform : transform;
        float facingX = 1f;
        if (body != null && body.localScale.x != 0f)
            facingX = Mathf.Sign(body.localScale.x);

        Vector2 dir = new Vector2(facingX, 0f);

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(dir, isBig);
    }
}
