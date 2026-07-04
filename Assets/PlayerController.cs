using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 8f;

    [Header("跳跃设置")]
    public float jumpForce = 12f;
    public Transform groundCheck;     // 地面检测点（需要我们在脚底创建一个空物体）
    public float checkRadius = 0.2f;  // 检测半径
    public LayerMask whatIsGround;    // 什么是地面（图层）
   
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private float horizontalInput;

    private bool isGrounded;          // 是否在地面上
    private bool jumpRequested;       // 是否按下了跳跃键

    [Header("References")]
    public Transform anchorTransform; // 场景中的固定点
    public RopeVisualConnector ropeVisual; // 绳子视觉脚本

    [Header("Rope Limits")]
    public float maxRopeLength = 5f; // 限制的最大移动距离
    public float climbSpeed = 3f;    // W/S 控制绳子长短的速度
    public float minRopeLength = 1.5f; 
    void Start()
    {
        // 获取人物身上的组件
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // 初始化绳子脚本
        if (ropeVisual != null && anchorTransform != null)
        {
            ropeVisual.anchorTransform = anchorTransform;
            ropeVisual.playerTransform = this.transform;
            ropeVisual.targetPhysicsDistance = maxRopeLength;
        }
    }

    void Update()
    {
        // 1. 在 Update 中每帧捕捉玩家的键盘输入（A/D 或 左右方向键）
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 根据移动方向翻转精灵（默认向右，向左时 flipX）
        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;

        // 驱动动画状态机：Speed > 0 切 walk，= 0 切 idle
        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        // 2. 在 Update 中检测是否按下了跳跃键
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    void FixedUpdate()
    {
        // 地面检测放在 FixedUpdate 里更准确，和物理同一帧
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // 处理水平移动
        float targetVx = horizontalInput * moveSpeed;

        // 绳索 X 轴约束（改为限制速度，避免直接改 position 造成抖动）
        if (anchorTransform != null)
        {
            float deltaY = Mathf.Abs(rb.position.y - anchorTransform.position.y);

            // 高度差已经超过绳长，完全拉不动了
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

                // 如果下一步会超出绳索范围，截断水平速度
                if (Mathf.Abs(nextDeltaX) > maxDeltaX)
                {
                    // 只阻止往外走的方向
                    if ((currentDeltaX > 0 && targetVx > 0) || (currentDeltaX < 0 && targetVx < 0))
                    {
                        targetVx = 0f;
                    }
                }
            }
        }

        rb.velocity = new Vector2(targetVx, rb.velocity.y);

        // 跳跃逻辑
        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }
}