using UnityEngine;

public class UltimateWeapon : WeaponBase
{
    [Header("Spawn Points")]
    [SerializeField] private Transform firePointSmall;
    [SerializeField] private Transform firePointBig;

    [Header("Prefabs")]
    [SerializeField] private GameObject smallBulletPrefab;
    [SerializeField] private GameObject bigBulletPrefab;
    [SerializeField] private GameObject finalBeamPrefab;

    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Pattern")]
    [SerializeField] private int totalShots = 120;
    [SerializeField] private int bigShotEvery = 6;
    [SerializeField] private float coneY = 0.7f;

    [Header("Timing")]
    [SerializeField] private float setupTime = 0.8f;
    [SerializeField] private float shotInterval = 0.025f;
    [SerializeField] private float endLag = 0.2f;

    private bool active;
    private bool shooting;

    private int shotsFired;
    private float shotTimer;
    private float setupTimer;
    private float endTimer;

    private float lockedFacing;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    public override bool IsActive => active;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        rb = movement != null ? movement.GetComponent<Rigidbody2D>() : GetComponentInParent<Rigidbody2D>();

        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (!active)
        {
            if (fireDown)
                Activate();
            return;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (!shooting)
        {
            setupTimer -= Time.deltaTime;
            if (setupTimer <= 0f)
            {
                shooting = true;
                playerAnimation?.SetUltimateStateActive(true);
            }
            return;
        }

        if (shotsFired >= totalShots)
        {
            if (endTimer <= 0f)
            {
                FireFinalBeam();
                endTimer = endLag;
            }
            else
            {
                endTimer -= Time.deltaTime;
                if (endTimer <= 0f)
                    End();
            }
            return;
        }

        if (shotTimer > 0f)
        {
            shotTimer -= Time.deltaTime;
            return;
        }

        FireShot(shotsFired);
        shotsFired++;
        shotTimer = shotInterval;
    }

    private void Activate()
    {
        active = true;
        shooting = false;

        shotsFired = 0;
        shotTimer = 0f;
        endTimer = 0f;

        setupTimer = setupTime;

        lockedFacing = Mathf.Sign(movement != null && movement.FacingX != 0f ? movement.FacingX : 1f);

        if (movement != null)
            movement.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        playerAnimation?.PlayUltimateStart();
    }

    private void End()
    {
        active = false;
        shooting = false;

        if (movement != null)
            movement.enabled = true;

        playerAnimation?.EndUltimate();
    }

    private void FireShot(int index)
    {
        bool big = bigShotEvery > 0 && ((index + 1) % bigShotEvery == 0);

        float t = totalShots <= 1 ? 0.5f : index / (float)(totalShots - 1);
        float y = Mathf.Lerp(-coneY, coneY, t);

        Vector2 dir = new Vector2(lockedFacing, y).normalized;

        Transform fp = big ? firePointBig : firePointSmall;
        GameObject prefab = big ? bigBulletPrefab : smallBulletPrefab;

        if (fp == null || prefab == null)
            return;

        GameObject go = Instantiate(prefab, fp.position, Quaternion.identity);

        if (go.TryGetComponent(out Bullet b))
            b.Initialize(dir, big);
    }

    private void FireFinalBeam()
    {
        if (finalBeamPrefab == null)
            return;

        Transform fp = firePointBig != null ? firePointBig : firePointSmall;
        if (fp == null)
            return;

        GameObject go = Instantiate(finalBeamPrefab, fp.position, Quaternion.identity);

        if (go.TryGetComponent(out Bullet b))
            b.Initialize(new Vector2(lockedFacing, 0f), true);
    }
}
