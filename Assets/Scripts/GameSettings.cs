using UnityEngine;

/// <summary>
/// Game performance settings - attach to any object in first scene
/// </summary>
public class GameSettings : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Performance Settings")]
    [SerializeField] private int targetFrameRate = 120;
    [SerializeField] private bool vSync = false;
    
    private void Awake()
    {
        // Set high target frame rate for smooth gameplay
        Application.targetFrameRate = targetFrameRate;
        
        // Disable VSync for lowest latency (better for mobile)
        QualitySettings.vSyncCount = vSync ? 1 : 0;
        
        // Prevent screen from dimming
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
