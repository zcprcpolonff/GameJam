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
    private float horizontalInput;

    private bool isGrounded;          // 是否在地面上
    private bool jumpRequested;       // 是否按下了跳跃键
 
    void Start()
    {
        // 获取人物身上的刚体组件
        rb = GetComponent<Rigidbody2D>();
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

    // 在 Scene 视图里画一个红色的圈圈，方便我们调试看清脚底的检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}