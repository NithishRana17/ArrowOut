using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Serializable data for a single arrow path
/// </summary>
[System.Serializable]
public class ArrowPathData
{
    [Tooltip("Path waypoints from TAIL to HEAD (order matters!)")]
    public List<Vector2Int> waypoints = new List<Vector2Int>();
    
    [Tooltip("Direction the arrow head points")]
    public Direction headDirection = Direction.Right;
    
    [Tooltip("Color for visual distinction in editor")]
    public Color editorColor = Color.blue;
    
    /// <summary>
    /// Validate the arrow path - needs at least 2 waypoints
    /// </summary>
    public bool IsValid => waypoints != null && waypoints.Count >= 2;
    
    /// <summary>
    /// Get the head position (last waypoint)
    /// </summary>
    public Vector2Int HeadPosition => waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : Vector2Int.zero;
    
    /// <summary>
    /// Get the tail position (first waypoint)
    /// </summary>
    public Vector2Int TailPosition => waypoints.Count > 0 ? waypoints[0] : Vector2Int.zero;
}
