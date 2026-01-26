using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;

    private int hashSpeed;
    private int hashHit;
    private int hashShoot;
    private int hashBatAttack;
    private int hashPlaceMine;
    private int hashMachineStart;
    private int hashMachineShoot;
    private int hashMachineEnd;

    private bool isInMachineMode;

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
        if (animator == null || movement == null)
            return;

        if (isInMachineMode)
        {
            animator.SetFloat(hashSpeed, 0f);
            return;
        }

        Vector2 moveInput = movement.MoveInput;
        float speed = Mathf.Abs(moveInput.x);
        animator.SetFloat(hashSpeed, speed);
    }

    private void OnDamaged()
    {
        if (animator == null)
            return;

        if (isInMachineMode)
            return;

        animator.SetTrigger(hashHit);
    }

    public void PlayShoot()
    {
        if (animator == null)
            return;
        if (isInMachineMode)
            return;
        animator.SetTrigger(hashShoot);
    }

    public void PlayBatAttack()
    {
        if (animator == null)
            return;
        if (isInMachineMode)
            return;
        animator.SetTrigger(hashBatAttack);
    }

    public void PlayPlaceMine()
    {
        if (animator == null)
            return;
        if (isInMachineMode)
            return;
        animator.SetTrigger(hashPlaceMine);
    }

    public void PlayMachineStart()
    {
        if (animator == null)
            return;

        isInMachineMode = true;
        animator.ResetTrigger(hashHit);
        animator.ResetTrigger(hashShoot);
        animator.ResetTrigger(hashBatAttack);
        animator.ResetTrigger(hashPlaceMine);
        animator.SetFloat(hashSpeed, 0f);
        animator.SetTrigger(hashMachineStart);
    }

    public void PlayMachineShoot()
    {
        if (animator == null)
            return;
        if (!isInMachineMode)
            return;

        animator.SetTrigger(hashMachineShoot);
    }

    public void PlayMachineEnd()
    {
        if (animator == null)
            return;

        animator.SetTrigger(hashMachineEnd);
        isInMachineMode = false;
    }
}
