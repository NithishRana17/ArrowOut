using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages all UI elements including HUD, win/lose screens
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private List<Image> lifeIcons = new List<Image>();
    [SerializeField] private Button showGuidesButton;
    [SerializeField] private TextMeshProUGUI guidesButtonText;
    
    [Header("Win Screen")]
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button winRetryButton;
    [SerializeField] private Button winMenuButton;
    
    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreenPanel;
    [SerializeField] private Button loseRetryButton;
    [SerializeField] private Button loseMenuButton;
    
    [Header("Visual Settings")]
    [SerializeField] private Color lifeActiveColor = Color.red;
    [SerializeField] private Color lifeInactiveColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    private void OnEnable()
    {
        MazeManager.OnLivesChanged += UpdateLivesDisplay;
        MazeManager.OnAllArrowsCleared += OnLevelComplete;
        MazeManager.OnLevelChanged += UpdateLevelDisplay;
    }
    
    private void OnDisable()
    {
        MazeManager.OnLivesChanged -= UpdateLivesDisplay;
        MazeManager.OnAllArrowsCleared -= OnLevelComplete;
        MazeManager.OnLevelChanged -= UpdateLevelDisplay;
    }
    
    private void Start()
    {
        SetupButtons();
        HideEndScreens();
        
        // Initial level display
        if (MazeManager.Instance != null)
        {
            UpdateLevelDisplay(MazeManager.Instance.CurrentLevel);
        }
    }
    
    private void SetupButtons()
    {
        // Win screen buttons
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }
        if (winRetryButton != null)
        {
            winRetryButton.onClick.AddListener(OnRetryClicked);
        }
        if (winMenuButton != null)
        {
            winMenuButton.onClick.AddListener(OnMenuClicked);
        }
        
        // Lose screen buttons
        if (loseRetryButton != null)
        {
            loseRetryButton.onClick.AddListener(OnRetryClicked);
        }
        if (loseMenuButton != null)
        {
            loseMenuButton.onClick.AddListener(OnMenuClicked);
        }
        
        // Guide button
        if (showGuidesButton != null)
        {
            showGuidesButton.onClick.AddListener(OnShowGuidesClicked);
        }
    }
    
    private void OnShowGuidesClicked()
    {
        MazeManager.Instance?.ToggleDirectionGuides();
        UpdateGuidesButtonText();
    }
    
    private void UpdateGuidesButtonText()
    {
        if (guidesButtonText != null && MazeManager.Instance != null)
        {
            bool isOn = MazeManager.Instance.IsDirectionGuideVisible;
            guidesButtonText.text = isOn ? "Hide Guides" : "Show Guides";
        }
    }
    
    private void OnNextLevelClicked()
    {
        HideEndScreens();
        MazeManager.Instance?.NextLevel();
    }
    
    private void OnRetryClicked()
    {
        HideEndScreens();
        MazeManager.Instance?.RetryLevel();
    }
    
    private void OnMenuClicked()
    {
        MazeManager.Instance?.GoToMainMenu();
    }
    
    public void UpdateLevelDisplay(int levelNumber)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {levelNumber}";
        }
    }
    
    public void UpdateLivesDisplay(int currentLives)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (lifeIcons[i] != null)
            {
                lifeIcons[i].color = i < currentLives ? lifeActiveColor : lifeInactiveColor;
            }
        }
        
        // Check for game over
        if (currentLives <= 0)
        {
            // Show interstitial ad on game over
            AdsManager.Instance?.ShowInterstitial();
            ShowLoseScreen();
        }
    }
    
    private void OnLevelComplete()
    {
        // Show interstitial ad on level complete
        AdsManager.Instance?.ShowInterstitial();
        
        bool hasNext = MazeManager.Instance != null && MazeManager.Instance.HasNextLevel;
        ShowWinScreen(hasNext);
    }
    
    public void ShowWinScreen(bool hasNextLevel)
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
            
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            }
        }
        
        // Hide guides button when level ends
        if (showGuidesButton != null)
        {
            showGuidesButton.gameObject.SetActive(false);
        }
    }
    
    public void ShowLoseScreen()
    {
        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(true);
        }
        
        // Hide guides button when level ends
        if (showGuidesButton != null)
        {
            showGuidesButton.gameObject.SetActive(false);
        }
    }
    
    public void HideEndScreens()
    {
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(false);
        }
        if (loseScreenPanel != null)
        {
            loseScreenPanel.SetActive(false);
        }
        
        // Show guides button when playing
        if (showGuidesButton != null)
        {
            showGuidesButton.gameObject.SetActive(true);
        }
        
        // Reset button text
        if (guidesButtonText != null)
        {
            guidesButtonText.text = "Show Guides";
        }
    }
}
