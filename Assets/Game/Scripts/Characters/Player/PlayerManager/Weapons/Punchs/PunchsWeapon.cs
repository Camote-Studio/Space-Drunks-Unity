using UnityEngine;

public class PunchsWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] private GameObject punchRightVisual;
    [SerializeField] private GameObject punchLeftVisual;

    [Header("Hitbox")]
    [SerializeField] private GameObject punchRightHitbox;
    [SerializeField] private GameObject punchLeftHitbox;

    [SerializeField] private float punchActiveTime = 0.15f;
    [SerializeField] private float punchAttackCooldown = 0.3f;

    private float punchCooldown;
    private float punchTimer;

    private PlayerAnimation playerAnim;
    private Transform playerRoot;

    private GameObject CurrentVisual => IsFacingRight() ? punchRightVisual : punchLeftVisual;
    private GameObject CurrentHitbox => IsFacingRight() ? punchRightHitbox : punchLeftHitbox;

    private void Awake()
    {
        playerAnim = GetComponentInParent<PlayerAnimation>();
        playerRoot = playerAnim != null ? playerAnim.transform : transform.root;
    }

    private bool IsFacingRight()
    {
        return playerRoot != null ? playerRoot.localScale.x >= 0f : true;
    }

    public override void OnSelected()
    {
        punchRightVisual?.SetActive(false);
        punchLeftVisual?.SetActive(false);
        CurrentVisual?.SetActive(true);
    }

    public override void OnDeselected()
    {
        punchRightVisual?.SetActive(false);
        punchLeftVisual?.SetActive(false);

        punchRightHitbox?.SetActive(false);
        punchLeftHitbox?.SetActive(false);

        punchCooldown = 0f;
        punchTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (punchRightVisual != null || punchLeftVisual != null)
        {
            punchRightVisual?.SetActive(false);
            punchLeftVisual?.SetActive(false);
            CurrentVisual?.SetActive(true);
        }

        if (punchCooldown > 0f)
            punchCooldown -= Time.deltaTime;

        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            if (punchTimer <= 0f)
            {
                punchRightHitbox?.SetActive(false);
                punchLeftHitbox?.SetActive(false);
            }
        }

        if (!fireDown || punchCooldown > 0f)
            return;

        var hitbox = CurrentHitbox;
        if (hitbox != null)
        {
            hitbox.SetActive(true);
            punchTimer = punchActiveTime;
        }

        punchCooldown = punchAttackCooldown;
        playerAnim?.PlayBatAttack(); 
    }
}
