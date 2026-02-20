using UnityEngine;
using UnityEngine.Rendering;

public class UltimateWeapon : WeaponBase
{
    [Header("Spawn Points")]
    [SerializeField] private Transform firePointSmall;
    [SerializeField] private Transform firePointBig;

    [SerializeField] private GameObject smallBulletPrefab;
    [SerializeField] private GameObject bigBulletPrefab;
    [SerializeField] private GameObject finalBeamPrefab;

    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Pattern")]
    [SerializeField] private int totalShots = 120;
    [SerializeField] private int bigShotEvery = 6;
    [SerializeField] private float coneY = 0.7f;

    [Header("Timing")]
    [SerializeField] private float setupTime = 0.15f;
    [SerializeField] private float shotInterval = 0.025f;
    [SerializeField] private float endLag = 0.2f;

    private bool active;
    private int shotsFired;
    private float timer;
    private float setupTimer;
    private float endTimer;

    private float lockedFacing;
    private PlayerMovement movement;
    private Rigidbody2D rb;

    public override bool IsActive => active;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        rb = movement.GetComponent<Rigidbody2D>();
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

        rb.linearVelocity = Vector2.zero;

        if (setupTimer > 0)
        {
            setupTimer -= Time.deltaTime;
            return;
        }

        if (shotsFired >= totalShots)
        {
            if (endTimer <= 0)
            {
                FireFinalBeam();
                playerAnimation.SetUltimateStateActive(true);
                endTimer = endLag;
            }
            else
            {
                endTimer -= Time.deltaTime;
                if (endTimer <= 0)
                    End();
            }
            return;
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }

        FireShot(shotsFired);
        shotsFired++;
        timer = shotInterval;
    }
    private void Activate()
    {
        Debug.Log("ULTIMATE ACTIVADO");
        active = true;
        shotsFired = 0;
        setupTimer = setupTime;
        endTimer = 0;
        lockedFacing = Mathf.Sign(movement.FacingX == 0 ? 1 : movement.FacingX);

        movement.enabled = false;
        rb.linearVelocity = Vector2.zero;

        playerAnimation.PlayUltimateStart();
    }

    private void End()
    {
        active = false;
        movement.enabled = true;
        playerAnimation.EndUltimate();
    }
    private void FireShot(int index)
    {
        bool big = (index + 1) % bigShotEvery == 0;

        float t = index / (float)(totalShots - 1);
        float y = Mathf.Lerp(-coneY, coneY, t);

        Vector2 dir = new Vector2(lockedFacing, y).normalized;

        Transform fp = big ? firePointBig : firePointSmall;
        GameObject prefab = big ? bigBulletPrefab : smallBulletPrefab;

        GameObject go = Instantiate(prefab, fp.position, Quaternion.identity);

        if (go.TryGetComponent(out Bullet b))
            b.Initialize(dir);
    }

    private void FireFinalBeam()
    {
        GameObject go = Instantiate(finalBeamPrefab, firePointBig.position, Quaternion.identity);
        if (go.TryGetComponent(out Bullet b))
            b.Initialize(new Vector2(lockedFacing, 0));
    }
}
