using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Camera controller for panning and zooming.
/// Uses two-finger gestures to avoid conflict with arrow taps.
/// Attach to the Main Camera.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 0.5f;
    [SerializeField] private float panLimitX = 3f;
    [SerializeField] private float panLimitY = 5f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 8f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = true;
    
    private Camera cam;
    private Vector3 startCameraPos;
    private float startZoom;
    
    // Two-finger gesture state
    private Vector2 lastMidpoint;
    private float lastPinchDistance = 0f;
    private bool isTwoFingerGesture = false;
    
    // Target values for smooth movement
    private Vector3 targetPosition;
    private float targetZoom;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController: No Camera component found!");
            return;
        }
        
        startCameraPos = transform.position;
        startZoom = cam.orthographicSize;
        targetPosition = startCameraPos;
        targetZoom = startZoom;
        
        if (enableDebugLog) Debug.Log("CameraController initialized");
    }
    
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        if (enableDebugLog) Debug.Log("EnhancedTouchSupport enabled");
    }
    
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }
    
    private void Update()
    {
        if (cam == null) return;
        
        HandleInput();
        
        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * 10f);
    }
    
    private void HandleInput()
    {
        // Get active touches
        var touches = Touch.activeTouches;
        int touchCount = touches.Count;
        
        // TWO FINGER: Pan + Zoom (to avoid conflict with arrow taps)
        if (touchCount >= 2)
        {
            Vector2 pos0 = touches[0].screenPosition;
            Vector2 pos1 = touches[1].screenPosition;
            
            Vector2 midpoint = (pos0 + pos1) / 2f;
            float distance = Vector2.Distance(pos0, pos1);
            
            if (!isTwoFingerGesture)
            {
                // Start gesture
                isTwoFingerGesture = true;
                lastMidpoint = midpoint;
                lastPinchDistance = distance;
                if (enableDebugLog) Debug.Log("Two-finger gesture started");
            }
            else
            {
                // Pan: Move camera based on midpoint movement
                Vector2 midpointDelta = midpoint - lastMidpoint;
                float worldDeltaX = -midpointDelta.x * panSpeed * cam.orthographicSize / Screen.height;
                float worldDeltaY = -midpointDelta.y * panSpeed * cam.orthographicSize / Screen.height;
                
                targetPosition += new Vector3(worldDeltaX, worldDeltaY, 0);
                targetPosition.x = Mathf.Clamp(targetPosition.x, startCameraPos.x - panLimitX, startCameraPos.x + panLimitX);
                targetPosition.y = Mathf.Clamp(targetPosition.y, startCameraPos.y - panLimitY, startCameraPos.y + panLimitY);
                
                // Zoom: Pinch
                float pinchDelta = lastPinchDistance - distance;
                targetZoom += pinchDelta * zoomSpeed;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                
                lastMidpoint = midpoint;
                lastPinchDistance = distance;
            }
        }
        else
        {
            isTwoFingerGesture = false;
            lastPinchDistance = 0f;
        }
        
        // MOUSE: For editor testing
        var mouse = Mouse.current;
        if (mouse != null && touchCount == 0)
        {
            // Scroll wheel zoom
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetZoom -= scroll * 0.5f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                if (enableDebugLog) Debug.Log($"Mouse scroll: {scroll}, zoom: {targetZoom}");
            }
            
            // Right mouse drag to pan
            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                if (delta.sqrMagnitude > 0.1f)
                {
                    float worldDeltaX = -delta.x * panSpeed * cam.orthographicSize / Screen.height;
                    float worldDeltaY = -delta.y * panSpeed * cam.orthographicSize / Screen.height;
                    
                    targetPosition += new Vector3(worldDeltaX, worldDeltaY, 0);
                    targetPosition.x = Mathf.Clamp(targetPosition.x, startCameraPos.x - panLimitX, startCameraPos.x + panLimitX);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, startCameraPos.y - panLimitY, startCameraPos.y + panLimitY);
                    
                    if (enableDebugLog) Debug.Log($"Mouse pan: {delta}");
                }
            }
        }
    }
    
    public void ResetCamera()
    {
        targetPosition = startCameraPos;
        targetZoom = startZoom;
        if (enableDebugLog) Debug.Log("Camera reset");
    }
    
    public void SetNewCenter(Vector3 newCenter)
    {
        startCameraPos = new Vector3(newCenter.x, newCenter.y, transform.position.z);
        targetPosition = startCameraPos;
    }
}
