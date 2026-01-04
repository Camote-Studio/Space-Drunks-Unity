using UnityEngine;

public class MachineWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform placeOrigin;        // por ejemplo: MachineVisual/FirePoint
    [SerializeField] private MachineGun machineGunPrefab;  // prefab de la torreta
    [SerializeField] private PlayerAnimation playerAnimation;

    [Header("Colocación")]
    [SerializeField] private float placeDistance = 5.5f;   // distancia delante del player
    [SerializeField] private float preSpawnDelay = 0.8f;   // retardo antes de que caiga

    [Header("Cooldown")]
    [SerializeField] private float cooldownDuration = 24f;

    private float cooldownTimer;
    private bool isPlacing;
    private float placingTimer;
    private Vector2 cachedPlacePos;

    private MachineGun activeMachineGun;

    private void Awake()
    {
        if (playerAnimation == null)
            playerAnimation = GetComponent<PlayerAnimation>();

        if (placeOrigin == null)
            placeOrigin = transform; // por si acaso
    }

    public void Tick()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isPlacing)
        {
            placingTimer -= Time.deltaTime;
            if (placingTimer <= 0f)
                FinishPlace();
        }
    }

    public void TryUseMachine()
    {
        if (cooldownTimer > 0f) return;
        if (isPlacing) return;
        if (activeMachineGun != null) return; // solo una activa

        // dirección: usamos el right del placeOrigin para respetar hacia dónde mira
        Vector2 dir = placeOrigin.right.normalized;
        cachedPlacePos = (Vector2)placeOrigin.position + dir * placeDistance;

        // anim del player con el control
        playerAnimation?.PlayMachineShoot();

        isPlacing = true;
        placingTimer = preSpawnDelay;
    }

    private void FinishPlace()
    {
        isPlacing = false;

        if (machineGunPrefab == null) return;

        MachineGun mg = Instantiate(machineGunPrefab, cachedPlacePos, Quaternion.identity);
        activeMachineGun = mg;
        mg.OnTurretEnd += HandleTurretEnd; // la torreta avisa cuando muere / explota
    }

    private void HandleTurretEnd(MachineGun mg)
    {
        if (mg == activeMachineGun)
            activeMachineGun = null;

        cooldownTimer = cooldownDuration;
    }
}
