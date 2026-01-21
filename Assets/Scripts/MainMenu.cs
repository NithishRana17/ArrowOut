using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu controller with level selection
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Main Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelSelectButton;
    
    [Header("Level Select Panel")]
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private Button closeLevelSelectButton;
    
    [Header("Level Buttons (assign 5 buttons)")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;
    [SerializeField] private Button level4Button;
    [SerializeField] private Button level5Button;
    
    [Header("Lock Icons (optional - child of each button)")]
    [SerializeField] private GameObject level1Lock;
    [SerializeField] private GameObject level2Lock;
    [SerializeField] private GameObject level3Lock;
    [SerializeField] private GameObject level4Lock;
    [SerializeField] private GameObject level5Lock;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    
    private Button[] levelButtons;
    private GameObject[] lockIcons;
    
    private void Start()
    {
        // Build arrays
        levelButtons = new Button[] { level1Button, level2Button, level3Button, level4Button, level5Button };
        lockIcons = new GameObject[] { level1Lock, level2Lock, level3Lock, level4Lock, level5Lock };
        
        SetupMainButtons();
        SetupLevelButtons();
        UpdateLevelButtonStates();
        
        // Hide level select panel initially
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }
    
    private void SetupMainButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        
        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        }
        
        if (closeLevelSelectButton != null)
        {
            closeLevelSelectButton.onClick.AddListener(CloseLevelSelect);
        }
    }
    
    private void SetupLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] != null)
            {
                int levelNumber = i + 1; // Capture for closure
                levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(levelNumber));
            }
        }
    }
    
    private void UpdateLevelButtonStates()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            bool isUnlocked = IsLevelUnlocked(levelNumber);
            
            // Update button interactable (keeps original colors, just dims when disabled)
            if (levelButtons[i] != null)
            {
                levelButtons[i].interactable = isUnlocked;
                // Button's ColorBlock handles disabled state automatically
            }
            
            // Show/hide lock icon
            if (lockIcons[i] != null)
            {
                lockIcons[i].SetActive(!isUnlocked);
            }
        }
    }
    
    private bool IsLevelUnlocked(int levelNumber)
    {
        // Level 1 always unlocked
        if (levelNumber <= 1) return true;
        
        // Other levels need previous level completed
        int highestCompleted = PlayerPrefs.GetInt("HighestCompletedLevel", 0);
        return levelNumber <= highestCompleted + 1;
    }
    
    private void OnPlayClicked()
    {
        // Start from level 1
        MazeManager.SetSelectedLevel(1);
        SceneManager.LoadScene(gameSceneName);
    }
    
    private void OnLevelSelectClicked()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
            UpdateLevelButtonStates(); // Refresh unlock states
        }
    }
    
    private void CloseLevelSelect()
    {
        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
        }
    }
    
    private void OnLevelButtonClicked(int levelNumber)
    {
        if (IsLevelUnlocked(levelNumber))
        {
            MazeManager.SetSelectedLevel(levelNumber);
            SceneManager.LoadScene(gameSceneName);
        }
    }
    
    /// <summary>
    /// Reset all progress (for testing - call from button or inspector)
    /// </summary>
    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey("HighestCompletedLevel");
        PlayerPrefs.DeleteKey("SelectedLevel");
        PlayerPrefs.Save();
        UpdateLevelButtonStates();
        if (enableDebugLog) Debug.Log("All progress reset!");
    }
}
