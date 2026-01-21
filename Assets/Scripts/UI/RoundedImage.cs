using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Image with rounded corners. Use this instead of Image component for rounded buttons.
/// Attach to any UI element that needs rounded corners.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class RoundedImage : MaskableGraphic
{
    [Header("Rounded Corners")]
    [SerializeField] private float cornerRadius = 10f;
    [SerializeField] private int cornerSegments = 8;
    
    [Header("Optional Border")]
    [SerializeField] private float borderWidth = 0f;
    [SerializeField] private Color borderColor = Color.black;
    
    public float CornerRadius
    {
        get => cornerRadius;
        set { cornerRadius = value; SetVerticesDirty(); }
    }
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        Rect rect = rectTransform.rect;
        float width = rect.width;
        float height = rect.height;
        
        // Clamp radius to half of smallest dimension
        float radius = Mathf.Min(cornerRadius, width / 2f, height / 2f);
        
        if (radius <= 0)
        {
            // Draw simple rectangle if no radius
            DrawRect(vh, rect, color);
            return;
        }
        
        // Draw rounded rectangle
        DrawRoundedRect(vh, rect, radius, color);
    }
    
    private void DrawRect(VertexHelper vh, Rect rect, Color col)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = col;
        
        vertex.position = new Vector3(rect.xMin, rect.yMin);
        vh.AddVert(vertex);
        vertex.position = new Vector3(rect.xMin, rect.yMax);
        vh.AddVert(vertex);
        vertex.position = new Vector3(rect.xMax, rect.yMax);
        vh.AddVert(vertex);
        vertex.position = new Vector3(rect.xMax, rect.yMin);
        vh.AddVert(vertex);
        
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
    }
    
    private void DrawRoundedRect(VertexHelper vh, Rect rect, float radius, Color col)
    {
        int segments = Mathf.Max(2, cornerSegments);
        
        // Center point for fan triangles
        UIVertex center = UIVertex.simpleVert;
        center.color = col;
        center.position = rect.center;
        vh.AddVert(center);
        
        int vertexCount = 1; // Start at 1 (center is 0)
        
        // Generate vertices for each corner
        // Order: Bottom-Left, Bottom-Right, Top-Right, Top-Left
        Vector2[] cornerCenters = new Vector2[]
        {
            new Vector2(rect.xMin + radius, rect.yMin + radius), // BL
            new Vector2(rect.xMax - radius, rect.yMin + radius), // BR
            new Vector2(rect.xMax - radius, rect.yMax - radius), // TR
            new Vector2(rect.xMin + radius, rect.yMax - radius)  // TL
        };
        
        float[] startAngles = { 180f, 270f, 0f, 90f };
        
        for (int corner = 0; corner < 4; corner++)
        {
            Vector2 cornerCenter = cornerCenters[corner];
            float startAngle = startAngles[corner];
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + (90f * i / segments);
                float rad = angle * Mathf.Deg2Rad;
                
                UIVertex vertex = UIVertex.simpleVert;
                vertex.color = col;
                vertex.position = new Vector3(
                    cornerCenter.x + Mathf.Cos(rad) * radius,
                    cornerCenter.y + Mathf.Sin(rad) * radius,
                    0
                );
                vh.AddVert(vertex);
                vertexCount++;
            }
        }
        
        // Create triangles (fan from center)
        int totalPoints = 4 * (segments + 1);
        for (int i = 1; i < totalPoints; i++)
        {
            vh.AddTriangle(0, i, i + 1);
        }
        // Close the loop
        vh.AddTriangle(0, totalPoints, 1);
    }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        cornerRadius = Mathf.Max(0, cornerRadius);
        cornerSegments = Mathf.Clamp(cornerSegments, 2, 20);
        SetVerticesDirty();
    }
#endif
}
