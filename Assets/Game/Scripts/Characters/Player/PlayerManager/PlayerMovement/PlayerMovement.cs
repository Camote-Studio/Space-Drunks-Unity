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
    [SerializeField] private Collider2D playerCollider;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;

    private Rigidbody2D rb;

    private float inputX;
    private float inputY;

    private bool jumpPressedThisFrame;
    private bool dashPressedThisFrame;

    private bool isJumping;
    private float jumpTime;
    private float baseVisualY;

    private bool isDashing;
    private float dashTime;
    private Vector2 dashDir;

    private bool movementLocked;
    private float facingX = 1f;
    public float FacingX => facingX;

    private int playerLayer;
    private int jumpableLayer;

    // "Encima de objeto"
    private bool isOnObject;
    private float lockedY;
    private Collider2D currentObjectCollider;
    public bool IsOnObject => isOnObject;

    public bool IsJumping => isJumping;
    public bool IsMovingHorizontally => Mathf.Abs(inputX) > 0.01f;
    public Vector2 MoveInput => new Vector2(inputX, inputY);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (visual == null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) visual = sr.transform;
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

        // Si está encima: bloquear Y y permitir salir solo saltando
        if (isOnObject)
        {
            inputY = 0f;

            if (jumpPressed && !isJumping)
            {
                ExitObject();              // restaura colisión + flags
                jumpPressedThisFrame = true;
                Debug.Log("[Salto] Saltó para salir del objeto");
            }
        }
        else
        {
            if (jumpPressed && !isJumping)
                jumpPressedThisFrame = true;
        }

        if (dashPressed && !isDashing)
            dashPressedThisFrame = true;
    }

    private void Update()
    {
        // Flip
        if (visual != null && inputX != 0f)
        {
            facingX = Mathf.Sign(inputX);
            Vector3 scale = visual.localScale;
            scale.x = Mathf.Sign(inputX) * Mathf.Abs(scale.x);
            visual.localScale = scale;
        }

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
            if (isOnObject) dir.y = 0f; // arriba solo X

            rb.linearVelocity = dir * moveSpeed;
        }

        // Anclar Y arriba
        if (isOnObject)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            Vector3 pos = transform.position;
            pos.y = lockedY;
            transform.position = pos;
        }
    }

    private void HandleJump()
    {
        if (jumpPressedThisFrame)
        {
            jumpPressedThisFrame = false;
            isJumping = true;
            jumpTime = 0f;

            Debug.Log("[Salto] Salto iniciado");

            // Si estaba encima, al saltar ya debe poder "salir"
            if (isOnObject)
                ExitObject();

            if (jumpableLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, true);
        }

        if (!isJumping) return;

        jumpTime += Time.deltaTime;
        float t = Mathf.Clamp01(jumpTime / jumpDuration);
        float h = 4f * jumpHeight * t * (1f - t);

        if (visual != null)
        {
            Vector3 lp = visual.localPosition;
            lp.y = baseVisualY + h;
            visual.localPosition = lp;
        }

        if (playerCollider != null)
            playerCollider.offset = new Vector2(playerCollider.offset.x, h);

        if (t >= 1f)
        {
            isJumping = false;

            if (visual != null)
            {
                Vector3 lp = visual.localPosition;
                lp.y = baseVisualY;
                visual.localPosition = lp;
            }

            if (playerCollider != null)
                playerCollider.offset = new Vector2(playerCollider.offset.x, 0f);

            if (jumpableLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, jumpableLayer, false);

            CheckLandingOnObject();
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

        if (!isDashing) return;

        dashTime += Time.deltaTime;
        if (dashTime >= dashDuration)
            isDashing = false;
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        Debug.Log($"[MovementLocked] => {locked} | caller:\n{System.Environment.StackTrace}");

        if (locked)
        {
            inputX = 0f;
            inputY = 0f;
            jumpPressedThisFrame = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void CheckLandingOnObject()
    {
        PropItems[] props = FindObjectsByType<PropItems>(FindObjectsSortMode.None);
        if (props == null || props.Length == 0)
            return;

        float px = playerCollider != null ? playerCollider.bounds.center.x : transform.position.x;

        foreach (var prop in props)
        {
            if (!prop.EsSaltable) continue;

            Collider2D col = prop.GetComponent<Collider2D>();
            if (col == null) continue;

            // 1) X dentro del objeto
            if (px < col.bounds.min.x || px > col.bounds.max.x) continue;

            // 2) Y cerca del LandingY (usa el rango del prop)
            float distY = Mathf.Abs(transform.position.y - prop.LandingY);
            float allowed = Mathf.Max(0.1f, prop.LandingRangeY * 0.5f);
            if (distY > allowed) continue;

            // Aterrizó
            EnterObject(prop, col);
            Debug.Log($"[Salto] Aterrizó encima de {prop.name}");
            return;
        }
    }

    private void EnterObject(PropItems prop, Collider2D col)
    {
        isOnObject = true;
        currentObjectCollider = col;
        lockedY = prop.LandingY;

        if (playerCollider != null && currentObjectCollider != null)
            Physics2D.IgnoreCollision(playerCollider, currentObjectCollider, true);

        Vector3 pos = transform.position;
        pos.y = lockedY;
        transform.position = pos;
    }

    private void ExitObject()
    {
        if (playerCollider != null && currentObjectCollider != null)
            Physics2D.IgnoreCollision(playerCollider, currentObjectCollider, false);

        isOnObject = false;
        currentObjectCollider = null;
    }
}
