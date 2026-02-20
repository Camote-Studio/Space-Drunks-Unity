using UnityEngine;

public class WeaponPlayer2Animation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;

    private int hashPunchRight;
    private int hashPunchMiddle;
    private int hashPunchLeft;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        hashPunchRight = Animator.StringToHash("RightAttack");
        hashPunchMiddle = Animator.StringToHash("UpAttack");
        hashPunchLeft = Animator.StringToHash("LeftAttack");
    }

    public void PlayPunchRight()
    {
        if (animator == null) return;
        animator.SetTrigger(hashPunchRight);
    }

    public void PlayPunchMiddle()
    {
        if (animator == null) return;
        animator.SetTrigger(hashPunchMiddle);
    }

    public void PlayPunchLeft()
    {
        if (animator == null) return;
        animator.SetTrigger(hashPunchLeft);
    }

    public void PlayUltimatePunch(int index)
    {
        if (animator == null) return;

        int pattern = index % 3;

        switch (pattern)
        {
            case 0:
                animator.SetTrigger(hashPunchRight);
                break;
            case 1:
                animator.SetTrigger(hashPunchMiddle);
                break;
            default:
                animator.SetTrigger(hashPunchLeft);
                break;
        }
    }
}
