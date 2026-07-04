using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct DialogueLine
{
    public string name;            // 说话者名字
    [TextArea(2, 5)]
    public string text;            // 对话内容
    public Sprite characterSprite; // 人物立绘
}

public class DialogueController : MonoBehaviour
{
    // 单例模式，让互动点可以随时随地调用对话框
    public static DialogueController Instance { get; private set; }

    [Header("UI 元素引用")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;

    [Header("当前对话数据")]
    private List<DialogueLine> currentLines = new List<DialogueLine>();
    private int currentIndex = 0;
    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 游戏一开始，确保对话框是隐藏的
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // 如果对话框是开启的，并且玩家按下了鼠标左键
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextLine();
        }
    }

    // 【核心接口】开始对话
    public void StartDialogue(List<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0) return;

        currentLines = lines;
        currentIndex = 0;
        isDialogueActive = true;
        
        dialoguePanel.SetActive(true); // 显示对话框

        DisplayNextLine(); // 立刻显示第一行
    }

    // 显示下一行文字
    public void DisplayNextLine()
    {
        // 如果所有对话都放完了
        if (currentIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        // 取出当前这一行的数据
        DialogueLine line = currentLines[currentIndex];

        nameText.text = line.name;
        dialogueText.text = line.text;

        // 处理立绘：如果有图片就显示，没有就隐藏
        if (line.characterSprite != null)
        {
            characterImage.gameObject.SetActive(true);
            characterImage.sprite = line.characterSprite;
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }

        currentIndex++; // 准备播放下一句
    }

    // 结束对话
    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false); // 隐藏对话框

        // 【关键点】对话结束了，通知玩家脚本恢复移动！
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UnfreezePlayer();
        }

        Debug.Log("对话全部结束，解冻玩家。");
    }
}