using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public enum PlayerControlsType
{
    KeyboardMouse,
    GamePad,
    Playstation
}

[RequireComponent(typeof(PlayerBase))]
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private PlayerControlsType controlType = PlayerControlsType.KeyboardMouse;
    [SerializeField] private int gamepadIndex = 0;

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

        ConfigureDevices();
        player.Initialize(playerIndex);
    }

    private void ConfigureDevices()
    {
        actions.devices = null;

        switch (controlType)
        {
            case PlayerControlsType.KeyboardMouse:
                {
                    var kb = Keyboard.current;
                    var ms = Mouse.current;

                    var devicesList = new System.Collections.Generic.List<InputDevice>();
                    if (kb != null) devicesList.Add(kb);
                    if (ms != null) devicesList.Add(ms);

                    actions.devices = devicesList.Count > 0 ? devicesList.ToArray() : System.Array.Empty<InputDevice>();
                    break;
                }

            case PlayerControlsType.GamePad:
                {
                    Gamepad pad = null;

                    if (Gamepad.all.Count > 0)
                    {
                        int index = Mathf.Clamp(gamepadIndex, 0, Gamepad.all.Count - 1);
                        pad = Gamepad.all[index];
                    }

                    actions.devices = pad != null ? new InputDevice[] { pad } : System.Array.Empty<InputDevice>();
                    break;
                }

            case PlayerControlsType.Playstation:
                {
                    DualShockGamepad ps4 = DualShockGamepad.current;

                    if (ps4 == null)
                    {
                        foreach (var pad in Gamepad.all)
                        {
                            if (pad is DualShockGamepad dualShock)
                            {
                                ps4 = dualShock;
                                break;
                            }
                        }
                    }

                    actions.devices = ps4 != null ? new InputDevice[] { ps4 } : System.Array.Empty<InputDevice>();
                    break;
                }
        }
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

        bool shiftHeld = false;
        if (controlType == PlayerControlsType.KeyboardMouse && Keyboard.current != null)
        {
            shiftHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }

        bool ultiDown = shiftHeld && a1Down;
        bool ultiHeld = shiftHeld && a1Held;
        bool ultiUp = shiftHeld && a1Up;

        player.HandleInput(
            move,
            jump,
            dash,
            a1Down, a1Held, a1Up,
            a2Down, a2Held, a2Up,
            a3Down, a3Held, a3Up,
            ultiDown, ultiHeld, ultiUp
        );
    }
}
