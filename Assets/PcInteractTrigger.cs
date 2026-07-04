using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 电脑交互触发器：首次按 E 跳转 Desktop 场景，之后不再响应。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PcInteractTrigger : MonoBehaviour
{
    public static bool HasTriggeredDesktop { get; private set; }

    [Header("场景跳转")]
    public string desktopScene = "Desktop";

    private bool isPlayerInRange;

    void Start()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        if (HasTriggeredDesktop) return;
        if (!isPlayerInRange) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        HasTriggeredDesktop = true;
        SceneManager.LoadScene(desktopScene);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other)) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other)) isPlayerInRange = false;
    }

    private bool IsPlayer(Collider2D c)
    {
        if (c == null) return false;
        if (c.CompareTag("Player")) return true;
        if (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag("Player")) return true;
        if (c.transform.root != null && c.transform.root.CompareTag("Player")) return true;
        return false;
    }
}
