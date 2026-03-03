using UnityEngine;

public class PunchsWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] private GameObject punchRight;
    [SerializeField] private GameObject punchMiddle;
    [SerializeField] private GameObject punchLeft;

    [Header("Config")]
    [SerializeField] private float punchActiveTime = 0.15f;
    [SerializeField] private float punchAttackCooldown = 0.3f;

    [Header("Anim")]
    [SerializeField] private WeaponPlayer2Animation weaponAnim;

    private float punchCooldown;
    private float punchTimer;
    private int comboIndex = 0;

    private Player2Animation playerAnim;
    private Transform playerRoot;
    private SpriteRenderer playerSprite;

    private Vector3 rightLocalOffset;
    private Vector3 middleLocalOffset;
    private Vector3 leftLocalOffset;

    private Vector3 rightBaseScale;
    private Vector3 middleBaseScale;
    private Vector3 leftBaseScale;

    private void Awake()
    {
        playerAnim = GetComponentInParent<Player2Animation>();
        if (weaponAnim == null)
            weaponAnim = GetComponent<WeaponPlayer2Animation>();

        playerRoot = playerAnim != null ? playerAnim.transform : transform.root;

        playerSprite = null;
        if (playerAnim != null) playerSprite = playerAnim.GetComponentInChildren<SpriteRenderer>();
        if (playerSprite == null) playerSprite = GetComponentInParent<SpriteRenderer>();

        CacheBase(punchRight, out rightLocalOffset, out rightBaseScale);
        CacheBase(punchMiddle, out middleLocalOffset, out middleBaseScale);
        CacheBase(punchLeft, out leftLocalOffset, out leftBaseScale);

        Shutdown();
    }

    private void CacheBase(GameObject go, out Vector3 localOffset, out Vector3 baseScale)
    {
        localOffset = Vector3.zero;
        baseScale = Vector3.one;

        if (go == null || playerRoot == null) return;

        localOffset = playerRoot.InverseTransformPoint(go.transform.position);
        baseScale = go.transform.localScale;
    }

    public override void OnSelected()
    {
        Shutdown();
        comboIndex = 0;
        punchCooldown = 0f;
        punchTimer = 0f;
    }

    public override void OnDeselected()
    {
        Shutdown();
        comboIndex = 0;
        punchCooldown = 0f;
        punchTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (playerAnim != null && playerAnim.IsCombatLocked) return;

        if (punchCooldown > 0f)
            punchCooldown -= Time.deltaTime;

        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            if (punchTimer <= 0f)
                Shutdown();
        }

        if (!fireDown || punchCooldown > 0f)
            return;

        int current = comboIndex;

        GameObject visual = null;
        Vector3 localOffset = Vector3.zero;
        Vector3 baseScale = Vector3.one;

        switch (current)
        {
            case 0:
                visual = punchRight;
                localOffset = rightLocalOffset;
                baseScale = rightBaseScale;
                break;
            case 1:
                visual = punchMiddle;
                localOffset = middleLocalOffset;
                baseScale = middleBaseScale;
                break;
            default:
                visual = punchLeft;
                localOffset = leftLocalOffset;
                baseScale = leftBaseScale;
                break;
        }

        comboIndex = (comboIndex + 1) % 3;

        Shutdown();

        if (visual != null)
        {
            int facing = GetFacing();

            Vector3 mirrored = localOffset;
            mirrored.x *= facing;
            visual.transform.position = playerRoot.TransformPoint(mirrored);

            Vector3 s = baseScale;
            s.x = Mathf.Abs(s.x) * facing;
            visual.transform.localScale = s;

            visual.SetActive(true);
            punchTimer = punchActiveTime;
        }

        punchCooldown = punchAttackCooldown;

        playerAnim?.PlayPunchAttack();

        if (weaponAnim != null)
        {
            switch (current)
            {
                case 0: weaponAnim.PlayPunchRight(); break;
                case 1: weaponAnim.PlayPunchMiddle(); break;
                default: weaponAnim.PlayPunchLeft(); break;
            }
        }
    }

    private int GetFacing()
    {
        if (playerSprite != null)
            return playerSprite.flipX ? -1 : 1;

        if (playerRoot != null)
            return playerRoot.localScale.x >= 0f ? 1 : -1;

        return 1;
    }

    private void Shutdown()
    {
        if (punchRight != null) punchRight.SetActive(false);
        if (punchMiddle != null) punchMiddle.SetActive(false);
        if (punchLeft != null) punchLeft.SetActive(false);
    }
}