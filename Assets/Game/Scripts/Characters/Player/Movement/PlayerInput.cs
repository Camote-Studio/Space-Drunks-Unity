using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool jumpPressed = Input.GetButtonDown("Jump");
        bool dashPressed = Input.GetButtonDown("Dash");
        movement.SetInput(horizontal, vertical, jumpPressed, dashPressed);
    }
}
