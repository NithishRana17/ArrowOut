using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// Manages Unity Ads for Banner and Interstitial ads
/// </summary>
public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;
    
    [Header("Unity Ads Settings")]
    [SerializeField] private string androidGameId = "6028445";
    [SerializeField] private string iosGameId = "6028444";
    [SerializeField] private bool testMode = false; // Set to true for testing
    
    [Header("Ad Unit IDs")]
    [SerializeField] private string bannerAdUnitId = "Banner_Android";
    [SerializeField] private string interstitialAdUnitId = "Interstitial_Android";
    
    private bool isInitialized = false;
    private bool isBannerShowing = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeAds();
    }
    
    private void InitializeAds()
    {
        string gameId = GetGameId();
        
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            if (enableDebugLog) Debug.Log($"[AdsManager] Initializing Unity Ads with Game ID: {gameId}");
            Advertisement.Initialize(gameId, testMode, this);
        }
    }
    
    private string GetGameId()
    {
#if UNITY_IOS
        return iosGameId;
#elif UNITY_ANDROID
        return androidGameId;
#else
        return androidGameId;
#endif
    }
    
    #region Initialization Callbacks
    
    public void OnInitializationComplete()
    {
        if (enableDebugLog) Debug.Log("[AdsManager] Unity Ads initialization complete!");
        isInitialized = true;
        
        // Load ads after initialization
        LoadBanner();
        LoadInterstitial();
    }
    
    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        if (enableDebugLog) Debug.LogError($"[AdsManager] Unity Ads initialization failed: {error} - {message}");
    }
    
    #endregion
    
    #region Banner Ads
    
    public void LoadBanner()
    {
        if (!isInitialized) return;
        
        // Set banner position
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        
        if (enableDebugLog) Debug.Log("[AdsManager] Loading banner...");
        Advertisement.Banner.Load(bannerAdUnitId, new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        });
    }
    
    private void OnBannerLoaded()
    {
        if (enableDebugLog) Debug.Log("[AdsManager] Banner loaded successfully!");
        ShowBanner();
    }
    
    private void OnBannerError(string message)
    {
        if (enableDebugLog) Debug.LogWarning($"[AdsManager] Banner load error: {message}");
        // Retry after delay
        Invoke(nameof(LoadBanner), 30f);
    }
    
    public void ShowBanner()
    {
        if (!isInitialized) return;
        
        if (enableDebugLog) Debug.Log("[AdsManager] Showing banner");
        Advertisement.Banner.Show(bannerAdUnitId, new BannerOptions
        {
            showCallback = () => { isBannerShowing = true; },
            hideCallback = () => { isBannerShowing = false; }
        });
    }
    
    public void HideBanner()
    {
        if (enableDebugLog) Debug.Log("[AdsManager] Hiding banner");
        Advertisement.Banner.Hide();
        isBannerShowing = false;
    }
    
    #endregion
    
    #region Interstitial Ads
    
    public void LoadInterstitial()
    {
        if (!isInitialized) return;
        
        if (enableDebugLog) Debug.Log("[AdsManager] Loading interstitial...");
        Advertisement.Load(interstitialAdUnitId, this);
    }
    
    public void ShowInterstitial()
    {
        if (!isInitialized)
        {
            if (enableDebugLog) Debug.Log("[AdsManager] Ads not initialized");
            return;
        }
        
        if (enableDebugLog) Debug.Log("[AdsManager] Showing interstitial");
        Advertisement.Show(interstitialAdUnitId, this);
    }
    
    #endregion
    
    #region Load Callbacks
    
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (enableDebugLog) Debug.Log($"[AdsManager] Ad loaded: {placementId}");
    }
    
    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (enableDebugLog) Debug.LogWarning($"[AdsManager] Failed to load {placementId}: {error} - {message}");
        
        // Retry loading
        if (placementId == interstitialAdUnitId)
        {
            Invoke(nameof(LoadInterstitial), 30f);
        }
    }
    
    #endregion
    
    #region Show Callbacks
    
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (enableDebugLog) Debug.LogWarning($"[AdsManager] Failed to show {placementId}: {error} - {message}");
        LoadInterstitial(); // Reload for next time
    }
    
    public void OnUnityAdsShowStart(string placementId)
    {
        if (enableDebugLog) Debug.Log($"[AdsManager] Ad started: {placementId}");
    }
    
    public void OnUnityAdsShowClick(string placementId)
    {
        if (enableDebugLog) Debug.Log($"[AdsManager] Ad clicked: {placementId}");
    }
    
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (enableDebugLog) Debug.Log($"[AdsManager] Ad complete: {placementId} - {showCompletionState}");
        LoadInterstitial(); // Pre-load next interstitial
    }
    
    #endregion
}
