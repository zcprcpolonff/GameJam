using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class SceneTransferTrigger : MonoBehaviour
{
    [Header("传送配置")]
    [Tooltip("目标场景的名称，必须与 Build Settings 中的名称完全一致")]
    [SerializeField] private string targetSceneName;

    [Tooltip("目标场景中 point 的 GameObject 名称，传送到该位置（为空则沿用 GameManager 保存的位置）")]
    [SerializeField] private string targetPointName;

    private bool isTransferring = false;

    private void Awake()
    {
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.PlayerJustRestored) return;

        if (collision.CompareTag("Player") && !isTransferring)
        {
            PlayerController player = PlayerController.Instance;
            if (player != null)
            {
                ExecuteSceneTransfer(player);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 找不到 PlayerController 实例！");
            }
        }
    }

    private void ExecuteSceneTransfer(PlayerController player)
    {
        isTransferring = true;
        player.FreezePlayer();

        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneTransferTrigger] {gameObject.name} → {targetSceneName} (point: {targetPointName ?? "无"})");

        if (!string.IsNullOrEmpty(targetPointName))
        {
            GameManager.TransferViaPortal(currentScene, targetSceneName, targetPointName);
        }
        else
        {
            GameManager.LoadScene(currentScene, targetSceneName);
        }
    }
}
