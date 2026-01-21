using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the arrow maze - spawns arrows, handles game state and level progression
/// </summary>
public class MazeManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Sprites")]
    [SerializeField] private Sprite underArrowDot;
    
    [Header("Settings")]
    [SerializeField] private float cellSize = 0.35f;
    [SerializeField] private float spriteScale = 0.3f;
    [SerializeField] private Vector2 gridOffset = new Vector2(0, -1f);
    [SerializeField] private float lineWidth = 0.09f;
    [SerializeField] private Color lineColor = Color.black;
    
    [Header("Levels (Assign up to 5 levels)")]
    [SerializeField] private LevelDataSO level1;
    [SerializeField] private LevelDataSO level2;
    [SerializeField] private LevelDataSO level3;
    [SerializeField] private LevelDataSO level4;
    [SerializeField] private LevelDataSO level5;
    
    private LevelDataSO[] allLevels;
    private int currentLevelIndex = 0;
    
    private List<Arrow> activeArrows = new List<Arrow>();
    private List<GameObject> allSpawnedArrows = new List<GameObject>(); // Track all arrow GameObjects for cleanup
    private int currentLives = 3;
    private int maxLives = 3;
    private bool isGameActive = true; // Disable input when false
    private bool isDirectionGuideVisible = false; // Track guide line visibility
    
    public static MazeManager Instance { get; private set; }
    public bool IsGameActive => isGameActive;
    
    // PlayerPrefs keys
    private const string HIGHEST_COMPLETED_KEY = "HighestCompletedLevel";
    private const string SELECTED_LEVEL_KEY = "SelectedLevel";
    
    // Events
    public static event System.Action OnAllArrowsCleared;
    public static event System.Action<int> OnLivesChanged;
    public static event System.Action<int> OnLevelChanged;
    
    public int CurrentLevel => currentLevelIndex + 1;
    public int TotalLevels => GetValidLevelCount();
    public bool HasNextLevel => currentLevelIndex < TotalLevels - 1;
    
    /// <summary>
    /// Get highest completed level (0 = none completed, 1 = level 1 completed, etc.)
    /// </summary>
    public static int HighestCompletedLevel
    {
        get => PlayerPrefs.GetInt(HIGHEST_COMPLETED_KEY, 0);
        private set
        {
            PlayerPrefs.SetInt(HIGHEST_COMPLETED_KEY, value);
            PlayerPrefs.Save();
        }
    }
    
    /// <summary>
    /// Check if a level is unlocked (level 1 always unlocked, others need previous completed)
    /// </summary>
    public static bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return levelNumber <= HighestCompletedLevel + 1;
    }
    
    /// <summary>
    /// Set which level to start when game scene loads
    /// </summary>
    public static void SetSelectedLevel(int levelNumber)
    {
        PlayerPrefs.SetInt(SELECTED_LEVEL_KEY, levelNumber - 1); // Store as 0-indexed
        PlayerPrefs.Save();
    }
    
    private void Awake()
    {
        Instance = this;
        allLevels = new LevelDataSO[] { level1, level2, level3, level4, level5 };
    }
    
    private void Start()
    {
        // Load the selected level (set by MainMenu or default to 0)
        currentLevelIndex = PlayerPrefs.GetInt(SELECTED_LEVEL_KEY, 0);
        
        // Clamp to valid range
        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, TotalLevels - 1);
        
        LoadCurrentLevel();
    }
    
    private int GetValidLevelCount()
    {
        int count = 0;
        foreach (var level in allLevels)
        {
            if (level != null && level.IsValid) count++;
            else break;
        }
        return Mathf.Max(1, count);
    }
    
    private void LoadCurrentLevel()
    {
        // Reset direction guide state for new level
        isDirectionGuideVisible = false;
        
        if (currentLevelIndex >= 0 && currentLevelIndex < allLevels.Length)
        {
            LevelDataSO levelData = allLevels[currentLevelIndex];
            if (levelData != null && levelData.IsValid)
            {
                CenterMaze(levelData.gridSize);
                LoadLevel(levelData.ToMazeLevelData());
                OnLevelChanged?.Invoke(currentLevelIndex + 1);
            }
            else
            {
                if (enableDebugLog) Debug.LogWarning($"Level {currentLevelIndex + 1} is not valid!");
            }
        }
    }
    
    private void CenterMaze(Vector2Int gridSize)
    {
        float gridWidth = gridSize.x * cellSize;
        float gridHeight = gridSize.y * cellSize;
        
        gridOffset = new Vector2(
            -gridWidth / 2f + cellSize / 2f,
            -gridHeight / 2f + cellSize / 2f
        );
    }
    
    public void LoadLevel(MazeLevelData levelData)
    {
        ClearLevel();
        currentLives = maxLives;
        isGameActive = true; // Enable input for new level
        OnLivesChanged?.Invoke(currentLives);
        
        foreach (var arrowPath in levelData.arrows)
        {
            SpawnArrow(arrowPath);
        }
    }
    
    public void NextLevel()
    {
        if (HasNextLevel)
        {
            currentLevelIndex++;
            LoadCurrentLevel();
        }
    }
    
    public void RetryLevel()
    {
        LoadCurrentLevel();
    }
    
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    
    private void ClearLevel()
    {
        // Destroy all spawned arrow GameObjects (including cleared ones with dots)
        foreach (var arrowObj in allSpawnedArrows)
        {
            if (arrowObj != null)
            {
                Destroy(arrowObj);
            }
        }
        allSpawnedArrows.Clear();
        activeArrows.Clear();
    }
    
    private void SpawnArrow(ArrowPath arrowPath)
    {
        if (arrowPath.path.Count < 2) return;
        
        GameObject arrowObj = new GameObject("Arrow");
        Arrow arrow = arrowObj.AddComponent<Arrow>();
        arrow.Initialize(arrowPath.path, arrowPath.headDirection, this);
        
        arrow.SetSpriteScale(spriteScale);
        arrow.SetLineWidth(lineWidth);
        arrow.SetLineColor(lineColor);
        
        for (int i = 0; i < arrowPath.path.Count; i++)
        {
            Vector3 worldPos = GridToWorld(arrowPath.path[i]);
            GameObject dot = CreateDot(worldPos);
            dot.transform.SetParent(arrowObj.transform); // Parent to arrow so it gets destroyed with it
            arrow.AddSegment(dot, isDot: true);
        }
        
        activeArrows.Add(arrow);
        allSpawnedArrows.Add(arrowObj); // Track for cleanup
    }
    
    private GameObject CreateDot(Vector3 position)
    {
        GameObject dot = new GameObject("Dot");
        dot.transform.position = position;
        dot.transform.localScale = Vector3.one * spriteScale * 0.2f;
        
        SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
        sr.sprite = underArrowDot;
        sr.sortingOrder = 1; // Above background
        sr.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        
        return dot;
    }
    

    
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize + gridOffset.x,
            gridPos.y * cellSize + gridOffset.y,
            0
        );
    }
    
    public void OnArrowClicked(Arrow arrow)
    {
        // Arrow always starts moving - collision detection happens during movement
        arrow.StartMoving();
    }
    
    /// <summary>
    /// Called by Arrow when it bumps into a blocker and reverses
    /// </summary>
    public void OnArrowBlocked(Arrow arrow)
    {
        currentLives--;
        OnLivesChanged?.Invoke(currentLives);
        
        if (currentLives <= 0)
        {
            isGameActive = false;
            if (enableDebugLog) Debug.Log("Game Over!");
        }
    }
    
    public void OnArrowCleared(Arrow arrow)
    {
        activeArrows.Remove(arrow);
        
        if (activeArrows.Count == 0)
        {
            // Save progress - mark this level as completed
            int levelJustCompleted = currentLevelIndex + 1;
            if (levelJustCompleted > HighestCompletedLevel)
            {
                HighestCompletedLevel = levelJustCompleted;
            }
            
            OnAllArrowsCleared?.Invoke();
            isGameActive = false; // Disable input on level complete
            if (enableDebugLog) Debug.Log($"Level {levelJustCompleted} Complete! Highest completed: {HighestCompletedLevel}");
        }
    }
    
    /// <summary>
    /// Reset all progress (for testing)
    /// </summary>
    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(HIGHEST_COMPLETED_KEY);
        PlayerPrefs.DeleteKey(SELECTED_LEVEL_KEY);
        PlayerPrefs.Save();
        if (enableDebugLog) Debug.Log("Progress reset!");
    }
    
    /// <summary>
    /// Toggle direction guide lines on all arrows
    /// </summary>
    public void ToggleDirectionGuides()
    {
        isDirectionGuideVisible = !isDirectionGuideVisible;
        
        foreach (var arrow in activeArrows)
        {
            if (arrow != null && !arrow.IsCleared && !arrow.IsMoving)
            {
                arrow.SetDirectionGuideVisible(isDirectionGuideVisible);
            }
        }
        
        if (enableDebugLog) Debug.Log($"Direction guides: {(isDirectionGuideVisible ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// Get current state of direction guides
    /// </summary>
    public bool IsDirectionGuideVisible => isDirectionGuideVisible;
}

