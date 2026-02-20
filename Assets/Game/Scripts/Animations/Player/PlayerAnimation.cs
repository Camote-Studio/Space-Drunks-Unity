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
    private int hashIsMovingUp;

    private int hashUltimate;
    private int hashUltiIni;

    private bool isInMachineMode;
    private bool isInUltimateMode;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        vidaJugador = GetComponent<VidaJugador>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (vidaJugador == null) vidaJugador = GetComponent<VidaJugador>();

        hashSpeed = Animator.StringToHash("Speed");
        hashHit = Animator.StringToHash("Hit");
        hashShoot = Animator.StringToHash("Shoot");
        hashBatAttack = Animator.StringToHash("BatAttack");
        hashPlaceMine = Animator.StringToHash("PlaceMine");
        hashMachineStart = Animator.StringToHash("MachineStart");
        hashMachineShoot = Animator.StringToHash("MachineShoot");
        hashMachineEnd = Animator.StringToHash("MachineEnd");
        hashIsMovingUp = Animator.StringToHash("IsMovingUp");

        hashUltimate = Animator.StringToHash("Ultimate");
        hashUltiIni = Animator.StringToHash("UltiIni");
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

        if (isInMachineMode || isInUltimateMode)
        {
            animator.SetFloat(hashSpeed, 0f);
            return;
        }

        Rigidbody2D rb = movement.GetComponent<Rigidbody2D>();
        Vector2 v = rb != null ? rb.linearVelocity : Vector2.zero;

        float horizontal = Mathf.Abs(v.x);
        float vertical = Mathf.Abs(v.y);
        bool movingUp = vertical > 0.01f;

        animator.SetBool(hashIsMovingUp, movingUp);

        if (movingUp) animator.SetFloat(hashSpeed, 0f);
        else animator.SetFloat(hashSpeed, horizontal);
    }

    private void OnDamaged(string fuente)
    {
        if (animator == null) return;
        if (isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashHit);
    }

    public void PlayShoot()
    {
        if (animator == null) return;
        if (isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashShoot);
    }

    public void PlayBatAttack()
    {
        if (animator == null) return;
        if (isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashBatAttack);
    }

    public void PlayPlaceMine()
    {
        if (animator == null) return;
        if (isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashPlaceMine);
    }

    public void PlayMachineStart()
    {
        if (animator == null) return;

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
        if (animator == null) return;
        if (!isInMachineMode) return;
        animator.SetTrigger(hashMachineShoot);
    }

    public void PlayMachineEnd()
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineEnd);
        isInMachineMode = false;
    }

    public void PlayUltimateStart()
    {
        if (animator == null) return;

        isInUltimateMode = true;

        animator.ResetTrigger(hashHit);
        animator.ResetTrigger(hashShoot);
        animator.ResetTrigger(hashBatAttack);
        animator.ResetTrigger(hashPlaceMine);

        animator.SetFloat(hashSpeed, 0f);
        animator.SetBool(hashUltiIni, false);
        animator.SetTrigger(hashUltimate);
    }

    public void SetUltimateStateActive(bool active)
    {
        if (animator == null) return;
        animator.SetBool(hashUltiIni, active);
    }

    public void EndUltimate()
    {
        if (animator == null) return;
        animator.SetBool(hashUltiIni, false);
        isInUltimateMode = false;
    }
}
