using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 8f;

    [Header("跳跃设置")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private float horizontalInput;

    private bool isGrounded;
    private bool jumpRequested;

    [Header("References")]
    public Transform anchorTransform;
    public RopeVisualConnector ropeVisual;

    [Header("Rope Limits")]
    public float maxRopeLength = 5f;
    public float climbSpeed = 3f;
    public float minRopeLength = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (ropeVisual != null && anchorTransform != null)
        {
            ropeVisual.anchorTransform = anchorTransform;
            ropeVisual.playerTransform = this.transform;
            ropeVisual.targetPhysicsDistance = maxRopeLength;
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 移动时 flipX 翻转（默认朝右，左移 flipX）
        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;

        // 驱动动画：有输入→walk，静止→idle
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        float targetVx = horizontalInput * moveSpeed;

        // 绳索 X 轴约束
        if (anchorTransform != null)
        {
            float deltaY = Mathf.Abs(rb.position.y - anchorTransform.position.y);

            if (deltaY >= maxRopeLength)
            {
                targetVx = 0f;
            }
            else
            {
                float maxDeltaX = Mathf.Sqrt((maxRopeLength * maxRopeLength) - (deltaY * deltaY));
                float currentDeltaX = rb.position.x - anchorTransform.position.x;
                float nextX = rb.position.x + targetVx * Time.fixedDeltaTime;
                float nextDeltaX = nextX - anchorTransform.position.x;

                if (Mathf.Abs(nextDeltaX) > maxDeltaX)
                {
                    if ((currentDeltaX > 0 && targetVx > 0) || (currentDeltaX < 0 && targetVx < 0))
                    {
                        targetVx = 0f;
                    }
                }
            }
        }

        rb.velocity = new Vector2(targetVx, rb.velocity.y);

        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }
}
