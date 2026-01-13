using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerBase))]
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;

    private PlayerBase player;
    private InputSystem_Actions actions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attack1Action;
    private InputAction attack2Action;
    private InputAction attack3Action;

    private void Awake()
    {
        player = GetComponent<PlayerBase>();
        actions = new InputSystem_Actions();

        if (player != null)
            player.Initialize(playerIndex);
    }

    private void OnEnable()
    {
        var map = actions.Player;

        moveAction = map.Move;
        jumpAction = map.Jump;
        dashAction = map.Sprint;
        attack1Action = map.Attack1;
        attack2Action = map.Attack2;
        attack3Action = map.Attack3;

        map.Enable();
    }

    private void OnDisable()
    {
        actions.Player.Disable();
    }

    private void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        bool jump = jumpAction.WasPressedThisFrame();
        bool dash = dashAction.WasPressedThisFrame();

        float a1Val = attack1Action.ReadValue<float>();
        bool a1Down = attack1Action.WasPressedThisFrame();
        bool a1Up = attack1Action.WasReleasedThisFrame();
        bool a1Held = a1Val > 0.1f;

        float a2Val = attack2Action.ReadValue<float>();
        bool a2Down = attack2Action.WasPressedThisFrame();
        bool a2Up = attack2Action.WasReleasedThisFrame();
        bool a2Held = a2Val > 0.1f;

        float a3Val = attack3Action.ReadValue<float>();
        bool a3Down = attack3Action.WasPressedThisFrame();
        bool a3Up = attack3Action.WasReleasedThisFrame();
        bool a3Held = a3Val > 0.1f;

        if (player != null)
        {
            player.HandleInput(
                move,
                jump,
                dash,
                a1Down, a1Held, a1Up,
                a2Down, a2Held, a2Up,
                a3Down, a3Held, a3Up
            );
        }
    }
}
