using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactable : MonoBehaviour
{
    [Header("互动设置")]
    public KeyCode interactKey = KeyCode.E;

    [Header("──────────────── Phase 1 ────────────────")]
    public List<DialogueLine> npcDialogue = new List<DialogueLine>();
    public string unlockFlagPhase1 = "";    // 需要这个标记为 true 才能触发 Phase1
    public float extendRopeAmount = 0f;
    public bool markAsGotReport = false;
    public string setFlagPhase1 = "";       // 对话结束后设置的标记

    [Header("──────────────── Phase 2 ────────────────")]
    public List<DialogueLine> phase2Dialogue = new List<DialogueLine>();
    public string unlockFlagPhase2 = "";    // 需要这个标记为 true 才能触发
    public float extendRopePhase2 = 0f;
    public string setFlagPhase2 = "";

    [Header("──────────────── Phase 3 ────────────────")]
    public List<DialogueLine> phase3Dialogue = new List<DialogueLine>();
    public string unlockFlagPhase3 = "";
    public float extendRopePhase3 = 0f;
    public string setFlagPhase3 = "";

    [Header("──────────────── Phase 4 ────────────────")]
    public List<DialogueLine> phase4Dialogue = new List<DialogueLine>();
    public string unlockFlagPhase4 = "";
    public float extendRopePhase4 = 0f;
    public string setFlagPhase4 = "";

    [Header("延迟条件绳子（逗号分隔的 flag 列表，全部满足后增长）")]
    public string contingentRopeFlags = "";
    public float contingentRopeAmount = 0f;

    [Header("条件限制")]
    public bool requireDesktopVisit = false;

    [Header("场景跳转（无对话时生效）")]
    public string jumpToScene = "";
    public bool jumpOnce = true;

    [Header("提示 UI")]
    public GameObject HintCanvas;

    // 运行时状态
    private bool isPlayerInRange = false;
    private bool hasInteracted = false;
    private static bool didJumpToDesktop = false;
    private int phasesPlayed = 0;   // 0=none, 1=phase1 done, 2=phase2 done, 3=phase3 done

    void Start()
    {
        if (HintCanvas != null) HintCanvas.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
            TriggerInteraction();
    }

    void TriggerInteraction()
    {
        // ── Phase 4 ──
        if (phasesPlayed < 4 && CanTriggerPhase(phase4Dialogue, unlockFlagPhase4, 3))
        {
            phasesPlayed = 4;
            Debug.Log($"<color=cyan>[Interaction]</color> Phase4 对话触发");
            PlayDialogue(phase4Dialogue, extendRopePhase4, setFlagPhase4);
            return;
        }
        // ── Phase 3 ──
        if (phasesPlayed < 3 && CanTriggerPhase(phase3Dialogue, unlockFlagPhase3, 2))
        {
            phasesPlayed = 3;
            Debug.Log($"<color=cyan>[Interaction]</color> Phase3 对话触发");
            PlayDialogue(phase3Dialogue, extendRopePhase3, setFlagPhase3);
            return;
        }
        // ── Phase 2 ──
        if (phasesPlayed < 2 && CanTriggerPhase(phase2Dialogue, unlockFlagPhase2, 1))
        {
            phasesPlayed = 2;
            Debug.Log($"<color=cyan>[Interaction]</color> Phase2 对话触发");
            PlayDialogue(phase2Dialogue, extendRopePhase2, setFlagPhase2);
            return;
        }
        // ── Phase 1 ──
        if (hasInteracted) return;
        if (phasesPlayed > 0) return; // 避免退出重进再触发 Phase1
        if (requireDesktopVisit && !GameManager.HasVisitedDesktop) return;
        if (!string.IsNullOrEmpty(unlockFlagPhase1) && !GameManager.GetFlag(unlockFlagPhase1)) return;

        bool noDialogue = (npcDialogue == null || npcDialogue.Count == 0);
        bool hasJump = !string.IsNullOrEmpty(jumpToScene);

        if (noDialogue && !hasJump) return;

        // 场景跳转
        if (noDialogue && hasJump)
        {
            if (jumpOnce && didJumpToDesktop) return;
            didJumpToDesktop = true;
            GameManager.HasVisitedDesktop = true;
            hasInteracted = true;
            GameManager.LoadScene(SceneManager.GetActiveScene().name, jumpToScene);
            return;
        }

        // 正常对话
        hasInteracted = true;
        phasesPlayed = 1;
        PlayDialogue(npcDialogue, extendRopeAmount, setFlagPhase1);
    }

    private bool CanTriggerPhase(List<DialogueLine> lines, string unlockFlag, int requiredPhase)
    {
        if (phasesPlayed > requiredPhase) return false;
        if (lines == null || lines.Count == 0) return false;
        if (!string.IsNullOrEmpty(unlockFlag) && !GameManager.GetFlag(unlockFlag)) return false;
        return true;
    }

    private void PlayDialogue(List<DialogueLine> lines, float ropeAdd, string setFlag)
    {
        Debug.Log($"<color=cyan>[Interaction]</color> 互动成功！");

        PlayerController player = GetPlayerController();
        if (player != null) player.FreezePlayer();

        DialogueController dc = GetDialogueController();
        if (dc != null) dc.StartDialogue(lines);
        else if (player != null) player.UnfreezePlayer();

        if (ropeAdd != 0f)
            StartCoroutine(ExtendAfterDialogue(player, ropeAdd));

        if (markAsGotReport && phasesPlayed <= 1)
            StartCoroutine(SetGotReportAfterDialogue());

        if (!string.IsNullOrEmpty(setFlag))
            StartCoroutine(SetFlagAfterDialogue(setFlag));

        if (!string.IsNullOrEmpty(contingentRopeFlags) && contingentRopeAmount != 0f)
            GameManager.RegisterContingentRope(contingentRopeFlags, contingentRopeAmount);

        if (HintCanvas != null) HintCanvas.SetActive(false);
    }

    private System.Collections.IEnumerator SetFlagAfterDialogue(string flag)
    {
        yield return new WaitWhile(() =>
            DialogueController.Instance != null &&
            DialogueController.Instance.IsDialogueActive
        );
        GameManager.SetFlag(flag);
    }

    private System.Collections.IEnumerator SetGotReportAfterDialogue()
    {
        yield return new WaitWhile(() =>
            DialogueController.Instance != null &&
            DialogueController.Instance.IsDialogueActive
        );
        GameManager.HasGotReport = true;
        Debug.Log("[Interactable] HasGotReport 已设置");
    }

    private System.Collections.IEnumerator ExtendAfterDialogue(PlayerController player, float amount)
    {
        yield return new WaitWhile(() =>
            DialogueController.Instance != null &&
            DialogueController.Instance.IsDialogueActive
        );
        if (player != null)
        {
            GameManager.ExtendRope(amount);
            Debug.Log($"[Interactable] 绳子增长 +{amount}，当前: {player.maxRopeLength}");
        }
    }

    private PlayerController GetPlayerController()
    {
        if (PlayerController.Instance != null) return PlayerController.Instance;
        return FindObjectOfType<PlayerController>();
    }

    private DialogueController GetDialogueController()
    {
        if (DialogueController.Instance != null) return DialogueController.Instance;
        return FindObjectOfType<DialogueController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayerCollider(other))
        {
            if (requireDesktopVisit && !GameManager.HasVisitedDesktop) return;
            if (!string.IsNullOrEmpty(unlockFlagPhase1) && !GameManager.GetFlag(unlockFlagPhase1)) return;
            if (phasesPlayed >= CountTotalPhases()) return;

            isPlayerInRange = true;
            if (HintCanvas != null) HintCanvas.SetActive(true);
        }
    }

    private int CountTotalPhases()
    {
        int c = 0;
        if (npcDialogue != null && npcDialogue.Count > 0) c++;
        if (phase2Dialogue != null && phase2Dialogue.Count > 0) c++;
        if (phase3Dialogue != null && phase3Dialogue.Count > 0) c++;
        if (phase4Dialogue != null && phase4Dialogue.Count > 0) c++;
        if (!string.IsNullOrEmpty(jumpToScene)) c++;
        return Mathf.Max(c, 1);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayerCollider(other))
        {
            isPlayerInRange = false;
            if (HintCanvas != null) HintCanvas.SetActive(false);
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

    public static bool HasTriggeredDesktop { get { return GameManager.HasVisitedDesktop || didJumpToDesktop; } }
}
