using UnityEngine;

public class Player2Animation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;

    private int hashSpeed;
    private int hashHit;
    private int hashPunchAttack;
    private int hashBackAttack;

    private int hashSwordEquipped;
    private int hashSwordAttack;

    private int hashUltimate;
    private int hashUltiIni;
    private int hashUltimatePunch;

    private int hashIsMovingUp;

    private bool combatLocked;

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
        hashPunchAttack = Animator.StringToHash("PunchAttack");
        hashBackAttack = Animator.StringToHash("BackAttack");

        hashSwordEquipped = Animator.StringToHash("SwordEquipped");
        hashSwordAttack = Animator.StringToHash("SwordAttack");

        hashUltimate = Animator.StringToHash("Ultimate");
        hashUltiIni = Animator.StringToHash("UltiIni");
        hashUltimatePunch = Animator.StringToHash("UltimatePunch");

        hashIsMovingUp = Animator.StringToHash("IsMovingUp");
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

        if (combatLocked)
        {
            animator.SetBool(hashIsMovingUp, false);
            animator.SetFloat(hashSpeed, 0f);
            return;
        }

        Vector2 input = movement.MoveInput;

        float hx = Mathf.Abs(input.x);
        float vy = Mathf.Abs(input.y);

        bool movingUp = vy > 0.1f && hx < 0.1f;

        animator.SetBool(hashIsMovingUp, movingUp);

        if (movingUp)
            animator.SetFloat(hashSpeed, 0f);
        else
            animator.SetFloat(hashSpeed, Mathf.Abs(movement.GetComponent<Rigidbody2D>()?.linearVelocity.x ?? 0f));

    }

    private void OnDamaged(string tipoDanio)
    {
        if (animator == null) return;
        animator.SetTrigger(hashHit);
    }

    public void SetCombatLocked(bool value)
    {
        combatLocked = value;
    }

    public bool IsCombatLocked => combatLocked;

    public void PlayPunchAttack()
    {
        if (animator == null) return;
        if (combatLocked) return;
        animator.SetTrigger(hashPunchAttack);
    }

    public void PlayBackAttack()
    {
        if (animator == null) return;
        if (combatLocked) return;
        animator.SetTrigger(hashBackAttack);
    }

    public void SetSwordEquipped(bool value)
    {
        if (animator == null) return;
        animator.SetBool(hashSwordEquipped, value);
    }

    public void DoSwordAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(hashSwordAttack);
    }

    public void PlayUltimateStart()
    {
        if (animator == null) return;

        combatLocked = true;
        animator.SetBool(hashUltiIni, false);
        animator.SetTrigger(hashUltimate);
    }

    public void SetUltimateStateActive(bool active)
    {
        if (animator == null) return;
        animator.SetBool(hashUltiIni, active);
    }

    public void PlayUltimatePunch()
    {
        if (animator == null) return;
        animator.SetTrigger(hashUltimatePunch);
    }

    public void EndUltimate()
    {
        if (animator == null) return;

        animator.SetBool(hashUltiIni, false);
        combatLocked = false;
    }
}
