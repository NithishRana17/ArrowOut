using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a segment type in an arrow
/// </summary>
public enum SegmentType
{
    Head,       // Arrow tip (start sprite)
    Tail,       // Arrow tail (end sprite)
    Straight,   // Middle straight piece
    Bend        // Corner/turn piece
}

/// <summary>
/// Direction for movement and sprite selection
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

/// <summary>
/// Represents a single segment of an arrow
/// </summary>
[System.Serializable]
public class ArrowSegment
{
    public Vector2Int gridPosition;
    public SegmentType type;
    public Direction inDirection;   // Direction arrow comes FROM
    public Direction outDirection;  // Direction arrow goes TO
    
    public ArrowSegment(Vector2Int pos, SegmentType type, Direction inDir, Direction outDir)
    {
        this.gridPosition = pos;
        this.type = type;
        this.inDirection = inDir;
        this.outDirection = outDir;
    }
}

/// <summary>
/// Defines a complete arrow path
/// </summary>
[System.Serializable]
public class ArrowPath
{
    public List<Vector2Int> path; // Grid positions from TAIL to HEAD
    public Direction headDirection; // Direction the head points (exit direction)
    
    public ArrowPath(List<Vector2Int> path, Direction headDir)
    {
        this.path = path;
        this.headDirection = headDir;
    }
}

/// <summary>
/// Level data for maze
/// </summary>
[System.Serializable]
public class MazeLevelData
{
    public int levelNumber;
    public string levelName;
    public List<ArrowPath> arrows;
    
    public MazeLevelData(int num, string name)
    {
        levelNumber = num;
        levelName = name;
        arrows = new List<ArrowPath>();
    }
    
    public void AddArrow(List<Vector2Int> path, Direction headDir)
    {
        arrows.Add(new ArrowPath(path, headDir));
    }
}
