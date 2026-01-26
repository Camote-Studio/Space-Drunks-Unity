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

    [Header("Bloqueo de otros ataques (Attack 1 / Attack 2)")]
    [SerializeField] private WeaponBase[] weaponsToBlock; // arrastra aquí Attack1Idle, Attack1Move, Attack2, etc.

    private bool isEquipped;
    private float cooldownUntil;
    private float swingUntil;

    private Coroutine durationRoutine;
    private Coroutine hitboxRoutine;

    private Player2Animation player2Anim;

    private void Awake()
    {
        player2Anim = GetComponentInParent<Player2Animation>();

        SetVisual(false);
        SetHitbox(false);

        // por si quedaba sucio
        player2Anim?.SetSwordEquipped(false);
        player2Anim?.SetCombatLocked(false);

        // por si el play mode quedó raro
        BlockOtherWeapons(false);
    }

    public override void OnSelected()
    {
        SetVisual(isEquipped);
        SetHitbox(false);
    }

    public override void OnDeselected()
    {
        ForceStopSword();
    }

    private void OnDisable()
    {
        ForceStopSword();
    }

    private void OnDestroy()
    {
        ForceStopSword();
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (!fireDown) return;

        // 1) Si NO está equipada: activar (si no está en cooldown)
        if (!isEquipped)
        {
            if (Time.time < cooldownUntil) return;
            StartSword();
            return;
        }

        // 2) Si YA está equipada: atacar (cooldown de swing)
        if (Time.time < swingUntil) return;
        swingUntil = Time.time + swingCooldown;

        player2Anim?.DoSwordAttack();

        if (hitboxRoutine != null) StopCoroutine(hitboxRoutine);
        hitboxRoutine = StartCoroutine(HitboxWindow());
    }

    private void StartSword()
    {
        isEquipped = true;
        swingUntil = 0f;

        // Bloquea otros ataques (Attack1/2) SIN tocar PlayerMovement
        BlockOtherWeapons(true);

        // (Opcional) También bloquea triggers de animaciones normales si ya lo estabas usando
        player2Anim?.SetCombatLocked(true);

        // entra a SwordIdle
        player2Anim?.SetSwordEquipped(true);

        SetVisual(true);
        SetHitbox(false);

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
        Physics2D.SyncTransforms();

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

        // Desbloquea otros ataques
        BlockOtherWeapons(false);

        cooldownUntil = Time.time + swordAttackCooldown;

        if (durationRoutine != null) StopCoroutine(durationRoutine);
        durationRoutine = null;

        if (hitboxRoutine != null) StopCoroutine(hitboxRoutine);
        hitboxRoutine = null;
    }

    private void ForceStopSword()
    {
        // corta coroutines
        if (durationRoutine != null) StopCoroutine(durationRoutine);
        durationRoutine = null;

        if (hitboxRoutine != null) StopCoroutine(hitboxRoutine);
        hitboxRoutine = null;

        isEquipped = false;

        SetHitbox(false);
        SetVisual(false);

        player2Anim?.SetSwordEquipped(false);
        player2Anim?.SetCombatLocked(false);

        // IMPORTANTE: si se desactiva el objeto/script, desbloquea igual
        BlockOtherWeapons(false);
    }

    private void BlockOtherWeapons(bool block)
    {
        if (weaponsToBlock == null) return;

        for (int i = 0; i < weaponsToBlock.Length; i++)
        {
            var w = weaponsToBlock[i];
            if (w == null) continue;
            if (w == this) continue;

            if (block)
            {
                // apaga visual/hitbox del arma antes de deshabilitarla (si lo implementa)
                w.OnDeselected();
                w.enabled = false;
            }
            else
            {
                w.enabled = true;
                // NO llamo OnSelected() porque PlayerWeapon decide cuál está activo visualmente
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
