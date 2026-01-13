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
        bool attack3Down, bool attack3Held, bool attack3Up
    )
    {
        if (Movement != null)
            Movement.SetInput(move.x, move.y, jump, dash);

        if (Weapon != null)
        {
            bool fireDown = attack1Down;
            bool fireHeld = attack1Held;
            bool fireUp = attack1Up;

            bool minePressed = attack2Down; 
            bool machinePressed = attack3Down;

            Weapon.SetInput(
                fireDown,
                fireHeld,
                fireUp,
                minePressed,
                machinePressed
            );
        }
    }
}
