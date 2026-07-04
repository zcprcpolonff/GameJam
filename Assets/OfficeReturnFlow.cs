using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 从 Desktop 返回 Office 后的入场对话：
/// 冻结 1s → 播放主角独白 2 句 → 恢复操作。
/// 只触发一次。
/// </summary>
public class OfficeReturnFlow : MonoBehaviour
{
    [Header("对话数据")]
    public List<DialogueLine> returnDialogue = new List<DialogueLine>();

    private static bool hasPlayed = false;

    private void Start()
    {
        if (!Interactable.HasTriggeredDesktop) return;
        if (hasPlayed) return;
        hasPlayed = true;

        StartCoroutine(ReturnSequence());
    }

    private IEnumerator ReturnSequence()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.FreezePlayer();

        yield return new WaitForSeconds(1f);

        if (returnDialogue != null && returnDialogue.Count > 0
            && DialogueController.Instance != null)
        {
            DialogueController.Instance.StartDialogue(returnDialogue);
        }

        yield return new WaitWhile(() =>
            DialogueController.Instance != null &&
            DialogueController.Instance.IsDialogueActive
        );

        if (PlayerController.Instance != null)
            PlayerController.Instance.UnfreezePlayer();
    }
}
