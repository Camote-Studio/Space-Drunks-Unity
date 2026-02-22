using UnityEngine;

public class UltimateWeapon : UltimateBase
{
    [Header("Spawn Points")]
    [SerializeField] private Transform firePointSmall;
    [SerializeField] private Transform firePointBig;

    [Header("Prefabs")]
    [SerializeField] private GameObject smallBulletPrefab;
    [SerializeField] private GameObject bigBulletPrefab;
    [SerializeField] private GameObject finalBeamPrefab;

    [Header("Anim")]
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Pattern")]
    [SerializeField] private int totalShots = 120;
    [SerializeField] private int bigShotEvery = 6;

    [Header("Timing")]
    [SerializeField] private float shotInterval = 0.025f;
    [SerializeField] private float endLag = 0.2f;

    [Header("Presentation")]
    [SerializeField] private float presentationTimeDefault = 0.7f;

    [Header("Charge System")]
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private UltiBar ultiBar;

    private float currentCharge;

    private int shotsFired;
    private float shotTimer;
    private float endTimer;
    private bool firedFinal;

    protected override void Awake()
    {
        base.Awake();

        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();

        currentCharge = 0f;
        UpdateBar();
    }

    public void AddCharge(float amount)
    {
        if (amount <= 0f) return;

        currentCharge += amount;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);

        UpdateBar();
    }

    public bool IsFull() => currentCharge >= maxCharge;

    public bool TryActivateUltimate()
    {
        if (!IsFull())
            return false;

        StartUltimate();
        return true;
    }

    protected override void StartUltimate()
    {
        if (!IsFull())
            return;

        currentCharge = 0f;
        UpdateBar();

        presentationTime = presentationTimeDefault;

        shotsFired = 0;
        shotTimer = 0f;
        endTimer = 0f;
        firedFinal = false;

        base.StartUltimate();
    }

    protected override void OnUltimateStart()
    {
        playerAnimation?.PlayUltimateStart();
    }

    protected override void OnUltimateExecuteStart()
    {
        playerAnimation?.SetUltimateStateActive(true);
    }

    protected override void TickExecute()
    {
        if (shotsFired < totalShots)
        {
            if (shotTimer > 0f)
            {
                shotTimer -= Time.deltaTime;
                return;
            }

            FireShot(shotsFired);
            shotsFired++;
            shotTimer = shotInterval;
            return;
        }

        if (!firedFinal)
        {
            FireFinalBeam();
            firedFinal = true;
            endTimer = endLag;
            return;
        }

        endTimer -= Time.deltaTime;
        if (endTimer <= 0f)
            EndUltimate();
    }

    protected override void OnUltimateEnd()
    {
        playerAnimation?.EndUltimate();
    }

    private void UpdateBar()
    {
        if (ultiBar != null)
            ultiBar.SetRaw(currentCharge, maxCharge);
    }

    private Vector3 GetMirroredSpawnPosition(Transform fp)
    {
        if (fp == null) return transform.position;

        Transform root = transform;
        Vector3 localOffset = root.InverseTransformPoint(fp.position);
        localOffset.x *= lockedFacing;
        return root.TransformPoint(localOffset);
    }

    private void FireShot(int index)
    {
        bool big = bigShotEvery > 0 && ((index + 1) % bigShotEvery == 0);

        Vector2 dir = new Vector2(lockedFacing, 0f).normalized;

        Transform fp = big ? firePointBig : firePointSmall;
        GameObject prefab = big ? bigBulletPrefab : smallBulletPrefab;

        if (fp == null || prefab == null)
            return;

        Vector3 spawnPos = GetMirroredSpawnPosition(fp);

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);

        MachineBullet b = go.GetComponentInChildren<MachineBullet>();
        if (b != null)
            b.Initialize(dir);
    }

    private void FireFinalBeam()
    {
        if (finalBeamPrefab == null)
            return;

        Transform fp = firePointBig != null ? firePointBig : firePointSmall;
        if (fp == null)
            return;

        Vector3 spawnPos = GetMirroredSpawnPosition(fp);

        GameObject go = Instantiate(finalBeamPrefab, spawnPos, Quaternion.identity);

        MachineBullet b = go.GetComponentInChildren<MachineBullet>();
        if (b != null)
            b.Initialize(new Vector2(lockedFacing, 0f));
    }
}