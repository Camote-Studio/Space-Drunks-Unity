using UnityEngine;
using System.Collections;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private VidaJugador vidaJugador;
    [SerializeField] private PlayerWeapon playerWeapon;

    [Header("Knockdown")]
    [SerializeField] private float knockdownStaySeconds = 1.5f;
    [SerializeField] private string knockdownStateName = "Damage_Hit";
    [SerializeField] private string reincorporationStateName = "Reincorporation_Damage";

    private int hashSpeed;
    private int hashHit;
    private int hashShoot;
    private int hashBatAttack;
    private int hashPlaceMine;
    private int hashMachineStart;
    private int hashMachineShoot;
    private int hashMachineEnd;
    private int hashIsMovingUp;
    private int hashIsMovingDown;
    private int hashUltimate;
    private int hashUltiIni;
    private int hashComboHit;
    private int hashUpHit;
    private int hashIsKnockedDown;

    private bool isInMachineMode;
    private bool isInUltimateMode;
    private bool isKnockedDown;

    private Rigidbody2D rb;
    private Coroutine knockdownRoutine;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        vidaJugador = GetComponent<VidaJugador>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (vidaJugador == null) vidaJugador = GetComponent<VidaJugador>();
        if (playerWeapon == null) playerWeapon = GetComponent<PlayerWeapon>();

        rb = movement != null ? movement.GetComponent<Rigidbody2D>() : GetComponent<Rigidbody2D>();

        hashSpeed = Animator.StringToHash("Speed");
        hashHit = Animator.StringToHash("Hit");
        hashShoot = Animator.StringToHash("Shoot");
        hashBatAttack = Animator.StringToHash("BatAttack");
        hashPlaceMine = Animator.StringToHash("PlaceMine");
        hashMachineStart = Animator.StringToHash("MachineStart");
        hashMachineShoot = Animator.StringToHash("MachineShoot");
        hashMachineEnd = Animator.StringToHash("MachineEnd");
        hashIsMovingUp = Animator.StringToHash("IsMovingUp");
        hashIsMovingDown = Animator.StringToHash("IsMovingDown");
        hashUltimate = Animator.StringToHash("Ultimate");
        hashUltiIni = Animator.StringToHash("UltiIni");
        hashComboHit = Animator.StringToHash("ComboHit");
        hashUpHit = Animator.StringToHash("UpHit");
        hashIsKnockedDown = Animator.StringToHash("IsKnockedDown");
    }

    private void OnEnable()
    {
        if (vidaJugador != null)
        {
            vidaJugador.OnDamaged += OnDamaged;
            vidaJugador.OnKnockdownStart += OnKnockdownStart;
        }
    }

    private void OnDisable()
    {
        if (vidaJugador != null)
        {
            vidaJugador.OnDamaged -= OnDamaged;
            vidaJugador.OnKnockdownStart -= OnKnockdownStart;
        }
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        if (isKnockedDown || isInMachineMode || isInUltimateMode)
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

        float vx = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
        animator.SetFloat(hashSpeed, vx);
    }

    private void OnDamaged(string fuente)
    {
        if (animator == null) return;
        if (isKnockedDown) return;
        if (isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashHit);
    }

    private void OnKnockdownStart()
    {
        if (animator == null) return;
        if (knockdownRoutine != null) StopCoroutine(knockdownRoutine);
        knockdownRoutine = StartCoroutine(KnockdownFlow());
    }

    private IEnumerator KnockdownFlow()
    {
        isKnockedDown = true;
        animator.SetBool(hashIsKnockedDown, true);

        if (movement != null) movement.SetMovementLocked(true);
        if (playerWeapon != null) playerWeapon.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetFloat(hashSpeed, 0f);
        animator.SetBool(hashIsMovingUp, false);
        animator.SetBool(hashIsMovingDown, false);

        animator.ResetTrigger(hashHit);
        animator.ResetTrigger(hashShoot);
        animator.ResetTrigger(hashBatAttack);
        animator.ResetTrigger(hashPlaceMine);
        animator.ResetTrigger(hashMachineStart);
        animator.ResetTrigger(hashMachineShoot);
        animator.ResetTrigger(hashMachineEnd);
        animator.ResetTrigger(hashUltimate);
        animator.ResetTrigger(hashUpHit);
        animator.ResetTrigger(hashComboHit);

        if (vidaJugador != null)
            vidaJugador.ActivarIntocable(knockdownStaySeconds + 3f);

        animator.SetTrigger(hashComboHit);

        yield return null;

        // Esperar a entrar al estado Damage_Hit
        float timeout = Time.time + 1f;
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(knockdownStateName)
            || Time.time > timeout);

        // Esperar a que termine la animación
        timeout = Time.time + 5f;
        yield return new WaitUntil(() =>
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            return (!st.IsName(knockdownStateName) || st.normalizedTime >= 0.95f)
                   || Time.time > timeout;
        });

        // Quedarse tirado el tiempo configurado
        yield return new WaitForSeconds(knockdownStaySeconds);

        // Levantarse
        animator.ResetTrigger(hashUpHit);
        animator.SetTrigger(hashUpHit);

        yield return null;

        // Esperar a entrar a Reincorporation_Damage
        timeout = Time.time + 2f;
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName(reincorporationStateName)
            || Time.time > timeout);

        // Esperar a que termine
        timeout = Time.time + 5f;
        yield return new WaitUntil(() =>
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            return (!st.IsName(reincorporationStateName) || st.normalizedTime >= 0.95f)
                   || Time.time > timeout;
        });

        // Restaurar todo
        animator.SetBool(hashIsKnockedDown, false);
        isKnockedDown = false;

        if (movement != null) movement.SetMovementLocked(false);
        if (playerWeapon != null) playerWeapon.enabled = true;

        if (vidaJugador != null)
            vidaJugador.FinishKnockdown();

        knockdownRoutine = null;
    }

    public void PlayShoot()
    {
        if (animator == null || isKnockedDown || isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashShoot);
    }

    public void PlayBatAttack()
    {
        if (animator == null || isKnockedDown || isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashBatAttack);
    }

    public void PlayPlaceMine()
    {
        if (animator == null || isKnockedDown || isInMachineMode || isInUltimateMode) return;
        animator.SetTrigger(hashPlaceMine);
    }

    public void PlayMachineStart()
    {
        if (animator == null || isKnockedDown) return;
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
        if (animator == null || isKnockedDown || !isInMachineMode) return;
        animator.SetTrigger(hashMachineShoot);
    }

    public void PlayMachineEnd()
    {
        if (animator == null) return;
        animator.SetTrigger(hashMachineEnd);
        isInMachineMode = false;
    }

    public void UltimateEnterState()
    {
        if (animator == null || isKnockedDown) return;
        animator.SetBool(hashUltiIni, true);
    }

    public void PlayUltimateStart()
    {
        if (animator == null || isKnockedDown) return;
        isInUltimateMode = true;
        animator.SetFloat(hashSpeed, 0f);
        animator.SetBool(hashUltiIni, false);
        animator.SetTrigger(hashUltimate);
    }

    public void SetUltimateStateActive(bool active)
    {
        if (animator == null || isKnockedDown) return;
        animator.SetBool(hashUltiIni, active);
    }

    public void EndUltimate()
    {
        if (animator == null) return;
        animator.SetBool(hashUltiIni, false);
        isInUltimateMode = false;
    }
}