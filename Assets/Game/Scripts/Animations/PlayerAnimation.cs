using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;

    private int hashSpeed;
    private int hashHit;

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

        Debug.Log("PlayerAnimation: disparo trigger HIT");
        animator.SetTrigger(hashHit);
    }
}
