using UnityEngine;

public class BatWeapon : WeaponBase 
{
    [SerializeField] private GameObject batHitbox;
    [SerializeField] private float batActiveTime = 0.15f;
    [SerializeField] private float batAttackCooldown = 0.3f;

    private float batCooldown;
    private float batTimer;

    private void Start()
    {
        if (batHitbox != null)
            batHitbox.SetActive(false);
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

        if (batCooldown > 0f)
            return;

        if (fireDown)
        {
            if (batHitbox != null)
            {
                batHitbox.SetActive(true);
                batTimer = batActiveTime;
            }

            batCooldown = batAttackCooldown;
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (batHitbox != null)
            batHitbox.SetActive(false);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        if (batHitbox != null)
            batHitbox.SetActive(false);
    }
}
 