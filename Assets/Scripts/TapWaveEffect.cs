using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Creates a ripple/wave animation effect when tapping the screen
/// </summary>
public class TapWaveEffect : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Wave Settings")]
    [SerializeField] private Color waveColor = new Color(0.3f, 0.7f, 1f, 0.5f);
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int segments = 32;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        CheckForTap();
    }
    
    private void CheckForTap()
    {
        bool tapped = false;
        Vector2 screenPos = Vector2.zero;
        
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            tapped = true;
            screenPos = mouse.position.ReadValue();
        }
        else
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                tapped = true;
                screenPos = touch.primaryTouch.position.ReadValue();
            }
        }
        
        if (tapped)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            CreateWave(worldPos);
        }
    }
    
    private void CreateWave(Vector3 position)
    {
        GameObject waveObj = new GameObject("TapWave");
        waveObj.transform.position = position;
        
        LineRenderer lr = waveObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = waveColor;
        lr.endColor = waveColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.sortingOrder = 100;
        lr.positionCount = segments + 1;
        
        // Start animation
        StartCoroutine(AnimateWave(waveObj, lr, position));
    }
    
    private System.Collections.IEnumerator AnimateWave(GameObject waveObj, LineRenderer lr, Vector3 center)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Expand radius
            float currentRadius = Mathf.Lerp(0, maxRadius, t);
            
            // Fade out
            float alpha = Mathf.Lerp(0.6f, 0f, t);
            Color currentColor = new Color(waveColor.r, waveColor.g, waveColor.b, alpha);
            lr.startColor = currentColor;
            lr.endColor = currentColor;
            
            // Thin out the line
            float currentWidth = Mathf.Lerp(lineWidth, lineWidth * 0.2f, t);
            lr.startWidth = currentWidth;
            lr.endWidth = currentWidth;
            
            // Draw circle
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * currentRadius;
                float y = Mathf.Sin(angle) * currentRadius;
                lr.SetPosition(i, center + new Vector3(x, y, 0));
            }
            
            yield return null;
        }
        
        Destroy(waveObj);
    }
}
