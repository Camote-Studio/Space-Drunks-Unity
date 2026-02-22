using UnityEngine;

public class Player2Animation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;

    [Header("Sword")]
    [SerializeField] private bool requireSwordEquippedToAttack = true;

    private int hashSpeed;
    private int hashHit;
    private int hashPunchAttack;
    private int hashBackAttack;

    private int hashSwordEquipped;
    private int hashSwordStart;

    private int hashUltimate;
    private int hashUltiIni;
    private int hashUltimatePunch;

    private int hashIsMovingUp;
    private int hashIsMovingDown;

    private bool combatLocked;
    private Rigidbody2D rb;

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

        rb = movement != null ? movement.GetComponent<Rigidbody2D>() : GetComponent<Rigidbody2D>();

        hashSpeed = Animator.StringToHash("Speed");
        hashHit = Animator.StringToHash("Hit");
        hashPunchAttack = Animator.StringToHash("PunchAttack");
        hashBackAttack = Animator.StringToHash("BackAttack");

        hashSwordEquipped = Animator.StringToHash("SwordEquipped");
        hashSwordStart = Animator.StringToHash("SwordStart");

        hashUltimate = Animator.StringToHash("Ultimate");
        hashUltiIni = Animator.StringToHash("UltiIni");
        hashUltimatePunch = Animator.StringToHash("UltimatePunch");

        hashIsMovingUp = Animator.StringToHash("IsMovingUp");
        hashIsMovingDown = Animator.StringToHash("IsMovingDown");
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        if (combatLocked)
        {
            animator.SetFloat(hashSpeed, 0f);
            animator.SetBool(hashIsMovingUp, false);
            animator.SetBool(hashIsMovingDown, false);
            return;
        }

        const float t = 0.1f;
        Vector2 input = movement.MoveInput;

        float ax = Mathf.Abs(input.x);

        bool movingUp = input.y > t && ax < t;
        bool movingDown = input.y < -t && ax < t;

        animator.SetBool(hashIsMovingUp, movingUp);
        animator.SetBool(hashIsMovingDown, movingDown);

        if (movingUp || movingDown)
        {
            animator.SetFloat(hashSpeed, 0f);
            return;
        }

        float vx = rb != null ? Mathf.Abs(rb.linearVelocity.x) : ax;
        animator.SetFloat(hashSpeed, vx);
    }

    public void SetCombatLocked(bool value)
    {
        combatLocked = value;
    }

    public bool IsCombatLocked => combatLocked;

    public void PlayPunchAttack()
    {
        if (combatLocked) return;
        animator.SetTrigger(hashPunchAttack);
    }

    public void PlayBackAttack()
    {
        if (combatLocked) return;
        animator.SetTrigger(hashBackAttack);
    }

    public void SetSwordEquipped(bool value)
    {
        animator.SetBool(hashSwordEquipped, value);
    }

    public void DoSwordAttack()
    {
        if (combatLocked) return;

        if (requireSwordEquippedToAttack &&
            !animator.GetBool(hashSwordEquipped))
            return;

        animator.ResetTrigger(hashSwordStart);
        animator.SetTrigger(hashSwordStart);
    }

    public void PlayUltimateStart()
    {
        combatLocked = true;

        animator.SetFloat(hashSpeed, 0f);
        animator.SetBool(hashUltiIni, false);
        animator.SetTrigger(hashUltimate);
    }

    public void SetUltimateStateActive(bool active)
    {
        animator.SetBool(hashUltiIni, active);
    }

    public void PlayUltimatePunch()
    {
        animator.SetTrigger(hashUltimatePunch);
    }

    public void EndUltimate()
    {
        animator.SetBool(hashUltiIni, false);
        combatLocked = false;
    }
}