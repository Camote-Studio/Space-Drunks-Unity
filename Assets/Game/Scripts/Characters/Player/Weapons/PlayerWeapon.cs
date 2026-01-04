using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private MineWeapon mineWeapon;

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
        bool movingHoriz = movement != null && movement.IsMovingHorizontally;

        WeaponBase target = movingHoriz ? batWeapon : gunWeapon;
        if (target != currentWeapon)
            EquipWeapon(target);

        bool fireDown = Input.GetMouseButtonDown(0);
        bool fireHeld = Input.GetMouseButton(0);
        bool fireUp = Input.GetMouseButtonUp(0);

        if (currentWeapon != null)
            currentWeapon.Tick(fireDown, fireHeld, fireUp);

        if (mineWeapon != null)
        {
            mineWeapon.Tick();

            if (Input.GetKeyDown(KeyCode.E))
            {
                mineWeapon.TryPlaceMine();
            }
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
