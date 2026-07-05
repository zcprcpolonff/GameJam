using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Office 场景入场流程：
/// 首次进入 → 冻结 1s → 4 句开场独白
/// 从 Desktop 回来 → 冻结 1s → 2 句返回独白
/// </summary>
public class OfficeIntroFlow : MonoBehaviour
{
    private static bool hasPlayedIntro = false;
    private static bool hasPlayedReturn = false;

    [Header("首次入场对话（4句）")]
    public List<DialogueLine> introDialogue = new List<DialogueLine>();

    [Header("从 Desktop 返回后对话（2句）")]
    public List<DialogueLine> returnDialogue = new List<DialogueLine>();

    private void Start()
    {
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        bool fromDesktop = Interactable.HasTriggeredDesktop && !hasPlayedReturn;

        if (hasPlayedIntro && !fromDesktop) yield break;

        if (!hasPlayedIntro)
        {
            hasPlayedIntro = true;
            yield return StartCoroutine(PlayDialogue(introDialogue));
        }
        else if (fromDesktop)
        {
            hasPlayedReturn = true;
            yield return StartCoroutine(PlayDialogue(returnDialogue));
        }
    }

    private IEnumerator PlayDialogue(List<DialogueLine> lines)
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.FreezePlayer();

        yield return new WaitForSeconds(1f);

        if (lines != null && lines.Count > 0 && DialogueController.Instance != null)
        {
            DialogueController.Instance.StartDialogue(lines);
        }

        yield return new WaitWhile(() =>
            DialogueController.Instance != null &&
            DialogueController.Instance.IsDialogueActive
        );

        if (PlayerController.Instance != null)
            PlayerController.Instance.UnfreezePlayer();
    }
}
