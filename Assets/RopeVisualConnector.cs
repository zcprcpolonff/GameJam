using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeVisualConnector : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Target References")]
    public Transform anchorTransform;
    public Transform playerTransform;

    [Header("Rope Settings")]
    public int segments = 25;
    public float maxWaveHeight = 0.6f; // 绳子最松弛时的弯曲度

    [HideInInspector] 
    public float targetPhysicsDistance; // 从物理组件传过来的“绳子最大拉伸长度”

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments;
    }

    private void Update()
    {
        if (anchorTransform == null || playerTransform == null) return;

        DrawRope();
    }

    private void DrawRope()
    {
        Vector3 start = anchorTransform.position;
        Vector3 end = playerTransform.position;
        
        float currentActualDistance = Vector3.Distance(start, end);

        // 计算当前实际距离和物理限制距离的比例
        // 如果实际距离小于限制距离，说明绳子松了，应该下垂弯曲
        float ropeSlack = 1f;
        if (targetPhysicsDistance > 0)
        {
            ropeSlack = currentActualDistance / targetPhysicsDistance;
        }

        // 当 ropeSlack 越小（人往上荡，绳子变松），弯曲度越大
        float currentWaveHeight = Mathf.Lerp(maxWaveHeight, 0f, ropeSlack);

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // 只在绳子中间部分产生下垂效果（二次方曲线或正弦曲线）
            if (currentWaveHeight > 0.01f)
            {
                float sag = Mathf.Sin(t * Mathf.PI) * currentWaveHeight;
                point.y -= sag; // 往重力方向下垂
            }

            lineRenderer.SetPosition(i, point);
        }
    }
}