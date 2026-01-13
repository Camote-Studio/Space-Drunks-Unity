using UnityEngine;

public class BatWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] private GameObject batVisual;

    [Header("Hitbox")]
    [SerializeField] private GameObject batHitbox;
    [SerializeField] private float batActiveTime = 0.15f;
    [SerializeField] private float batAttackCooldown = 0.3f;

    private float batCooldown;
    private float batTimer;

    private PlayerAnimation playerAnim;

    private void Awake()
    {
        playerAnim = GetComponentInParent<PlayerAnimation>();

        if (batHitbox != null)
            batHitbox.SetActive(false);
    }

    public override void OnSelected()
    {
        if (batVisual != null)
            batVisual.SetActive(true);
    }

    public override void OnDeselected()
    {
        if (batVisual != null)
            batVisual.SetActive(false);

        if (batHitbox != null)
            batHitbox.SetActive(false);

        batCooldown = 0f;
        batTimer = 0f;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (batCooldown > 0f)
            batCooldown -= Time.deltaTime;

        if (batTimer > 0f)
        {
            batTimer -= Time.deltaTime;
            if (batTimer <= 0f && batHitbox != null)
                batHitbox.SetActive(false);
        }

        if (!fireDown || batCooldown > 0f)
            return;

        // Activamos hitbox un momento
        if (batHitbox != null)
        {
            batHitbox.SetActive(true);
            batTimer = batActiveTime;
        }

        batCooldown = batAttackCooldown;
        playerAnim?.PlayBatAttack();  
    }
}
