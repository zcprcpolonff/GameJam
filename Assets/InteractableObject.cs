using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("互动设置")]
    public KeyCode interactKey = KeyCode.E;

    [Header("对话剧本")]
    public List<DialogueLine> npcDialogue = new List<DialogueLine>();

    //拖入头顶气泡的 Canvas 物体
    [Header("提示 UI")]
    public GameObject HintCanvas;

    [Header("对话结束后的触发事件")]
    public UnityEvent onDialogueEndEvent;

    private bool isPlayerInRange = false;
    private bool hasInteracted = false;

    void Start()
    {
        // 游戏一开始，确保提示气泡是隐藏的
        if (HintCanvas != null) HintCanvas.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && !hasInteracted && Input.GetKeyDown(interactKey))
        {
            TriggerInteraction();
        }
    }

    void TriggerInteraction()
    {
        if (hasInteracted) return;

        hasInteracted = true;
        Debug.Log($"<color=cyan>[Interaction]</color> 互动成功！检测到玩家按下了 {interactKey}");

        PlayerController player = GetPlayerController();
        if (player != null)
        {
            player.FreezePlayer();
        }
        else
        {
            Debug.LogWarning("未找到 PlayerController，无法冻结玩家。 ");
        }

        DialogueController dialogueController = GetDialogueController();
        if (dialogueController != null)
        {
            dialogueController.StartDialogue(npcDialogue, this);
        }
        else
        {
            Debug.LogWarning("未找到 DialogueController，无法打开对话框。 ");
            if (player != null)
            {
                player.UnfreezePlayer();
            }
        }

        if (HintCanvas != null) HintCanvas.SetActive(false);
    }

    // 提供给 DialogueManager 调用的公共方法
    public void CompleteInteraction()
    {
        // 触发我们在 Inspector 里配置的所有事件！
        if (onDialogueEndEvent != null)
        {
            onDialogueEndEvent.Invoke();
        }
    }

    private PlayerController GetPlayerController()
    {
        if (PlayerController.Instance != null)
            return PlayerController.Instance;

        return FindObjectOfType<PlayerController>();
    }

    private DialogueController GetDialogueController()
    {
        if (DialogueController.Instance != null)
            return DialogueController.Instance;

        return FindObjectOfType<DialogueController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayerCollider(other))
        {
            isPlayerInRange = true;

            if (HintCanvas != null) 
            {
                HintCanvas.SetActive(true);
            }

            Debug.Log($"玩家进入互动范围，请按 {interactKey} 键互动。");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayerCollider(other))
        {
            isPlayerInRange = false;
            hasInteracted = false;

            if (HintCanvas != null) 
            {
                HintCanvas.SetActive(false);
            }

            Debug.Log("玩家离开了互动范围。");
        }
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null) return false;

        if (other.CompareTag("Player")) return true;
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player")) return true;
        if (other.transform.root != null && other.transform.root.CompareTag("Player")) return true;

        return false;
    }
}