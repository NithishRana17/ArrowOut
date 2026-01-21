using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Arrow using LineRenderer for smooth continuous body movement.
/// Arrow head rendered with LineRenderer for unified color system.
/// Optimized for butter-smooth 120 FPS gameplay.
/// </summary>
public class Arrow : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    // Path data
    private List<Vector3> waypoints = new List<Vector3>();
    private List<float> waypointDistances = new List<float>();
    private float totalPathLength = 0f;
    
    // Visual components
    private LineRenderer bodyLineRenderer;
    private List<GameObject> dots = new List<GameObject>();
    private List<bool> dotRevealed = new List<bool>();
    
    // Movement state - use doubles for precision
    private double headDistance = 0;
    private double tailDistance = 0;
    private float moveSpeed = 8.64f;
    private float exitDistance = 0f;
    
    private bool isMoving = false;
    private bool isCleared = false;
    private bool isReversing = false;      // True when bumped and returning
    private bool hasHitBlocker = false;    // True after hitting blocker
    private double savedHeadDistance = 0;   // Position before bump
    private double savedTailDistance = 0;
    
    private Direction headDirection;
    private MazeManager mazeManager;
    private Camera mainCamera;
    
    // Line settings
    private float lineWidth = 0.09f;
    private Color lineColor = Color.black;
    private float arrowHeadSize = 0.5f; // Size of arrow head (much bigger)
    
    // Cached arrays for LineRenderer (avoid allocations)
    private Vector3[] linePointsCache = new Vector3[20];
    private int cachedPointCount = 0;
    
    // Cached colliders
    private Collider2D[] cachedColliders;
    
    // Direction guide line
    private LineRenderer directionGuide;
    private bool isGuideVisible = false;
    
    public bool IsMoving => isMoving;
    public bool IsCleared => isCleared;
    
    public void Initialize(List<Vector2Int> path, Direction headDir, MazeManager manager)
    {
        headDirection = headDir;
        mazeManager = manager;
        mainCamera = Camera.main;
        
        // Pre-allocate waypoints
        waypoints.Capacity = path.Count;
        waypointDistances.Capacity = path.Count;
        
        foreach (var gridPos in path)
        {
            waypoints.Add(manager.GridToWorld(gridPos));
        }
        
        waypointDistances.Add(0f);
        for (int i = 1; i < waypoints.Count; i++)
        {
            float dist = Vector3.Distance(waypoints[i - 1], waypoints[i]);
            waypointDistances.Add(waypointDistances[i - 1] + dist);
        }
        totalPathLength = waypointDistances[waypointDistances.Count - 1];
        
        exitDistance = 3f;
        
        // Pre-allocate line points cache (max possible points)
        linePointsCache = new Vector3[waypoints.Count + 2];
        
        SetupBodyLineRenderer();
        SetupHeadMesh();
        
        headDistance = totalPathLength;
        tailDistance = 0;
        
        UpdateLinePoints();
        UpdateArrowHead();
        SetupCollider();
        
        // Cache colliders for click detection
        cachedColliders = GetComponentsInChildren<Collider2D>();
    }
    
    private void SetupBodyLineRenderer()
    {
        bodyLineRenderer = gameObject.AddComponent<LineRenderer>();
        bodyLineRenderer.startWidth = lineWidth;
        bodyLineRenderer.endWidth = lineWidth;
        
        // Use shared material for better batching
        bodyLineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        bodyLineRenderer.startColor = lineColor;
        bodyLineRenderer.endColor = lineColor;
        bodyLineRenderer.sortingOrder = 0;
        
        // Smooth corners and caps
        bodyLineRenderer.numCapVertices = 4;
        bodyLineRenderer.numCornerVertices = 4;
        bodyLineRenderer.useWorldSpace = true;
        
        // Disable dynamic shadows for performance
        bodyLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        bodyLineRenderer.receiveShadows = false;
    }
    
    // Arrow head mesh components
    private MeshFilter headMeshFilter;
    private MeshRenderer headMeshRenderer;
    private Mesh headMesh;
    private Transform headTransform;
    private Material headMaterial;
    
    private void SetupHeadMesh()
    {
        // Create a child GameObject for the arrow head
        GameObject headObj = new GameObject("ArrowHead");
        headObj.transform.SetParent(transform);
        headTransform = headObj.transform;
        
        // Add mesh components
        headMeshFilter = headObj.AddComponent<MeshFilter>();
        headMeshRenderer = headObj.AddComponent<MeshRenderer>();
        
        // Create material
        headMaterial = new Material(Shader.Find("Sprites/Default"));
        headMaterial.color = lineColor;
        headMeshRenderer.material = headMaterial;
        headMeshRenderer.sortingOrder = 1;
        
        // Create the arrow chevron mesh
        headMesh = new Mesh();
        headMesh.name = "ArrowHeadMesh";
        
        // Arrow with 4 vertices: Tip, Left wing, Center notch, Right wing
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0, 0.6f, 0);       // Tip (sharp point)
        vertices[1] = new Vector3(-0.65f, -0.3f, 0); // Left wing outer
        vertices[2] = new Vector3(0f, -0.3f, 0);     // Center notch point
        vertices[3] = new Vector3(0.65f, -0.3f, 0);  // Right wing outer
        
        // Two triangles connecting edges to center
        // Triangle 1: Tip -> Center -> Left wing (counter-clockwise)
        // Triangle 2: Tip -> Right wing -> Center (counter-clockwise)
        int[] triangles = new int[] { 
            0, 2, 1,  // Tip -> Center -> Left wing
            0, 3, 2   // Tip -> Right wing -> Center
        };
        
        // Colors (4 vertices)
        Color[] colors = new Color[] { lineColor, lineColor, lineColor, lineColor };
        
        headMesh.vertices = vertices;
        headMesh.triangles = triangles;
        headMesh.colors = colors;
        headMesh.RecalculateNormals();
        
        headMeshFilter.mesh = headMesh;
    }
    
    private void SetupCollider()
    {
        float colliderSize = Mathf.Max(lineWidth * 4, 0.15f);
        
        foreach (var wp in waypoints)
        {
            GameObject colliderObj = new GameObject("Collider");
            colliderObj.transform.SetParent(transform);
            colliderObj.transform.position = wp;
            BoxCollider2D col = colliderObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(colliderSize, colliderSize);
        }
        
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            Vector3 midpoint = (start + end) * 0.5f;
            float segmentLength = Vector3.Distance(start, end);
            
            GameObject segCollider = new GameObject("SegmentCollider");
            Transform segTransform = segCollider.transform;
            segTransform.SetParent(transform);
            segTransform.position = midpoint;
            
            Vector3 direction = (end - start).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            segTransform.rotation = Quaternion.Euler(0, 0, angle);
            
            BoxCollider2D segCol = segCollider.AddComponent<BoxCollider2D>();
            segCol.size = new Vector2(segmentLength, colliderSize);
        }
    }
    
    public void AddSegment(GameObject segment, bool isDot = false)
    {
        if (isDot)
        {
            dots.Add(segment);
            dotRevealed.Add(false);
            return;
        }
        if (segment != null) Destroy(segment);
    }
    
    private void Update()
    {
        // Don't process input if game is not active (win/loss screen showing)
        if (mazeManager != null && !mazeManager.IsGameActive) return;
        
        if (!isMoving && !isCleared) CheckClick();
        if (isMoving) AnimateMovement();
    }
    
    private void CheckClick()
    {
        // Double check game is active
        if (mazeManager != null && !mazeManager.IsGameActive) return;
        
        Vector2 clickPos = Vector2.zero;
        bool clicked = false;
        
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            clicked = true;
            clickPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        }
        else
        {
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                clicked = true;
                clickPos = mainCamera.ScreenToWorldPoint(touch.primaryTouch.position.ReadValue());
            }
        }
        
        if (clicked)
        {
            // Use cached colliders
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i].OverlapPoint(clickPos))
                {
                    mazeManager?.OnArrowClicked(this);
                    break;
                }
            }
        }
    }
    
    public void StartMoving()
    {
        if (isMoving || isCleared) return;
        
        isMoving = true;
        headDistance = totalPathLength;
        tailDistance = 0;
        
        // Disable colliders using cached array
        for (int i = 0; i < cachedColliders.Length; i++)
        {
            cachedColliders[i].enabled = false;
        }
    }
    
    private void AnimateMovement()
    {
        double deltaMove = moveSpeed * Time.deltaTime;
        
        if (isReversing)
        {
            // Reversing back to original position
            headDistance -= deltaMove;
            
            // Also move tail back if it had started moving
            if (savedTailDistance > 0)
            {
                tailDistance -= deltaMove;
                if (tailDistance < 0) tailDistance = 0;
            }
            
            // Check if fully reversed to original position
            if (headDistance <= totalPathLength)
            {
                headDistance = totalPathLength;
                tailDistance = 0;
                isMoving = false;
                isReversing = false;
                hasHitBlocker = false;
                
                // Re-enable colliders
                for (int i = 0; i < cachedColliders.Length; i++)
                {
                    if (cachedColliders[i] != null)
                        cachedColliders[i].enabled = true;
                }
            }
        }
        else
        {
            // Moving forward
            headDistance += deltaMove;
            
            if (headDistance > totalPathLength)
            {
                tailDistance += deltaMove;
            }
            
            // Check for collision with other arrows DURING movement
            if (!hasHitBlocker && headDistance > totalPathLength)
            {
                Vector3 headPos = GetPositionAtDistance(headDistance);
                Vector3 dir = GetDirectionVector(headDirection);
                
                // Raycast ahead to detect blockers
                RaycastHit2D hit = Physics2D.Raycast(headPos, dir, 0.1f);
                if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
                {
                    // Hit a blocker! Start reverse
                    hasHitBlocker = true;
                    isReversing = true;
                    savedHeadDistance = headDistance;
                    savedTailDistance = tailDistance;
                    
                    // Flash red and notify manager
                    StartCoroutine(FlashRedDuringReverse());
                    mazeManager?.OnArrowBlocked(this);
                }
            }
        }
        
        RevealDotsAlongPath();
        UpdateLinePoints();
        UpdateArrowHead();
        
        if (!isReversing && tailDistance >= totalPathLength + 0.5)
        {
            CheckIfFullyExited();
        }
    }
    
    private void RevealDotsAlongPath()
    {
        int count = Mathf.Min(dots.Count, waypointDistances.Count);
        for (int i = 0; i < count; i++)
        {
            if (!dotRevealed[i] && tailDistance >= waypointDistances[i])
            {
                dots[i].SetActive(true);
                dotRevealed[i] = true;
            }
        }
    }
    
    private void UpdateLinePoints()
    {
        if (bodyLineRenderer == null) return;
        
        cachedPointCount = 0;
        
        // Add tail position
        Vector3 tailPos = GetPositionAtDistance(tailDistance);
        linePointsCache[cachedPointCount++] = tailPos;
        
        // Add intermediate waypoints
        float tailDist = (float)tailDistance;
        float headDist = (float)headDistance;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            float wpDist = waypointDistances[i];
            if (wpDist <= tailDist + 0.001f) continue;
            if (wpDist >= headDist - 0.001f) break;
            linePointsCache[cachedPointCount++] = waypoints[i];
        }
        
        // Add head position
        Vector3 headPos = GetPositionAtDistance(headDistance);
        if (cachedPointCount == 0 || Vector3.SqrMagnitude(linePointsCache[cachedPointCount - 1] - headPos) > 0.000001f)
        {
            linePointsCache[cachedPointCount++] = headPos;
        }
        
        // Set line positions
        if (cachedPointCount >= 2)
        {
            bodyLineRenderer.positionCount = cachedPointCount;
            for (int i = 0; i < cachedPointCount; i++)
            {
                bodyLineRenderer.SetPosition(i, linePointsCache[i]);
            }
        }
        else if (cachedPointCount == 1)
        {
            bodyLineRenderer.positionCount = 2;
            bodyLineRenderer.SetPosition(0, linePointsCache[0]);
            bodyLineRenderer.SetPosition(1, linePointsCache[0] + Vector3.right * 0.01f);
        }
        else
        {
            bodyLineRenderer.positionCount = 0;
        }
    }
    
    private void UpdateArrowHead()
    {
        if (headTransform == null) return;
        
        Vector3 headPos = GetPositionAtDistance(headDistance);
        Vector3 dir = GetDirectionVector(headDirection);
        
        // Position the arrow head at the current head position + slight forward offset
        float forwardOffset = arrowHeadSize * 0.02f; // Move head forward
        headTransform.position = headPos + dir * forwardOffset;
        
        // Rotate to face the movement direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f; // -90 because triangle points up by default
        headTransform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Scale the arrow head
        headTransform.localScale = Vector3.one * arrowHeadSize;
    }
    
    private Vector3 GetPositionAtDistance(double distance)
    {
        if (distance <= 0) return waypoints[0];
        
        if (distance >= totalPathLength)
        {
            float extraDist = (float)(distance - totalPathLength);
            Vector3 exitDir = GetDirectionVector(headDirection);
            return waypoints[waypoints.Count - 1] + exitDir * extraDist;
        }
        
        float dist = (float)distance;
        for (int i = 1; i < waypointDistances.Count; i++)
        {
            if (dist <= waypointDistances[i])
            {
                float segmentStart = waypointDistances[i - 1];
                float segmentLength = waypointDistances[i] - segmentStart;
                float t = (dist - segmentStart) / segmentLength;
                return Vector3.Lerp(waypoints[i - 1], waypoints[i], t);
            }
        }
        
        return waypoints[waypoints.Count - 1];
    }
    
    private void CheckIfFullyExited()
    {
        Vector3 tailPos = GetPositionAtDistance(tailDistance);
        Vector3 vp = mainCamera.WorldToViewportPoint(tailPos);
        
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            FinishArrow();
        }
    }
    
    private void FinishArrow()
    {
        isMoving = false;
        isCleared = true;
        
        int count = dots.Count;
        for (int i = 0; i < count; i++)
        {
            if (dots[i] != null) dots[i].SetActive(true);
        }
        
        if (headMeshRenderer != null) headMeshRenderer.enabled = false;
        if (bodyLineRenderer != null) bodyLineRenderer.positionCount = 0;
        
        // Hide direction guide when arrow is cleared
        HideDirectionGuide();
        
        mazeManager?.OnArrowCleared(this);
    }
    
    public bool IsPathClear()
    {
        return GetBlockingInfo(out _, out _);
    }
    
    /// <summary>
    /// Check if path is clear and get blocking info if not
    /// </summary>
    public bool GetBlockingInfo(out Vector3 blockingPoint, out float distanceToBlocker)
    {
        blockingPoint = Vector3.zero;
        distanceToBlocker = 0f;
        
        if (waypoints.Count == 0) return false;
        
        Vector3 headPos = waypoints[waypoints.Count - 1];
        Vector2 dir = GetDirectionVector(headDirection);
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(headPos, dir, 15f);
        
        float closestDist = float.MaxValue;
        bool foundBlocker = false;
        
        int hitCount = hits.Length;
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider == null) continue;
            bool isOurs = hits[i].collider.transform.IsChildOf(transform);
            if (!isOurs)
            {
                foundBlocker = true;
                if (hits[i].distance < closestDist)
                {
                    closestDist = hits[i].distance;
                    blockingPoint = hits[i].point;
                    distanceToBlocker = hits[i].distance;
                }
            }
        }
        
        return !foundBlocker;
    }
    
    private Vector3 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector3.up;
            case Direction.Down: return Vector3.down;
            case Direction.Left: return Vector3.left;
            case Direction.Right: return Vector3.right;
            default: return Vector3.up;
        }
    }
    
    // SetHeadSprite kept for backward compatibility but not used
    public void SetHeadSprite(Sprite sprite) { }
    
    public void SetLineColor(Color color)
    {
        lineColor = color;
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startColor = color;
            bodyLineRenderer.endColor = color;
        }
        if (headMaterial != null)
        {
            headMaterial.color = color;
        }
    }
    
    public void SetLineWidth(float width)
    {
        lineWidth = width;
        arrowHeadSize = width * 1.8f; // Arrow head proportional to line width
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startWidth = width;
            bodyLineRenderer.endWidth = width;
        }
        // Update arrow head scale
        if (headTransform != null)
        {
            headTransform.localScale = Vector3.one * arrowHeadSize;
        }
    }
    
    public void SetSpriteScale(float scale)
    {
        arrowHeadSize = scale * 0.5f; // Use scale to set arrow head size
    }
    
    public void FlashRed()
    {
        // Get blocking info for bump animation
        GetBlockingInfo(out Vector3 blockingPoint, out float distanceToBlocker);
        StartCoroutine(FlashAndBumpRoutine(blockingPoint, distanceToBlocker));
    }
    
    private System.Collections.IEnumerator FlashAndBumpRoutine(Vector3 blockingPoint, float distanceToBlocker)
    {
        Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
        Color origLineColor = lineColor;
        
        // Store original positions
        Vector3 originalPosition = transform.position;
        
        // Apply red color to BOTH body LineRenderer and head mesh
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startColor = flashColor;
            bodyLineRenderer.endColor = flashColor;
        }
        
        // Update head mesh color (both material and vertex colors)
        if (headMaterial != null)
        {
            headMaterial.color = flashColor;
            headMaterial.SetColor("_Color", flashColor);
        }
        if (headMesh != null)
        {
            Color[] flashColors = new Color[] { flashColor, flashColor, flashColor, flashColor };
            headMesh.colors = flashColors;
        }
        
        // Calculate bump direction and distance
        Vector3 bumpDirection = GetDirectionVector(headDirection);
        float bumpDistance = Mathf.Min(distanceToBlocker * 0.6f, 0.3f);
        
        // Only bump if there's a valid blocker nearby
        if (distanceToBlocker > 0.05f && distanceToBlocker < 5f)
        {
            // Bump towards blocker (fast)
            float bumpDuration = 0.12f;
            float elapsed = 0f;
            
            while (elapsed < bumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bumpDuration;
                float easeOut = 1f - (1f - t) * (1f - t);
                transform.position = originalPosition + bumpDirection * bumpDistance * easeOut;
                yield return null;
            }
            
            // Bounce back (slower)
            float returnDuration = 0.25f;
            elapsed = 0f;
            Vector3 bumpedPosition = transform.position;
            
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                float easeElastic = 1f + Mathf.Sin(t * Mathf.PI * 1.5f) * (1f - t) * 0.3f;
                transform.position = Vector3.Lerp(bumpedPosition, originalPosition, t * easeElastic);
                yield return null;
            }
            
            transform.position = originalPosition;
        }
        
        // Keep red color for remaining time (total ~1 second)
        yield return new WaitForSeconds(0.6f);
        
        // Restore original colors
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startColor = origLineColor;
            bodyLineRenderer.endColor = origLineColor;
        }
        if (headMaterial != null)
        {
            headMaterial.color = origLineColor;
            headMaterial.SetColor("_Color", origLineColor);
        }
        if (headMesh != null)
        {
            Color[] origColors = new Color[] { origLineColor, origLineColor, origLineColor, origLineColor, origLineColor };
            headMesh.colors = origColors;
        }
    }
    
    /// <summary>
    /// Flash red while reversing after hitting a blocker
    /// </summary>
    private System.Collections.IEnumerator FlashRedDuringReverse()
    {
        Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
        Color origLineColor = lineColor;
        
        // Apply red color to body and head
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startColor = flashColor;
            bodyLineRenderer.endColor = flashColor;
        }
        if (headMaterial != null)
        {
            headMaterial.color = flashColor;
            headMaterial.SetColor("_Color", flashColor);
        }
        if (headMesh != null)
        {
            Color[] flashColors = new Color[] { flashColor, flashColor, flashColor, flashColor };
            headMesh.colors = flashColors;
        }
        
        // Wait until reverse is complete or timeout
        float timeout = 3f;
        float elapsed = 0f;
        while (isReversing && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Keep red color for 2 more seconds after reversing
        yield return new WaitForSeconds(2f);
        
        // Restore original colors
        if (bodyLineRenderer != null)
        {
            bodyLineRenderer.startColor = origLineColor;
            bodyLineRenderer.endColor = origLineColor;
        }
        if (headMaterial != null)
        {
            headMaterial.color = origLineColor;
            headMaterial.SetColor("_Color", origLineColor);
        }
        if (headMesh != null)
        {
            Color[] origColors = new Color[] { origLineColor, origLineColor, origLineColor, origLineColor };
            headMesh.colors = origColors;
        }
    }
    
    #region Direction Guide
    
    /// <summary>
    /// Shows a gray guide line from arrow head indicating movement direction
    /// </summary>
    public void ShowDirectionGuide()
    {
        if (isCleared || isMoving) return;
        
        // Create guide line if needed
        if (directionGuide == null)
        {
            GameObject guideObj = new GameObject("DirectionGuide");
            guideObj.transform.SetParent(transform);
            directionGuide = guideObj.AddComponent<LineRenderer>();
            
            // Material setup - use Sprites/Default for 2D visibility
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = Color.gray;
            directionGuide.material = mat;
            directionGuide.sortingOrder = 5;
            directionGuide.positionCount = 2;
            directionGuide.useWorldSpace = true; // Important for proper positioning
        }
        
        // Get head position (last waypoint)
        Vector3 headPos = waypoints[waypoints.Count - 1];
        Vector3 dir = GetDirectionVector(headDirection);
        
        // Calculate end point - extend all the way to edge of screen
        float maxDistance = 20f;
        Vector3 endPos = headPos + dir * maxDistance;
        
        // Set line positions - start exactly from head
        directionGuide.SetPosition(0, headPos);
        directionGuide.SetPosition(1, endPos);
        
        // Style: light gray, thin line
        Color guideColor = new Color(0.6f, 0.6f, 0.6f, 0.7f); // Light gray
        directionGuide.startColor = guideColor;
        directionGuide.endColor = new Color(0.7f, 0.7f, 0.7f, 0.3f); // Fade out
        directionGuide.startWidth = 0.04f;
        directionGuide.endWidth = 0.02f;
        
        directionGuide.enabled = true;
        isGuideVisible = true;
        
        if (enableDebugLog) Debug.Log($"Guide shown from {headPos} to {endPos}");
    }
    
    /// <summary>
    /// Hides the direction guide line
    /// </summary>
    public void HideDirectionGuide()
    {
        if (directionGuide != null)
        {
            directionGuide.enabled = false;
        }
        isGuideVisible = false;
    }
    
    /// <summary>
    /// Toggle direction guide visibility
    /// </summary>
    public void SetDirectionGuideVisible(bool visible)
    {
        if (visible)
            ShowDirectionGuide();
        else
            HideDirectionGuide();
    }
    
    #endregion
}

