using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private PlayerMovement movement;

    [Header("Weapons")]
    [SerializeField] private WeaponBase gunWeapon;
    [SerializeField] private WeaponBase batWeapon;
    [SerializeField] private MineWeapon mineWeapon;
    [SerializeField] private MachineWeapon machineWeapon;

    private WeaponBase currentWeapon;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (mineWeapon == null)
            mineWeapon = GetComponent<MineWeapon>();

        if (machineWeapon == null)
            machineWeapon = GetComponent<MachineWeapon>();
    }

    private void Start()
    {
        EquipWeapon(gunWeapon);
    }

    public void SetInput(
        bool fireDown,
        bool fireHeld,
        bool fireUp,
        bool minePressed,
        bool machinePressed
    )
    {
        bool hasMachine = machineWeapon != null;

        if (hasMachine && machinePressed && !machineWeapon.IsActive && machineWeapon.IsReady)
        {
            machineWeapon.TryActivate();
        }

        bool machineActive = hasMachine && machineWeapon.IsActive;

        if (movement != null)
            movement.SetMovementLocked(machineActive);

        if (hasMachine)
        {
            bool shootForMachine = machineActive && fireDown;
            machineWeapon.SetShootInput(shootForMachine);
        }

        if (machineActive)
            return; 

        bool movingHoriz = movement != null && movement.IsMovingHorizontally;
        WeaponBase target = movingHoriz ? batWeapon : gunWeapon;

        if (target != currentWeapon)
            EquipWeapon(target);

        if (currentWeapon != null)
            currentWeapon.Tick(fireDown, fireHeld, fireUp);

        if (mineWeapon != null)
        {
            mineWeapon.Tick();

            if (minePressed)
                mineWeapon.TryPlaceMine();
        }
    }

    private void EquipWeapon(WeaponBase newWeapon)
    {
        if (currentWeapon == newWeapon)
            return;

        if (currentWeapon != null)
            currentWeapon.OnDeselected();

        currentWeapon = newWeapon;

        if (currentWeapon != null)
            currentWeapon.OnSelected();
    }
}
