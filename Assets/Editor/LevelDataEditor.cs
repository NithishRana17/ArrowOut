using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom Editor for LevelDataSO with visual grid for drawing arrows
/// </summary>
[CustomEditor(typeof(LevelDataSO))]
public class LevelDataEditor : Editor
{
    private LevelDataSO levelData;
    private int selectedArrowIndex = -1;
    private bool isDrawingMode = false;
    private Vector2 scrollPosition;
    
    // Grid visual settings
    private const float CELL_SIZE = 30f;
    private const float GRID_PADDING = 10f;
    
    private void OnEnable()
    {
        levelData = (LevelDataSO)target;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Level Info
        EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelNumber"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gridSize"));
        
        EditorGUILayout.Space(10);
        
        // Arrow Management
        EditorGUILayout.LabelField("Arrow Management", EditorStyles.boldLabel);
        
        // Arrow list with buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Arrow", GUILayout.Height(25)))
        {
            AddNewArrow();
        }
        if (selectedArrowIndex >= 0 && selectedArrowIndex < levelData.arrows.Count)
        {
            if (GUILayout.Button("- Remove Selected", GUILayout.Height(25)))
            {
                RemoveSelectedArrow();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Arrow selector - colored buttons
        if (levelData.arrows.Count > 0)
        {
            EditorGUILayout.LabelField("Select Arrow to Edit:", EditorStyles.miniLabel);
            
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < levelData.arrows.Count; i++)
            {
                var arrow = levelData.arrows[i];
                
                // Store original colors
                Color oldBg = GUI.backgroundColor;
                Color oldContent = GUI.contentColor;
                
                // Set button color to arrow's editor color
                GUI.backgroundColor = arrow.editorColor;
                
                // Highlight selected arrow
                string label = $"{i + 1}";
                GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
                btnStyle.fontStyle = (i == selectedArrowIndex) ? FontStyle.Bold : FontStyle.Normal;
                
                if (i == selectedArrowIndex)
                {
                    GUI.backgroundColor = Color.Lerp(arrow.editorColor, Color.white, 0.4f);
                    label = $"►{i + 1}◄";
                }
                
                if (GUILayout.Button(label, btnStyle, GUILayout.Width(45), GUILayout.Height(30)))
                {
                    selectedArrowIndex = i;
                    isDrawingMode = false;
                }
                
                // Restore colors
                GUI.backgroundColor = oldBg;
                GUI.contentColor = oldContent;
                
                // Wrap to new line if too many arrows
                if ((i + 1) % 8 == 0 && i < levelData.arrows.Count - 1)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Selected arrow properties
            if (selectedArrowIndex >= 0 && selectedArrowIndex < levelData.arrows.Count)
            {
                var arrow = levelData.arrows[selectedArrowIndex];
                
                EditorGUILayout.BeginVertical("box");
                
                // Direction
                arrow.headDirection = (Direction)EditorGUILayout.EnumPopup("Head Direction", arrow.headDirection);
                
                // Color
                arrow.editorColor = EditorGUILayout.ColorField("Editor Color", arrow.editorColor);
                
                // Drawing mode toggle
                EditorGUILayout.BeginHorizontal();
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = isDrawingMode ? Color.green : Color.white;
                if (GUILayout.Button(isDrawingMode ? "✓ Drawing Mode ON" : "Start Drawing", GUILayout.Height(25)))
                {
                    isDrawingMode = !isDrawingMode;
                }
                GUI.backgroundColor = oldColor;
                
                if (GUILayout.Button("Clear Path", GUILayout.Height(25)))
                {
                    arrow.waypoints.Clear();
                    EditorUtility.SetDirty(levelData);
                }
                EditorGUILayout.EndHorizontal();
                
                // Waypoint count
                EditorGUILayout.LabelField($"Waypoints: {arrow.waypoints.Count}");
                
                EditorGUILayout.EndVertical();
            }
        }
        
        EditorGUILayout.Space(10);
        
        // Visual Grid
        EditorGUILayout.LabelField("Visual Grid Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            isDrawingMode 
                ? "DRAWING MODE: Click cells to add waypoints to selected arrow" 
                : "Select an arrow and enable Drawing Mode to edit", 
            MessageType.Info);
        
        DrawVisualGrid();
        
        EditorGUILayout.Space(10);
        
        // Validation and status
        int validCount = levelData.ValidArrowCount;
        int totalCount = levelData.ArrowCount;
        
        if (totalCount > 0)
        {
            // Show status
            string statusMsg = $"Valid arrows: {validCount}/{totalCount}";
            if (validCount == totalCount)
            {
                EditorGUILayout.HelpBox(statusMsg + " ✓ Ready to play!", MessageType.Info);
            }
            else
            {
                // Find incomplete arrows
                List<int> incompleteArrows = new List<int>();
                for (int i = 0; i < levelData.arrows.Count; i++)
                {
                    if (!levelData.arrows[i].IsValid)
                    {
                        incompleteArrows.Add(i + 1);
                    }
                }
                string arrowList = string.Join(", ", incompleteArrows);
                EditorGUILayout.HelpBox(
                    $"{statusMsg}\nArrow(s) {arrowList} need at least 2 waypoints!", 
                    MessageType.Warning);
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }
    
    private void DrawVisualGrid()
    {
        if (levelData.gridSize.x <= 0 || levelData.gridSize.y <= 0) return;
        
        float gridWidth = levelData.gridSize.x * CELL_SIZE + GRID_PADDING * 2;
        float gridHeight = levelData.gridSize.y * CELL_SIZE + GRID_PADDING * 2;
        
        // Create scrollable area if grid is large
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, 
            GUILayout.Height(Mathf.Min(gridHeight + 20, 400)));
        
        Rect gridRect = GUILayoutUtility.GetRect(gridWidth, gridHeight);
        
        // Draw background
        EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));
        
        // Draw cells
        for (int y = 0; y < levelData.gridSize.y; y++)
        {
            for (int x = 0; x < levelData.gridSize.x; x++)
            {
                // Flip Y for visual (0,0 at bottom-left)
                int visualY = levelData.gridSize.y - 1 - y;
                
                Rect cellRect = new Rect(
                    gridRect.x + GRID_PADDING + x * CELL_SIZE,
                    gridRect.y + GRID_PADDING + visualY * CELL_SIZE,
                    CELL_SIZE - 2,
                    CELL_SIZE - 2
                );
                
                Vector2Int cellPos = new Vector2Int(x, y);
                int arrowIndex = levelData.GetArrowAtCell(cellPos);
                
                // Determine cell color
                Color cellColor = new Color(0.4f, 0.4f, 0.4f); // Empty
                
                if (arrowIndex >= 0)
                {
                    cellColor = levelData.arrows[arrowIndex].editorColor;
                    
                    // Highlight if this is the selected arrow
                    if (arrowIndex == selectedArrowIndex)
                    {
                        cellColor = Color.Lerp(cellColor, Color.white, 0.3f);
                    }
                }
                
                // Draw cell
                EditorGUI.DrawRect(cellRect, cellColor);
                
                // Draw cell border
                Handles.color = Color.black;
                Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.xMax, cellRect.y));
                Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.y), new Vector3(cellRect.xMax, cellRect.yMax));
                Handles.DrawLine(new Vector3(cellRect.xMax, cellRect.yMax), new Vector3(cellRect.x, cellRect.yMax));
                Handles.DrawLine(new Vector3(cellRect.x, cellRect.yMax), new Vector3(cellRect.x, cellRect.y));
                
                // Draw waypoint number for selected arrow
                if (arrowIndex == selectedArrowIndex && arrowIndex >= 0)
                {
                    var arrow = levelData.arrows[arrowIndex];
                    int waypointIndex = arrow.waypoints.IndexOf(cellPos);
                    if (waypointIndex >= 0)
                    {
                        string label = waypointIndex == 0 ? "T" : 
                                      waypointIndex == arrow.waypoints.Count - 1 ? "H" : 
                                      (waypointIndex + 1).ToString();
                        
                        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                        style.alignment = TextAnchor.MiddleCenter;
                        style.normal.textColor = Color.white;
                        EditorGUI.LabelField(cellRect, label, style);
                    }
                }
                
                // Handle click
                if (isDrawingMode && Event.current.type == EventType.MouseDown && 
                    Event.current.button == 0 && cellRect.Contains(Event.current.mousePosition))
                {
                    HandleCellClick(cellPos);
                    Event.current.Use();
                }
            }
        }
        
        // Draw arrow connections
        DrawArrowConnections(gridRect);
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawArrowConnections(Rect gridRect)
    {
        foreach (var arrow in levelData.arrows)
        {
            if (arrow.waypoints.Count < 2) continue;
            
            // Use BLACK color for all arrow lines for visibility
            Handles.color = Color.black;
            
            for (int i = 0; i < arrow.waypoints.Count - 1; i++)
            {
                Vector2Int from = arrow.waypoints[i];
                Vector2Int to = arrow.waypoints[i + 1];
                
                int fromVisualY = levelData.gridSize.y - 1 - from.y;
                int toVisualY = levelData.gridSize.y - 1 - to.y;
                
                Vector3 fromPos = new Vector3(
                    gridRect.x + GRID_PADDING + from.x * CELL_SIZE + CELL_SIZE / 2,
                    gridRect.y + GRID_PADDING + fromVisualY * CELL_SIZE + CELL_SIZE / 2,
                    0
                );
                
                Vector3 toPos = new Vector3(
                    gridRect.x + GRID_PADDING + to.x * CELL_SIZE + CELL_SIZE / 2,
                    gridRect.y + GRID_PADDING + toVisualY * CELL_SIZE + CELL_SIZE / 2,
                    0
                );
                
                // Draw thicker line (draw multiple parallel lines)
                Handles.DrawLine(fromPos, toPos);
                Handles.DrawLine(fromPos + Vector3.right, toPos + Vector3.right);
                Handles.DrawLine(fromPos + Vector3.up, toPos + Vector3.up);
            }
            
            // Draw BIGGER arrow head direction indicator
            if (arrow.waypoints.Count > 0)
            {
                Vector2Int head = arrow.HeadPosition;
                int headVisualY = levelData.gridSize.y - 1 - head.y;
                
                Vector3 headPos = new Vector3(
                    gridRect.x + GRID_PADDING + head.x * CELL_SIZE + CELL_SIZE / 2,
                    gridRect.y + GRID_PADDING + headVisualY * CELL_SIZE + CELL_SIZE / 2,
                    0
                );
                
                // Direction offset - BIGGER (12 instead of 8)
                Vector3 dirOffset = Vector3.zero;
                Vector3 perpOffset = Vector3.zero;
                switch (arrow.headDirection)
                {
                    case Direction.Up: 
                        dirOffset = new Vector3(0, -14, 0); 
                        perpOffset = new Vector3(6, 0, 0);
                        break;
                    case Direction.Down: 
                        dirOffset = new Vector3(0, 14, 0); 
                        perpOffset = new Vector3(6, 0, 0);
                        break;
                    case Direction.Left: 
                        dirOffset = new Vector3(-14, 0, 0); 
                        perpOffset = new Vector3(0, 6, 0);
                        break;
                    case Direction.Right: 
                        dirOffset = new Vector3(14, 0, 0); 
                        perpOffset = new Vector3(0, 6, 0);
                        break;
                }
                
                // Draw arrow head triangle (thicker)
                Vector3 tipPos = headPos + dirOffset;
                Handles.color = Color.black;
                Handles.DrawLine(headPos, tipPos);
                Handles.DrawLine(headPos + Vector3.right * 0.5f, tipPos);
                Handles.DrawLine(tipPos, headPos + perpOffset * 0.5f);
                Handles.DrawLine(tipPos, headPos - perpOffset * 0.5f);
            }
            
            // Draw tail indicator (small square)
            if (arrow.waypoints.Count > 0)
            {
                Vector2Int tail = arrow.TailPosition;
                int tailVisualY = levelData.gridSize.y - 1 - tail.y;
                
                Vector3 tailPos = new Vector3(
                    gridRect.x + GRID_PADDING + tail.x * CELL_SIZE + CELL_SIZE / 2,
                    gridRect.y + GRID_PADDING + tailVisualY * CELL_SIZE + CELL_SIZE / 2,
                    0
                );
                
                // Draw small filled circle for tail
                Handles.color = Color.black;
                Handles.DrawSolidDisc(tailPos, Vector3.forward, 4f);
            }
        }
    }
    
    private void HandleCellClick(Vector2Int cellPos)
    {
        if (selectedArrowIndex < 0 || selectedArrowIndex >= levelData.arrows.Count) return;
        
        var arrow = levelData.arrows[selectedArrowIndex];
        
        // Check if cell already in this arrow's path
        int existingIndex = arrow.waypoints.IndexOf(cellPos);
        
        if (existingIndex >= 0)
        {
            // Remove this and all subsequent waypoints
            arrow.waypoints.RemoveRange(existingIndex, arrow.waypoints.Count - existingIndex);
        }
        else
        {
            // Check if cell is adjacent to last waypoint (or first waypoint if empty)
            bool canAdd = true;
            if (arrow.waypoints.Count > 0)
            {
                Vector2Int lastWp = arrow.waypoints[arrow.waypoints.Count - 1];
                int dist = Mathf.Abs(cellPos.x - lastWp.x) + Mathf.Abs(cellPos.y - lastWp.y);
                canAdd = (dist == 1); // Must be adjacent (Manhattan distance = 1)
            }
            
            // Check if another arrow occupies this cell
            int otherArrow = levelData.GetArrowAtCell(cellPos);
            if (otherArrow >= 0 && otherArrow != selectedArrowIndex)
            {
                canAdd = false;
                Debug.LogWarning($"Cell ({cellPos.x}, {cellPos.y}) is already used by Arrow {otherArrow + 1}");
            }
            
            if (canAdd)
            {
                arrow.waypoints.Add(cellPos);
            }
            else if (arrow.waypoints.Count > 0)
            {
                Debug.LogWarning("Can only add adjacent cells! Click on a cell next to the last waypoint.");
            }
        }
        
        EditorUtility.SetDirty(levelData);
        Repaint();
    }
    
    private void AddNewArrow()
    {
        var newArrow = new ArrowPathData
        {
            waypoints = new List<Vector2Int>(),
            headDirection = Direction.Right,
            editorColor = GetNextArrowColor()
        };
        
        levelData.arrows.Add(newArrow);
        selectedArrowIndex = levelData.arrows.Count - 1;
        isDrawingMode = true;
        
        EditorUtility.SetDirty(levelData);
    }
    
    private void RemoveSelectedArrow()
    {
        if (selectedArrowIndex >= 0 && selectedArrowIndex < levelData.arrows.Count)
        {
            levelData.arrows.RemoveAt(selectedArrowIndex);
            selectedArrowIndex = Mathf.Min(selectedArrowIndex, levelData.arrows.Count - 1);
            isDrawingMode = false;
            
            EditorUtility.SetDirty(levelData);
        }
    }
    
    private Color GetNextArrowColor()
    {
        Color[] colors = new Color[]
        {
            new Color(0.2f, 0.6f, 1f),    // Blue
            new Color(1f, 0.4f, 0.4f),    // Red
            new Color(0.4f, 0.9f, 0.4f),  // Green
            new Color(1f, 0.8f, 0.2f),    // Yellow
            new Color(0.8f, 0.4f, 1f),    // Purple
            new Color(1f, 0.6f, 0.2f),    // Orange
            new Color(0.2f, 0.9f, 0.9f),  // Cyan
            new Color(1f, 0.5f, 0.8f),    // Pink
        };
        
        return colors[levelData.arrows.Count % colors.Length];
    }
}
