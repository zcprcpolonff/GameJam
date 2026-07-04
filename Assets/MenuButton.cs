using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class MenuButton : MonoBehaviour
{
    public string sceneToLoad;
    public float hoverScale = 1.2f;

    private Vector3 originalScale;
    private BoxCollider2D col;
    private bool isHovering;
    private Transform trans;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;

        trans = transform;
        originalScale = trans.localScale;

        // 让碰撞体刚好包住精灵
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            col.size = sr.sprite.bounds.size;
        }
    }

    void Update()
    {
        if (Camera.main == null) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool hovering = col.OverlapPoint(mouseWorldPos);

        // 悬停放大 / 离开恢复
        if (hovering && !isHovering)
        {
            trans.localScale = originalScale * hoverScale;
        }
        else if (!hovering && isHovering)
        {
            trans.localScale = originalScale;
        }
        isHovering = hovering;

        // 点击加载场景
        if (hovering && Input.GetMouseButtonDown(0))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
