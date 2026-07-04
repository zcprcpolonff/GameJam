using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("互动设置")]
    public KeyCode interactKey = KeyCode.E;
    
    // 【新增】在这个 NPC 身上配置的对话内容列表
    [Header("对话剧本")]
    public List<DialogueLine> npcDialogue = new List<DialogueLine>();

    private bool isTalking = false; // 记录当前是否正在对话中
    private bool isPlayerInRange = false; // 玩家是否在范围内
    private bool hasInteracted = false;   // 是否已经互动过（防止重复触发）

    void Update()
    {
        // 当玩家在范围内、按下了互动键、且当前没有处于互动状态时
        if (isPlayerInRange && !hasInteracted && Input.GetKeyDown(interactKey))
        {
            TriggerInteraction();
        }
    }

    // 触发互动
    void TriggerInteraction()
    {
        hasInteracted = true;
        Debug.Log("<color=cyan>[Interaction]</color> 互动成功！检测到玩家按下了 " + interactKey);

        // 调用玩家身上的方法，屏蔽操作
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.FreezePlayer();
        }

        // 2. 【核心串联】启动对话框，并把这个 NPC 的剧本传过去
        if (DialogueController.Instance != null)
        {
            DialogueController.Instance.StartDialogue(npcDialogue);
        }
    }

    // 当物体进入触发区域
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 确保碰撞到的是玩家（这里通过检查身上是否有 PlayerController 来判断，也可以用 Tag）
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
            Debug.Log("玩家进入互动范围，请按 " + interactKey + " 键互动。");
    }

    // 当物体离开触发区域
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            isTalking = false; // 离开范围重置状态
            hasInteracted = false; // 离开后重置，下次来还能互动
            Debug.Log("玩家离开了互动范围。");
        }
    }
}