using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Desktop 场景主控制器：
/// 
/// 初始：只显示 wechat + 1，隐藏 2/3/4/start/quit
/// 点击 wechat → 隐藏 wechat+1，显示 2
/// 停留 4s → 隐藏 2，显示 3 + start + quit
/// 点击 quit → 隐藏 3 + start + quit，显示 4
/// 停留 5s → 回到 Office
/// </summary>
public class DesktopController : MonoBehaviour
{
    [Header("场景物体")]
    public GameObject img1;
    public GameObject img2;
    public GameObject img3;
    public GameObject img4;
    public GameObject wechatBtn;
    public GameObject startBtn;
    public GameObject quitBtn;

    [Header("配置")]
    public string officeScene = "Office";
    public float holdDuration = 4f;
    public float quitDelay = 5f;

    private Collider2D wechatCollider;
    private Collider2D quitCollider;
    private MenuButton quitMenuBtn;
    private MenuButton startMenuBtn;

    private bool sequenceStarted;
    private bool stage3Active;
    private bool quitClicked;

    void Start()
    {
        // 初始：只显示 wechat + 1
        if (img2 != null) img2.SetActive(false);
        if (img3 != null) img3.SetActive(false);
        if (img4 != null) img4.SetActive(false);
        if (startBtn != null) startBtn.SetActive(false);
        if (quitBtn != null) quitBtn.SetActive(false);
        if (wechatBtn != null) wechatBtn.SetActive(true);
        if (img1 != null) img1.SetActive(true);

        wechatCollider = wechatBtn != null ? wechatBtn.GetComponent<Collider2D>() : null;
        quitCollider = quitBtn != null ? quitBtn.GetComponent<Collider2D>() : null;

        // quit 的 MenuButton 不要自动跳转
        if (quitBtn != null)
        {
            quitMenuBtn = quitBtn.GetComponent<MenuButton>();
            if (quitMenuBtn != null) quitMenuBtn.sceneToLoad = "";
        }

        // start 完全无效果
        if (startBtn != null)
        {
            startMenuBtn = startBtn.GetComponent<MenuButton>();
            if (startMenuBtn != null) startMenuBtn.sceneToLoad = "";
        }
    }

    void Update()
    {
        if (Camera.main == null) return;
        Vector2 mp = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 阶段1：点击 wechat
        if (!sequenceStarted && wechatCollider != null)
        {
            if (wechatCollider.OverlapPoint(mp) && Input.GetMouseButtonDown(0))
            {
                sequenceStarted = true;
                StartCoroutine(WechatSequence());
            }
        }

        // 阶段3：点击 quit
        if (stage3Active && !quitClicked && quitCollider != null)
        {
            if (quitCollider.OverlapPoint(mp) && Input.GetMouseButtonDown(0))
            {
                quitClicked = true;
                StartCoroutine(QuitSequence());
            }
        }
    }

    private IEnumerator WechatSequence()
    {
        if (wechatBtn != null) wechatBtn.SetActive(false);
        if (img1 != null) img1.SetActive(false);
        if (img2 != null) img2.SetActive(true);

        yield return new WaitForSeconds(holdDuration);

        if (img2 != null) img2.SetActive(false);
        if (img3 != null) img3.SetActive(true);
        if (startBtn != null) startBtn.SetActive(true);
        if (quitBtn != null) quitBtn.SetActive(true);

        stage3Active = true;
    }

    private IEnumerator QuitSequence()
    {
        if (img3 != null) img3.SetActive(false);
        if (startBtn != null) startBtn.SetActive(false);
        if (quitBtn != null) quitBtn.SetActive(false);

        if (img4 != null) img4.SetActive(true);

        yield return new WaitForSeconds(quitDelay);

        GameManager.LoadScene("Desktop", officeScene);
    }
}
