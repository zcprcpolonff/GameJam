using UnityEngine;
using UnityEngine.SceneManagement;

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

    public static PlayerController Instance { get; private set; }

    [HideInInspector]
    public bool canMove = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb == null)
        {
            Debug.LogError("PlayerController 需要 Rigidbody2D 组件。 ");
        }

        // 位置恢复：GameManager > 默认出生点
        string scn = SceneManager.GetActiveScene().name;
        if (GameManager.TryGetSavedPosition(scn, out Vector2 savedPos))
        {
            if (Vector2.Distance(savedPos, transform.position) < 100f && Vector2.Distance(savedPos, transform.position) > 0.01f)
            {
                transform.position = savedPos;
                Debug.Log($"[PlayerController] Start 恢复位置 [{scn}]={savedPos}");
            }
        }

        // 绳子恢复：GameManager > Inspector 默认值
        if (ropeVisual != null && anchorTransform != null)
        {
            ropeVisual.anchorTransform = anchorTransform;
            ropeVisual.playerTransform = transform;

            float restored = GameManager.GetSavedRope(scn);
            if (restored > 0f)
            {
                maxRopeLength = restored;
                Debug.Log($"[PlayerController] Start 恢复绳子 [{scn}]={restored}");
            }

            ropeVisual.targetPhysicsDistance = maxRopeLength;
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            horizontalInput = 0f;
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
            }
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 玩家主动移动时，清除"刚恢复位置"标记（传送门恢复保护）
        if (horizontalInput != 0f)
            GameManager.PlayerJustRestored = false;

        if (spriteRenderer != null)
        {
            if (horizontalInput > 0)
                spriteRenderer.flipX = false;
            else if (horizontalInput < 0)
                spriteRenderer.flipX = true;
        }

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            jumpRequested = false;
            return;
        }

        float targetVx = horizontalInput * moveSpeed;

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

    public void FreezePlayer()
    {
        canMove = false;
        horizontalInput = 0f;
        jumpRequested = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
        }

        Debug.Log("<color=yellow>[Player]</color> 玩家操作已屏蔽。");
    }

    public void UnfreezePlayer()
    {
        canMove = true;
        horizontalInput = 0f;
        Debug.Log("<color=green>[Player]</color> 玩家操作已恢复。");
    }

    public void ExtendRope(float amount)
    {
        maxRopeLength += amount;
        if (ropeVisual != null)
        {
            ropeVisual.targetPhysicsDistance = maxRopeLength;
        }
        Debug.Log($"<color=yellow>[Player]</color> 绳子长度增加 {amount}，当前: {maxRopeLength}");
    }
}
