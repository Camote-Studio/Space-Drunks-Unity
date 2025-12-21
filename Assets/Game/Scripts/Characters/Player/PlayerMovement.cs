using UnityEngine;

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

    private Rigidbody2D rb;
    private PlayerGroundChecker groundChecker;

    private float inputX;
    private float inputY;
    private bool jumpPressedThisFrame;

    private bool isJumping;
    private float jumpTime;
    private float baseVisualY;

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

    public void SetInput(float horizontal, float vertical, bool jumpPressed)
    {
        inputX = horizontal;
        inputY = vertical;

        if (jumpPressed && !isJumping /* && groundChecker.IsGrounded */)
            jumpPressedThisFrame = true;
    }

    private void Update()
    {
        if (visual != null && inputX != 0f)
        {
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            visual.localScale = scale;
        }

        HandleJump();
    }

    private void FixedUpdate()
    {
        Vector2 dir = new Vector2(inputX, inputY).normalized;
        rb.linearVelocity = dir * moveSpeed;
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
}
