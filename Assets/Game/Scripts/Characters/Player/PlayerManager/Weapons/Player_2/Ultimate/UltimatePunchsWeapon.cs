using UnityEngine;

public class UltimatePunchsWeapon : UltimateBase
{
    [Header("Punch Points")]
    [SerializeField] private Transform[] punchPoints;

    [Header("Hitbox")]
    [SerializeField] private GameObject punchHitbox;

    [Header("Config")]
    [SerializeField] private float punchActiveTime = 0.12f;
    [SerializeField] private float timeBetweenPunches = 0.12f;

    [Header("Ultimate Duration")]
    [SerializeField] private float executeDuration = 5f;

    [Header("Presentation")]
    [SerializeField] private float presentationTimeDefault = 0.7f;

    [Header("Anim")]
    [SerializeField] private Player2Animation playerAnim;
    [SerializeField] private WeaponPlayer2Animation weaponAnim;

    [Header("Charge System")]
    [SerializeField] private float maxCharge = 100f;
    [SerializeField] private UltiBar ultiBar;

    private float currentCharge;

    private float punchTimer;
    private float betweenTimer;
    private float executeTimer;
    private int index;
    private bool punchActive;

    protected override void Awake()
    {
        base.Awake();

        if (playerAnim == null)
            playerAnim = GetComponentInParent<Player2Animation>();

        if (punchHitbox != null)
            punchHitbox.SetActive(false);

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

        punchTimer = 0f;
        betweenTimer = 0f;
        executeTimer = executeDuration;

        index = 0;
        punchActive = false;

        if (punchHitbox != null)
            punchHitbox.SetActive(false);

        base.StartUltimate();
    }

    protected override void OnUltimateStart()
    {
        playerAnim?.PlayUltimateStart();
    }

    protected override void OnUltimateExecuteStart()
    {
        playerAnim?.SetUltimateStateActive(true);
    }

    protected override void TickExecute()
    {
        executeTimer -= Time.deltaTime;
        if (executeTimer <= 0f)
        {
            EndUltimate();
            return;
        }

        if (punchPoints == null || punchPoints.Length == 0 || punchHitbox == null)
            return;

        if (punchActive)
        {
            punchTimer -= Time.deltaTime;
            if (punchTimer <= 0f)
            {
                punchHitbox.SetActive(false);
                punchActive = false;
                betweenTimer = timeBetweenPunches;
            }
            return;
        }

        if (betweenTimer > 0f)
        {
            betweenTimer -= Time.deltaTime;
            return;
        }

        if (index >= punchPoints.Length)
            index = 0;

        Transform p = punchPoints[index];

        Transform root = transform;
        Vector3 localOffset = root.InverseTransformPoint(p.position);
        localOffset.x *= lockedFacing;
        Vector3 worldPos = root.TransformPoint(localOffset);

        punchHitbox.transform.position = worldPos;

        Vector3 scale = punchHitbox.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * lockedFacing;
        punchHitbox.transform.localScale = scale;

        punchHitbox.SetActive(true);

        punchActive = true;
        punchTimer = punchActiveTime;

        weaponAnim?.PlayUltimatePunch(index);
        playerAnim?.PlayUltimatePunch();

        index++;
    }

    protected override void OnUltimateEnd()
    {
        if (punchHitbox != null)
            punchHitbox.SetActive(false);

        playerAnim?.EndUltimate();
    }

    private void UpdateBar()
    {
        if (ultiBar != null)
            ultiBar.SetRaw(currentCharge, maxCharge);
    }
}