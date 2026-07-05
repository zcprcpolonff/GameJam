using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct DialogueLine
{
    public string name;
    [TextArea(2, 5)]
    public string text;
    public Sprite characterSprite;
}

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [Header("UI 元素引用")]
    public GameObject dialogueRoot; // 整个对话 UI 的父级容器
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;

    [Header("当前对话数据")]
    private List<DialogueLine> currentLines = new List<DialogueLine>();
    private int currentIndex = 0;
    private bool isDialogueActive = false;
    public bool IsDialogueActive => isDialogueActive;

    private Interactable currentTriggerSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        if (dialogueRoot == null && dialoguePanel != null)
        {
            dialogueRoot = dialoguePanel;
        }
    }

    private void Start()
    {
        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            DisplayNextLine();
        }
    }

    public void StartDialogue(List<DialogueLine> lines, Interactable source = null)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("没有可显示的对话内容。 ");
            return;
        }

        currentTriggerSource = source;
        currentLines = new List<DialogueLine>(lines);
        currentIndex = 0;
        isDialogueActive = true;

        if (dialogueRoot == null && dialoguePanel != null)
        {
            dialogueRoot = dialoguePanel;
        }

        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(true);
        }
        else
        {
            Debug.LogWarning("DialogueController 没有绑定 dialogueRoot 或 dialoguePanel。无法显示对话 UI。");
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (currentIndex >= currentLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines[currentIndex];

        if (nameText != null) nameText.text = line.name;
        if (dialogueText != null) dialogueText.text = line.text;

        if (characterImage != null)
        {
            if (line.characterSprite != null)
            {
                characterImage.gameObject.SetActive(true);
                characterImage.sprite = line.characterSprite;
            }
            else
            {
                characterImage.gameObject.SetActive(false);
            }
        }

        currentIndex++;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialogueRoot == null && dialoguePanel != null)
        {
            dialogueRoot = dialoguePanel;
        }

        if (dialogueRoot != null)
        {
            dialogueRoot.SetActive(false);
        }

        PlayerController player = PlayerController.Instance;
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }

        if (player != null)
        {
            player.UnfreezePlayer();
        }

        Debug.Log("对话全部结束，解冻玩家。");
    }
}