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
    private int hashMachineShoot;   

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
        hashMachineShoot = Animator.StringToHash("MachineShoot"); 
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

    public void PlayShoot()          
    {
        if (animator == null) return;
        animator.SetTrigger(hashShoot);
    }

    public void PlayBatAttack()      
    {
        if (animator == null) return;
        animator.SetTrigger(hashBatAttack);
    }

    public void PlayPlaceMine()     
    {
        if (animator == null) return;
        animator.SetTrigger(hashPlaceMine);
    }

    public void PlayMachineShoot()  
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineShoot);
    }
}
