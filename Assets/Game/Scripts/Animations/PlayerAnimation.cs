using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;

    // Parámetros del Animator
    private int hashSpeed;
    private int hashHit;
    private int hashShoot;
    private int hashBatAttack;
    private int hashPlaceMine;

    // Máquina
    private int hashMachineStart;   // entra a Player_machine_idle
    private int hashMachineShoot;   // Player_machine_shoot (rafaga)
    private int hashMachineEnd;     // Player_machine_destroy / salir de modo máquina

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        vidaJugador = GetComponent<VidaJugador>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (movement == null)
            movement = GetComponent<PlayerMovement>();
        if (vidaJugador == null)
            vidaJugador = GetComponent<VidaJugador>();

        hashSpeed = Animator.StringToHash("Speed");
        hashHit = Animator.StringToHash("Hit");
        hashShoot = Animator.StringToHash("Shoot");
        hashBatAttack = Animator.StringToHash("BatAttack");
        hashPlaceMine = Animator.StringToHash("PlaceMine");

        // nombres de TRIGGERS que debes crear en el Animator
        hashMachineStart = Animator.StringToHash("MachineStart");
        hashMachineShoot = Animator.StringToHash("MachineShoot");
        hashMachineEnd = Animator.StringToHash("MachineEnd");
    }

    private void OnEnable()
    {
        if (vidaJugador != null)
            vidaJugador.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (vidaJugador != null)
            vidaJugador.OnDamaged -= OnDamaged;
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        Vector2 moveInput = movement.MoveInput;
        float speed = Mathf.Abs(moveInput.x);
        animator.SetFloat(hashSpeed, speed);
    }

    private void OnDamaged()
    {
        if (animator == null) return;
        animator.SetTrigger(hashHit);
    }

    // --- Pistola normal ---
    public void PlayShoot()
    {
        if (animator == null) return;
        animator.SetTrigger(hashShoot);
    }

    // --- Bate ---
    public void PlayBatAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(hashBatAttack);
    }

    // --- Mina ---
    public void PlayPlaceMine()
    {
        if (animator == null) return;
        animator.SetTrigger(hashPlaceMine);
    }

    // --- Máquina / torreta pegada al player ---
    public void PlayMachineStart()
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineStart);
    }

    public void PlayMachineShoot()
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineShoot);
    }

    public void PlayMachineEnd()
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineEnd);
    }
}
