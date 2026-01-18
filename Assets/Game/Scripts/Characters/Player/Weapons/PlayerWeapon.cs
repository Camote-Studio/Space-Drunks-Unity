using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private MineWeapon mineWeapon;
    [SerializeField] private MachineWeapon machineWeapon;

    [Header("Weapons")]
    [SerializeField] private WeaponBase gunWeapon;
    [SerializeField] private WeaponBase batWeapon;

    private WeaponBase currentWeapon;

    private void Awake()
    {
        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (mineWeapon == null)
            mineWeapon = GetComponent<MineWeapon>();
    }

    private void Start()
    {
        EquipWeapon(gunWeapon);
    }

    private void Update()
    {
        bool fireDown = Input.GetMouseButtonDown(0);
        bool fireHeld = Input.GetMouseButton(0);
        bool fireUp = Input.GetMouseButtonUp(0);

        if (machineWeapon != null && Input.GetKeyDown(KeyCode.G))
        {
            machineWeapon.TryActivate();
        }

        bool machineActive = machineWeapon != null && machineWeapon.IsActive;

        if (movement != null)
            movement.SetMovementLocked(machineActive);

        bool movingHoriz = movement != null && movement.IsMovingHorizontally;
        WeaponBase target = movingHoriz ? batWeapon : gunWeapon;

        if (!machineActive && target != currentWeapon)
            EquipWeapon(target);

        if (!machineActive && currentWeapon != null)
            currentWeapon.Tick(fireDown, fireHeld, fireUp);

        if (!machineActive && mineWeapon != null)
        {
            mineWeapon.Tick();

            if (Input.GetKeyDown(KeyCode.E))
                mineWeapon.TryPlaceMine();
        }

        if (machineWeapon != null)
        {
            bool shootForMachine = machineActive && fireDown;
            machineWeapon.SetShootInput(shootForMachine);
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
