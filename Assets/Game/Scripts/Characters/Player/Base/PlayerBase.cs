using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
    public int PlayerIndex { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerWeapon Weapon { get; private set; }
    public VidaJugador Vida { get; private set; }

    protected virtual void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Weapon = GetComponent<PlayerWeapon>();
        Vida = GetComponent<VidaJugador>();
    }

    public virtual void Initialize(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }

    public abstract void HandleInput(
        Vector2 move,
        bool jump,
        bool dash,
        bool attack1Down, bool attack1Held, bool attack1Up,
        bool attack2Down, bool attack2Held, bool attack2Up,
        bool attack3Down, bool attack3Held, bool attack3Up,
        bool ultiDown, bool ultiHeld, bool ultiUp
        );

}
