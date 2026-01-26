using UnityEngine;

public class BackAttackWeapon : WeaponBase
{
    [Header("Visual + Hitbox")]
    [SerializeField] private GameObject backVisual;
    [SerializeField] private Collider2D backHitbox;

    [Header("Config")]
    [SerializeField] private float activeTime = 0.4f;
    [SerializeField] private float cooldownTime = 2f;

    private float activeTimer;
    private float cooldownTimer;

    private Player2Animation playerAnim;

    private void Awake()
    {
        playerAnim = GetComponentInParent<Player2Animation>();

        if (backVisual != null) backVisual.SetActive(false);
        if (backHitbox != null) backHitbox.enabled = false;
    }

    public override void OnSelected()
    {
        if (backVisual != null) backVisual.SetActive(false);
        if (backHitbox != null) backHitbox.enabled = false;

        activeTimer = 0f;
        cooldownTimer = 0f;
    }

    public override void OnDeselected()
    {
        if (backVisual != null) backVisual.SetActive(false);
        if (backHitbox != null) backHitbox.enabled = false;

        activeTimer = 0f;
        cooldownTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (activeTimer > 0f)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                if (backVisual != null) backVisual.SetActive(false);
                if (backHitbox != null) backHitbox.enabled = false;
            }
        }

        if (!fireDown || cooldownTimer > 0f)
            return;

        DoBackAttack();
    }

    private void DoBackAttack()
    {
        Debug.Log("BackAttackWeapon: BACK ATTACK");

        if (backVisual != null) backVisual.SetActive(true);
        if (backHitbox != null) backHitbox.enabled = true;

        activeTimer = activeTime;
        cooldownTimer = cooldownTime;

        playerAnim?.PlayBackAttack();
    }
}
