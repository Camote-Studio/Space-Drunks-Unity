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

    private void Awake()
    {
        playerAnim = GetComponentInParent<Player2Animation>();
        if (weaponAnim == null)
            weaponAnim = GetComponent<WeaponPlayer2Animation>();
        playerRoot = playerAnim != null ? playerAnim.transform : transform.root;
        Shutdown();
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
        switch (current)
        {
            case 0: visual = punchRight; break;
            case 1: visual = punchMiddle; break;
            default: visual = punchLeft; break;
        }

        comboIndex = (comboIndex + 1) % 3;

        Shutdown();

        if (visual != null)
        {
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


    private void Shutdown()
    {
        if (punchRight != null) punchRight.SetActive(false);
        if (punchMiddle != null) punchMiddle.SetActive(false);
        if (punchLeft != null) punchLeft.SetActive(false);
    }
}
