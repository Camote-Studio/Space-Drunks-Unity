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
        hashPunchAttack = Animator.StringToHash("PunchAttack");
        hashBackAttack = Animator.StringToHash("BackAttack");
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

        Vector2 moveInput = movement.MoveInput;
        float speed = moveInput.magnitude;
        animator.SetFloat(hashSpeed, speed);
    }

    private void OnDamaged()
    {
        if (animator == null)
            return;

        animator.SetTrigger(hashHit);
    }

    public void PlayPunchAttack()
    {
        if (animator == null) return;
        Debug.Log("Player2Animation: PunchAttack trigger");
        animator.SetTrigger(hashPunchAttack);
    }

    public void PlayBackAttack()
    {
        if (animator == null) return;
        Debug.Log("Player2Animation: BackAttack trigger");
        animator.SetTrigger(hashBackAttack);
    }
}
