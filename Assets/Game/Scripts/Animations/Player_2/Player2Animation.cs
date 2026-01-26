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

    // Espada
    private int hashSwordEquipped; // Bool
    private int hashSwordAttack;   // Trigger

    // Lock simple
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

        // NOMBRES EXACTOS en el Animator:
        hashSwordEquipped = Animator.StringToHash("SwordEquipped");
        hashSwordAttack = Animator.StringToHash("SwordAttack");
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
        animator.SetFloat(hashSpeed, moveInput.magnitude);
    }

    private void OnDamaged()
    {
        if (animator == null) return;
        animator.SetTrigger(hashHit);
    }

    // ===== LOCK =====
    public void SetCombatLocked(bool value)
    {
        combatLocked = value;
    }

    public bool IsCombatLocked => combatLocked;

    // ===== ATAQUES NORMALES (bloqueados si hay espada) =====
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

    // ===== ESPADA =====
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
}
