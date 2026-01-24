using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
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
        switch (controlType)
        {
            case PlayerControlsType.KeyboardMouse:
                {
                    var kb = Keyboard.current;
                    var ms = Mouse.current;

                    if (kb != null || ms != null)
                    {
                        actions.devices = new InputDevice[]
                        {
                        kb,
                        ms
                        };
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerInput {playerIndex}] No hay teclado/mouse conectados");
                    }
                    break;
                }

            case PlayerControlsType.GamePad:
                {
                    Gamepad pad = null;

                    if (Gamepad.all.Count > playerIndex)
                        pad = Gamepad.all[playerIndex];
                    else
                        pad = Gamepad.current;

                    if (pad != null)
                    {
                        actions.devices = new InputDevice[] { pad };
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerInput {playerIndex}] No hay gamepad conectado");
                    }
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

                    if (ps4 != null)
                    {
                        actions.devices = new InputDevice[] { ps4 };
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerInput {playerIndex}] No hay mando de Play conectado");
                    }
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
