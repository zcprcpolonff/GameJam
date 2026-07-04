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
    private DistanceJoint2D ropeJoint;
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
        // 获取人物身上的刚体组件
        rb = GetComponent<Rigidbody2D>();

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

        // 2. 在 Update 中检测是否在地面上
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        // 3. 在 Update 中检测是否按下了跳跃键
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }

        if (anchorTransform != null)
        {
            float deltaY = Mathf.Abs(rb.position.y - anchorTransform.position.y);
            
            // 如果高度差已经大于绳长，说明根本够不着地（特殊情况，直接按原逻辑卡死）
            if (deltaY >= maxRopeLength) return;

            // 【数学魔法】利用勾股定理计算出当前高度下，最大允许的 X 轴偏移量
            // maxDeltaX = sqrt(RopeLength^2 - deltaY^2)
            float maxDeltaX = Mathf.Sqrt((maxRopeLength * maxRopeLength) - (deltaY * deltaY));

            float currentDeltaX = rb.position.x - anchorTransform.position.x;

            // 3. 只强行截断 X 轴，完全把 Y 轴留给重力和地面碰撞！
            if (Mathf.Abs(currentDeltaX) > maxDeltaX)
            {
                float clampedX = anchorTransform.position.x + (Mathf.Sign(currentDeltaX) * maxDeltaX);
                rb.position = new Vector2(clampedX, rb.position.y);
                
                // 把往外走的 X 速度清零
                if ((currentDeltaX > 0 && rb.velocity.x > 0) || (currentDeltaX < 0 && rb.velocity.x < 0))
                {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }
        }
    }

    void FixedUpdate()
    {
        // 2. 在 FixedUpdate 中处理物理移动，保证在不同帧率的电脑上速度一致
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // 4. 在 FixedUpdate 中处理跳跃逻辑
        if (jumpRequested)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }
}