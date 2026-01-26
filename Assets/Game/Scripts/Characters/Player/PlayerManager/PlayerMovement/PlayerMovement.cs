using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Salto")]
    [SerializeField] private Transform visual;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float jumpDuration = 0.4f;
    [SerializeField] private string jumpableLayerName = "JumpableObstacle";

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;

    private Rigidbody2D rb;
    private PlayerGroundChecker groundChecker;

    private float inputX;
    private float inputY;
    private bool jumpPressedThisFrame;

    private bool isJumping;
    private float jumpTime;
    private float baseVisualY;

    private bool isDashing;
    private float dashTime;
    private bool dashPressedThisFrame;
    private Vector2 dashDir;

    private bool movementLocked = false;
    private float facingX = 1f;
    public float FacingX => facingX;

    private int playerLayer;
    private int jumpableLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundChecker = GetComponent<PlayerGroundChecker>();

        rb.gravityScale = 0f;

        if (visual == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                visual = sr.transform;
        }

        if (visual != null)
            baseVisualY = visual.localPosition.y;

        playerLayer = gameObject.layer;
        jumpableLayer = LayerMask.NameToLayer(jumpableLayerName);
    }

    public void SetInput(float horizontal, float vertical, bool jumpPressed, bool dashPressed)
    {
        if (movementLocked)
        {
            inputX = 0f;
            inputY = 0f;
            return;
        }

        inputX = horizontal;
        inputY = vertical;

        if (jumpPressed && !isJumping)
            jumpPressedThisFrame = true;
        if (dashPressed && !isDashing)
            dashPressedThisFrame = true;
    }

    private void Update()
    {
        if (visual != null && inputX != 0f)
        {
            float dir = Mathf.Sign(inputX);
            facingX = dir;
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            visual.localScale = scale;
        }
        Debug.Log($"{name} pos={transform.position} visualScale={visual.localScale}");
        HandleJump();
        HandleDash();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDir * dashSpeed;
        }
        else
        {
            Vector2 dir = new Vector2(inputX, inputY).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
    }

    private void HandleJump()
    {
        if (jumpPressedThisFrame)
        {
            jumpPressedThisFrame = false;
            isJumping = true;
            jumpTime = 0f;

            if (jumpableLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, true);
        }

        if (!isJumping)
            return;

        jumpTime += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTime / jumpDuration);

        float h = 4f * jumpHeight * t * (1f - t);

        if (visual != null)
        {
            Vector3 lp = visual.localPosition;
            lp.y = baseVisualY + h;
            visual.localPosition = lp;
        }

        if (t >= 1f)
        {
            isJumping = false;

            if (visual != null)
            {
                Vector3 lp = visual.localPosition;
                lp.y = baseVisualY;
                visual.localPosition = lp;
            }

            if (jumpableLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, false);
        }
    }

    private void HandleDash()
    {
        if (dashPressedThisFrame)
        {
            dashPressedThisFrame = false;
            isDashing = true;
            dashTime = 0f;

            dashDir = new Vector2(inputX, inputY);

            if (dashDir.sqrMagnitude < 0.01f && visual != null)

                dashDir = new Vector2(Mathf.Sign(visual.localScale.x), 0f);

            dashDir = dashDir.normalized;
        }

        if (!isDashing)
            return;

        dashTime += Time.deltaTime;
        if (dashTime >= dashDuration)
            isDashing = false;
    }
    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        if (locked)
        {
            inputX = 0f;
            inputY = 0f;
            jumpPressedThisFrame = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public bool IsMovingHorizontally => Mathf.Abs(inputX) > 0.01f;

    public Vector2 MoveInput => new Vector2(inputX, inputY);
}
