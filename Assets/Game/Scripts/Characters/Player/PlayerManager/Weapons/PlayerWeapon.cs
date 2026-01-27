using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private PlayerMovement movement;

    [Header("Estado")]
    [SerializeField] private estado_jugador estadoJugador;

    [Header("Attack 1")]
    [SerializeField] private WeaponBase attack1IdleWeapon;
    [SerializeField] private WeaponBase attack1MoveWeapon;

    [Header("Attack 2")]
    [SerializeField] private WeaponBase attack2Weapon;

    [Header("Attack 3")]
    [SerializeField] private WeaponBase attack3Weapon;

    private WeaponBase currentAttack1;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (estadoJugador == null)
            estadoJugador = GetComponent<estado_jugador>();
    }

    private void Start()
    {
        EquipAttack1(attack1IdleWeapon);
    }

    public void SetInput(
        bool attack1Down, bool attack1Held, bool attack1Up,
        bool attack2Down, bool attack2Held, bool attack2Up,
        bool attack3Down, bool attack3Held, bool attack3Up
    )
    {
        // 🚫 BLOQUEO TOTAL DE ATAQUES SI ESTÁ FLOTANDO
        if (estadoJugador != null && !estadoJugador.PuedeAtacar())
            return;

        HandleAttack1(attack1Down, attack1Held, attack1Up);

        if (attack2Weapon != null)
            attack2Weapon.Tick(attack2Down, attack2Held, attack2Up);

        if (attack3Weapon != null)
            attack1IdleWeapon = null;
            attack1MoveWeapon = null;
            attack3Weapon.Tick(attack3Down, attack3Held, attack3Up);
    }

    private void HandleAttack1(bool down, bool held, bool up)
    {
        if (attack1IdleWeapon == null && attack1MoveWeapon == null)
            return;

        bool movingHoriz = movement != null && movement.IsMovingHorizontally;

        WeaponBase target = attack1IdleWeapon;
        if (movingHoriz && attack1MoveWeapon != null)
            target = attack1MoveWeapon;

        if (target != currentAttack1)
            EquipAttack1(target);

        currentAttack1?.Tick(down, held, up);
    }

    private void EquipAttack1(WeaponBase newWeapon)
    {
        if (currentAttack1 == newWeapon)
            return;

        currentAttack1?.OnDeselected();

        currentAttack1 = newWeapon;
        currentAttack1?.OnSelected();
    }
}
