using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全局持久化管理器：
/// - 每个场景独立的绳子长度
/// - 场景间玩家位置保留与恢复
/// - Desktop 访问标记 / 通用 Flag 系统
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 场景名称 → 该场景当前的绳子长度
    private static System.Collections.Generic.Dictionary<string, float> sceneRopeLengths
        = new System.Collections.Generic.Dictionary<string, float>();

    // 场景名称 → 离开该场景时玩家的位置
    private static System.Collections.Generic.Dictionary<string, Vector2> savedPositions
        = new System.Collections.Generic.Dictionary<string, Vector2>();

    public static bool HasVisitedDesktop { get; set; }
    public static bool HasGotReport { get; set; }

    // 通用 Flag
    private static System.Collections.Generic.Dictionary<string, bool> flags
        = new System.Collections.Generic.Dictionary<string, bool>();
    public static void SetFlag(string key) { flags[key] = true; Debug.Log($"[GameManager] Flag ↑ {key}"); CheckContingentRopes(); }
    public static bool GetFlag(string key) { return !string.IsNullOrEmpty(key) && flags.ContainsKey(key) && flags[key]; }

    public static float TeleportCooldownUntil { get; private set; }

    /// <summary> 刚恢复了玩家位置（落地在传送点不算"走进来"）</summary>
    public static bool PlayerJustRestored { get; set; }

    // ─── 延迟条件绳子（等所有关联 flag 都就绪后才增长）───
    private class ContingentRope
    {
        public string[] flagNames;
        public float amount;
        public bool consumed;
    }
    private static System.Collections.Generic.List<ContingentRope> contingentRopes
        = new System.Collections.Generic.List<ContingentRope>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ============================================================
    // sceneLoaded：新场景加载后恢复状态（不在此保存！保存由 LoadScene 在跳转前完成）
    // ============================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(RestoreAfterFrame(sceneName));
            CheckContingentRopes();
        }
    }

    // ============================================================
    // PlayerController.Start() 同步查询，确保不论时序如何都能恢复
    // ============================================================
    public static float GetSavedRope(string sceneName)
    {
        if (sceneRopeLengths.TryGetValue(sceneName, out float v)) return v;
        return -1f;
    }
    public static bool TryGetSavedPosition(string sceneName, out Vector2 pos)
    {
        return savedPositions.TryGetValue(sceneName, out pos);
    }
    public static void ExtendRope(float amount)
    {
        if (PlayerController.Instance == null) return;
        string scene = SceneManager.GetActiveScene().name;
        PlayerController.Instance.ExtendRope(amount);
        sceneRopeLengths[scene] = PlayerController.Instance.maxRopeLength;
    }

    /// <summary> 注册延迟绳子：当所有逗号分隔的 flag 都为 true 时，自动增长绳子 </summary>
    public static void RegisterContingentRope(string flagNamesCsv, float amount)
    {
        if (string.IsNullOrEmpty(flagNamesCsv) || amount == 0f) return;

        var cr = new ContingentRope
        {
            flagNames = flagNamesCsv.Split(','),
            amount = amount,
            consumed = false
        };
        contingentRopes.Add(cr);
        Debug.Log($"[GameManager] 注册延迟绳子: flags=[{flagNamesCsv}] amount={amount}");

        // 立即检查一次（可能在当前帧就满足了）
        CheckContingentRopes();
    }

    private static void CheckContingentRopes()
    {
        foreach (var cr in contingentRopes)
        {
            if (cr.consumed) continue;

            bool allTrue = true;
            foreach (string name in cr.flagNames)
            {
                if (!GetFlag(name.Trim())) { allTrue = false; break; }
            }

            if (allTrue)
            {
                cr.consumed = true;
                ExtendRope(cr.amount);
                Debug.Log($"[GameManager] 条件满足，绳子 +{cr.amount}");
            }
        }
    }

    // ============================================================
    // 公开接口 —— 兼容旧调用
    // ============================================================
    public static void LoadScene(string currentScene, string targetScene)
    {
        // 主动保存（兜底，即使 activeSceneChanged 也会保存）
        SaveState(currentScene);
        SceneManager.LoadScene(targetScene);
    }

    public static void SaveSceneState(string sceneName)
    {
        SaveState(sceneName);
    }

    // ============================================================
    // 内部实现
    // ============================================================
    private static void SaveState(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning($"[GameManager] SaveState('{sceneName}') 跳过：PlayerController.Instance 为 null");
            return;
        }

        savedPositions[sceneName] = PlayerController.Instance.transform.position;
        sceneRopeLengths[sceneName] = PlayerController.Instance.maxRopeLength;
        Debug.Log($"[GameManager] 保存 [{sceneName}] pos={savedPositions[sceneName]} rope={sceneRopeLengths[sceneName]}");
    }

    private System.Collections.IEnumerator RestoreAfterFrame(string sceneName)
    {
        yield return null;
        yield return null;
        yield return null; // 三帧等待，确保所有 Start/Awake 跑完

        PlayerController pc = PlayerController.Instance;
        if (pc == null)
        {
            Debug.LogWarning($"[GameManager] RestoreAfterFrame('{sceneName}') 跳过：PlayerController.Instance 为 null");
            yield break;
        }

        // 恢复绳子
        if (sceneRopeLengths.TryGetValue(sceneName, out float savedRope))
        {
            pc.maxRopeLength = savedRope;
            if (pc.ropeVisual != null)
                pc.ropeVisual.targetPhysicsDistance = savedRope;
            Debug.Log($"[GameManager] 恢复 [{sceneName}] rope={savedRope}");
        }
        else
        {
            sceneRopeLengths[sceneName] = pc.maxRopeLength;
            Debug.Log($"[GameManager] 首次进入 [{sceneName}]，基线 rope={pc.maxRopeLength}");
        }

        // 恢复位置
        if (savedPositions.TryGetValue(sceneName, out Vector2 savedPos))
        {
            float dist = Vector2.Distance(savedPos, pc.transform.position);
            Debug.Log($"[GameManager] 尝试恢复 [{sceneName}] pos={savedPos} dist={dist}");
            if (dist < 100f && dist > 0.01f)
            {
                pc.transform.position = savedPos;
                TeleportCooldownUntil = Time.time + 0.5f;
                PlayerJustRestored = true;
                Debug.Log($"[GameManager] 恢复 [{sceneName}] pos={savedPos}");
            }
        }
    }
}
