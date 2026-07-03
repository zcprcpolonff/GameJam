using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool isPlayerInZone = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        // 当玩家在区域内，且按下 E 键时触发交互
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        // 1. 控制台弹出提示（满足你的核心需求）
        Debug.Log("【控制台提示】: 交互成功！你成功触发了该交互点。");
        
        // 2. 顺便给一个Jam常用的视觉反馈：变绿！
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }
    }

    // 玩家走入光圈
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            Debug.Log("【系统】: 接近交互点，请按下 [E] 键进行交互");
        }
    }

    // 玩家离开光圈
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            Debug.Log("【系统】: 离开了交互范围");
            
            // 离开后恢复原来的颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}