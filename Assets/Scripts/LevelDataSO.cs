using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject for level data - create levels in the Inspector
/// </summary>
[CreateAssetMenu(fileName = "Level_", menuName = "Arrow Out/Level Data", order = 1)]
public class LevelDataSO : ScriptableObject
{
    [Header("Level Info")]
    [Tooltip("Level number for display")]
    public int levelNumber = 1;
    
    [Tooltip("Level name/description")]
    public string levelName = "New Level";
    
    [Header("Grid Settings")]
    [Tooltip("Size of the grid (width x height)")]
    public Vector2Int gridSize = new Vector2Int(8, 8);
    
    [Header("Arrows")]
    [Tooltip("List of arrows in this level")]
    public List<ArrowPathData> arrows = new List<ArrowPathData>();
    
    /// <summary>
    /// Check if level has at least one valid arrow
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (arrows == null || arrows.Count == 0) return false;
            // Return true if at least one arrow is valid
            foreach (var arrow in arrows)
            {
                if (arrow.IsValid) return true;
            }
            return false;
        }
    }
    
    /// <summary>
    /// Get count of valid arrows (2+ waypoints)
    /// </summary>
    public int ValidArrowCount
    {
        get
        {
            int count = 0;
            foreach (var arrow in arrows)
            {
                if (arrow.IsValid) count++;
            }
            return count;
        }
    }
    
    /// <summary>
    /// Get arrow count
    /// </summary>
    public int ArrowCount => arrows?.Count ?? 0;
    
    /// <summary>
    /// Check if a cell is occupied by any arrow
    /// </summary>
    public bool IsCellOccupied(Vector2Int cell)
    {
        foreach (var arrow in arrows)
        {
            if (arrow.waypoints.Contains(cell)) return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get which arrow occupies a cell (returns -1 if none)
    /// </summary>
    public int GetArrowAtCell(Vector2Int cell)
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            if (arrows[i].waypoints.Contains(cell)) return i;
        }
        return -1;
    }
    
    /// <summary>
    /// Convert to runtime MazeLevelData format
    /// </summary>
    public MazeLevelData ToMazeLevelData()
    {
        MazeLevelData levelData = new MazeLevelData(levelNumber, levelName);
        
        foreach (var arrow in arrows)
        {
            if (arrow.IsValid)
            {
                levelData.AddArrow(arrow.waypoints, arrow.headDirection);
            }
        }
        
        return levelData;
    }
}
