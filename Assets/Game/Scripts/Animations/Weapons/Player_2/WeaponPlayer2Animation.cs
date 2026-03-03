using UnityEngine;

public class WeaponPlayer2Animation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer playerSprite;

    [Header("Trigger Names")]
    [SerializeField] private string triggerPunchRight = "RightAttack";
    [SerializeField] private string triggerPunchMiddle = "UpAttack";
    [SerializeField] private string triggerPunchLeft = "LeftAttack";

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>(true);
        playerSprite = GetComponentInParent<SpriteRenderer>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (playerSprite == null)
            playerSprite = GetComponentInParent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (playerSprite == null) return;

        // Flip completo del arma (visual + colliders)
        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (playerSprite.flipX ? -1f : 1f);
        transform.localScale = scale;
    }

    public void PlayPunchRight()
    {
        if (animator == null) return;
        animator.SetTrigger(triggerPunchRight);
    }

    public void PlayPunchMiddle()
    {
        if (animator == null) return;
        animator.SetTrigger(triggerPunchMiddle);
    }

    public void PlayPunchLeft()
    {
        if (animator == null) return;
        animator.SetTrigger(triggerPunchLeft);
    }

    public void PlayUltimatePunch(int index)
    {
        int pattern = index % 3;

        switch (pattern)
        {
            case 0: animator.SetTrigger(triggerPunchRight); break;
            case 1: animator.SetTrigger(triggerPunchMiddle); break;
            default: animator.SetTrigger(triggerPunchLeft); break;
        }
    }
}