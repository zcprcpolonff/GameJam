using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Reception 1 场景入场流程控制器：
/// 冻结 1s → 自动播放前台对话 → 对话结束 → 冻结 1s → 纯黑 → 渐入图 → 停留 → 渐出 → 纯黑 → Office
/// </summary>
public class Reception1IntroFlow : MonoBehaviour
{
    [Header("转场全屏图")]
    public Sprite transitionSprite;

    [Header("对话数据")]
    [Tooltip("拖入前台 NPC (InteractChatbox) 的 Interactable 组件；留空则自动查找")]
    public Interactable receptionNpc;

    [Header("跳转配置")]
    public string targetScene = "Office";

    [Header("时间配置")]
    public float fadeInDuration = 0.5f;
    public float holdDuration = 3f;
    public float fadeOutDuration = 0.5f;

    private void Start()
    {
        if (receptionNpc == null)
        {
            receptionNpc = FindReceptionNpc();
        }

        StartCoroutine(IntroSequence());
    }

    private Interactable FindReceptionNpc()
    {
        foreach (var npc in FindObjectsOfType<Interactable>())
        {
            if (npc.gameObject.name.Contains("InteractChatbox"))
                return npc;
        }
        return null;
    }

    private IEnumerator IntroSequence()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.FreezePlayer();

        yield return new WaitForSeconds(1f);

        List<DialogueLine> lines = null;
        if (receptionNpc != null && receptionNpc.npcDialogue != null && receptionNpc.npcDialogue.Count > 0)
            lines = receptionNpc.npcDialogue;

        if (lines != null && DialogueController.Instance != null)
            DialogueController.Instance.StartDialogue(lines);

        yield return new WaitWhile(() =>
            DialogueController.Instance != null && DialogueController.Instance.IsDialogueActive
        );

        yield return new WaitForSeconds(1f);

        // ===== 转场 =====
        var go = new GameObject("TransitionOverlay");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        go.AddComponent<GraphicRaycaster>();

        // 底层黑底
        var blackGo = new GameObject("Black");
        blackGo.transform.SetParent(go.transform, false);
        var black = blackGo.AddComponent<Image>();
        black.color = Color.black;
        var br = black.rectTransform;
        br.anchorMin = Vector2.zero; br.anchorMax = Vector2.one;
        br.offsetMin = Vector2.zero; br.offsetMax = Vector2.zero;

        // 上层转场图
        var imgGo = new GameObject("TransitionImg");
        imgGo.transform.SetParent(go.transform, false);
        var img = imgGo.AddComponent<Image>();
        var cg = imgGo.AddComponent<CanvasGroup>();
        img.raycastTarget = false;
        if (transitionSprite != null) { img.sprite = transitionSprite; img.color = Color.white; }
        else img.color = Color.clear;
        var ir = img.rectTransform;
        ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
        ir.offsetMin = Vector2.zero; ir.offsetMax = Vector2.zero;

        // 纯黑 0.3s
        cg.alpha = 0f;
        yield return new WaitForSeconds(0.3f);

        // 渐入
        yield return Fade(cg, 0f, 1f, fadeInDuration);

        // 停留
        yield return new WaitForSeconds(holdDuration);

        // 异步加载 Office
        GameManager.SaveSceneState("Reception 1");
        var op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        // 渐出 → 纯黑
        yield return Fade(cg, 1f, 0f, fadeOutDuration);

        yield return new WaitForSeconds(0.3f);

        op.allowSceneActivation = true;
    }

    private IEnumerator Fade(CanvasGroup cg, float a, float b, float d)
    {
        if (d <= 0f) { cg.alpha = b; yield break; }
        float t = 0f;
        while (t < d) { t += Time.deltaTime; cg.alpha = Mathf.Lerp(a, b, t / d); yield return null; }
        cg.alpha = b;
    }
}
