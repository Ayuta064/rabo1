using UnityEngine;

public class BeamController : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Transform targetAnchor;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null) 
        {
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true; // ★コードで強制的にONにする！
        }
    }

    public void SetTarget(Transform target)
    {
        targetAnchor = target;
        if (lineRenderer != null) lineRenderer.enabled = true;
    }

    public void StopBeam()
    {
        targetAnchor = null;
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    void Update()
    {
        // ターゲットかLineRendererがなければ何もしない
        // ターゲット自体(HighlightObjectの親など)が非表示ならビームも消す
        if (targetAnchor == null || lineRenderer == null || !targetAnchor.gameObject.activeInHierarchy) 
        {
            if (lineRenderer != null && lineRenderer.enabled) StopBeam();
            return;
        }

        // 始点: 自分の位置（SolverHandlerで指先に追従）
        lineRenderer.SetPosition(0, transform.position);

        // 終点: ターゲットの位置
        lineRenderer.SetPosition(1, targetAnchor.position);
    }
}