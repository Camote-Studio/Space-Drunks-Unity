using UnityEngine;

public class LocalPlayer : PlayerBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void HandleInput(
        Vector2 move,
        bool jump,
        bool dash,
        bool attack1Down, bool attack1Held, bool attack1Up,
        bool attack2Down, bool attack2Held, bool attack2Up,
        bool attack3Down, bool attack3Held, bool attack3Up,
        bool ultiDown, bool ultiHeld, bool ultiUp
    )
    {
        if (Movement != null)
            Movement.SetInput(move.x, move.y, jump, dash);

        if (Weapon != null)
        {
            Weapon.SetInput(
                attack1Down, attack1Held, attack1Up,
                attack2Down, attack2Held, attack2Up,
                attack3Down, attack3Held, attack3Up,
                ultiDown, ultiHeld, ultiUp
            );
        }
    }
}
