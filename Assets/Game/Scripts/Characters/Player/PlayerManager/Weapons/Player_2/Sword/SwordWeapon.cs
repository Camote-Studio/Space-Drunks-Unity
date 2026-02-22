using System.Collections;
using UnityEngine;

public class SwordWeapon : WeaponBase
{
    [Header("Visual")]
    [SerializeField] private GameObject swordVisual;

    [Header("Hitbox")]
    [SerializeField] private GameObject swordHitbox;

    [Header("Duración / Cooldown")]
    [SerializeField] private float swordActiveTime = 15f;
    [SerializeField] private float swordAttackCooldown = 30f;

    [Header("Ataque")]
    [SerializeField] private float swingCooldown = 0.35f;
    [SerializeField] private float hitboxActiveTime = 0.12f;

    [Header("Bloqueo de otras armas")]
    [SerializeField] private WeaponBase[] weaponsToBlock;

    private bool isEquipped;
    private float cooldownUntil;
    private float swingUntil;

    private Coroutine durationRoutine;
    private Coroutine hitboxRoutine;

    private Player2Animation player2Anim;

    private void Awake()
    {
        player2Anim = GetComponentInParent<Player2Animation>(true);

        SetVisual(false);
        SetHitbox(false);

        player2Anim?.SetSwordEquipped(false);
        player2Anim?.SetCombatLocked(false);
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (!fireDown) return;

        if (!isEquipped)
        {
            if (Time.time < cooldownUntil) return;
            StartSword();
            return;
        }

        if (Time.time < swingUntil) return;
        swingUntil = Time.time + swingCooldown;

        player2Anim?.SetCombatLocked(false);
        player2Anim?.DoSwordAttack();

        if (hitboxRoutine != null) StopCoroutine(hitboxRoutine);
        hitboxRoutine = StartCoroutine(HitboxWindow());
    }

    private void StartSword()
    {
        isEquipped = true;
        swingUntil = 0f;

        BlockOtherWeapons(true);

        player2Anim?.SetCombatLocked(false);
        player2Anim?.SetSwordEquipped(true);

        SetVisual(true);

        if (durationRoutine != null) StopCoroutine(durationRoutine);
        durationRoutine = StartCoroutine(SwordDurationRoutine());
    }

    private IEnumerator SwordDurationRoutine()
    {
        yield return new WaitForSeconds(swordActiveTime);
        EndSword();
    }

    private IEnumerator HitboxWindow()
    {
        SetHitbox(true);
        yield return new WaitForSeconds(hitboxActiveTime);
        SetHitbox(false);
        hitboxRoutine = null;
    }

    private void EndSword()
    {
        isEquipped = false;

        SetHitbox(false);
        SetVisual(false);

        player2Anim?.SetSwordEquipped(false);
        player2Anim?.SetCombatLocked(false);

        BlockOtherWeapons(false);

        cooldownUntil = Time.time + swordAttackCooldown;
    }

    private void BlockOtherWeapons(bool block)
    {
        if (weaponsToBlock == null) return;

        foreach (var w in weaponsToBlock)
        {
            if (w == null || w == this) continue;

            if (block)
            {
                w.OnDeselected();
                w.enabled = false;
            }
            else
            {
                w.enabled = true;
            }
        }
    }

    private void SetVisual(bool state)
    {
        if (swordVisual != null) swordVisual.SetActive(state);
    }

    private void SetHitbox(bool state)
    {
        if (swordHitbox != null) swordHitbox.SetActive(state);
    }
}