using UnityEngine;

public abstract class UltimateBase : WeaponBase
{
    [Header("Ultimate Base")]
    [SerializeField] protected float presentationTime = 0.6f;

    protected bool active;
    protected bool executing;

    protected float lockedFacing = 1f;

    protected Rigidbody2D rb;
    protected PlayerMovement movement;

    public override bool IsActive => active;

    protected virtual void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        rb = movement != null ? movement.GetComponent<Rigidbody2D>() : GetComponentInParent<Rigidbody2D>();
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (!active)
        {
            if (fireDown) StartUltimate();
            return;
        }

        FreezeBody();

        if (!executing)
        {
            if (TickPresentation())
                BeginExecute();
            return;
        }

        TickExecute();
    }

    protected virtual void StartUltimate()
    {
        active = true;
        executing = false;

        lockedFacing = 1f;
        if (movement != null && movement.FacingX != 0f)
            lockedFacing = Mathf.Sign(movement.FacingX);

        if (movement != null)
            movement.enabled = false;

        FreezeBody();
        OnUltimateStart();
    }

    protected virtual bool TickPresentation()
    {
        presentationTime -= Time.deltaTime;
        return presentationTime <= 0f;
    }

    protected virtual void BeginExecute()
    {
        executing = true;
        OnUltimateExecuteStart();
    }

    protected void EndUltimate()
    {
        active = false;
        executing = false;

        if (movement != null)
            movement.enabled = true;

        OnUltimateEnd();
    }

    protected void FreezeBody()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    protected abstract void OnUltimateStart();
    protected abstract void OnUltimateExecuteStart();
    protected abstract void TickExecute();
    protected abstract void OnUltimateEnd();
}
