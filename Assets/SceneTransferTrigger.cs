using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransferTrigger : MonoBehaviour
{
    [Header("传送配置")]
    [Tooltip("目标场景的名称，必须与 Build Settings 中的名称完全一致")]
    [SerializeField] private string targetSceneName;

    private bool isTransferring = false; // 防止单帧内多次触发

    private void Awake()
    {
        // 自动确保 Collider 的配置正确
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 刚恢复过位置 → 不算"走进来"，必须走出去再进来
        if (GameManager.PlayerJustRestored) return;

        // 核心触发：检查碰撞体是否为玩家，且当前没有在传送中
        if (collision.CompareTag("Player") && !isTransferring)
        {
            // 检查单例是否存在，防止空引用报错
            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                ExecuteSceneTransfer(player);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 找不到 PlayerController 实例！请确保玩家物体已正确初始化。");
            }
        }
    }

    private void ExecuteSceneTransfer(PlayerController player)
    {
        isTransferring = true;

        // 2. 状态防护：立刻冻结玩家操作与物理状态
        player.FreezePlayer();
        
        Debug.Log($"玩家已触发传送，正在加载目标场景: {targetSceneName}");

        // 3. 通过 GameManager 跳转（自动保存当前位置）
        GameManager.LoadScene(SceneManager.GetActiveScene().name, targetSceneName);
    }
}