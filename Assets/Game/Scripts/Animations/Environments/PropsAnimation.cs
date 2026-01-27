using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PropsAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Params")]
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string breakTrigger = "Break";

    private int hitHash;
    private int breakHash;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();

        hitHash = Animator.StringToHash(hitTrigger);
        breakHash = Animator.StringToHash(breakTrigger);
    }

    public void PlayHit()
    {
        if (animator == null) return;
        animator.SetTrigger(hitHash);
    }

    public void PlayBreak()
    {
        if (animator == null) return;
        animator.SetTrigger(breakHash);
    }
}
