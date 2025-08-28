using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class DOTweenSceneManager : MonoBehaviour
{
    [Header("DOTween Scene Management")]
    [Tooltip("Should DOTween be automatically killed when scene changes?")]
    public bool autoKillOnSceneChange = true;
    
    [Tooltip("Should this object persist across scenes?")]
    public bool dontDestroyOnLoad = true;
    
    [Tooltip("Advanced cleanup with multiple passes")]
    public bool useAdvancedCleanup = true;
    
    [Tooltip("Time between cleanup passes (in seconds)")]
    [Range(0.1f, 2f)]
    public float cleanupInterval = 0.5f;
    
    [Tooltip("Number of cleanup passes to perform")]
    [Range(1, 5)]
    public int cleanupPasses = 3;
    
    [Tooltip("Enable detailed logging for debugging")]
    public bool enableDetailedLogging = true;
    
    private static DOTweenSceneManager instance;
    private HashSet<Transform> trackedTransforms = new HashSet<Transform>();
    private bool isCleanupInProgress = false;
    
    void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            // Subscribe to scene change events
            if (autoKillOnSceneChange)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            
            // Initialize DOTween with safe settings
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultUpdateType = UpdateType.Normal;
            DOTween.defaultTimeScaleIndependent = false;
            DOTween.useSafeMode = true; // Enable safe mode to prevent errors
            
            if (enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: Initialized with advanced cleanup features");
        }
        else if (instance != this)
        {
            // Destroy duplicate instances
            if (enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: Destroying duplicate instance");
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (autoKillOnSceneChange)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        
        // Perform final cleanup synchronously - can't use coroutine in OnDestroy
        PerformFinalCleanupSync();
        
        if (instance == this)
        {
            instance = null;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (enableDetailedLogging)
            Debug.Log($"DOTweenSceneManager: Scene '{scene.name}' loaded, starting advanced cleanup");
        
        if (useAdvancedCleanup)
        {
            StartCoroutine(AdvancedCleanupCoroutine());
        }
        else
        {
            KillAllDOTweenAnimations();
        }
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        if (enableDetailedLogging)
            Debug.Log($"DOTweenSceneManager: Scene '{scene.name}' unloaded, performing cleanup");
        
        KillAllDOTweenAnimations();
        trackedTransforms.Clear();
    }
    
    // Advanced cleanup coroutine with multiple passes
    private IEnumerator AdvancedCleanupCoroutine()
    {
        if (isCleanupInProgress) yield break;
        isCleanupInProgress = true;
        
        if (enableDetailedLogging)
            Debug.Log($"DOTweenSceneManager: Starting advanced cleanup with {cleanupPasses} passes");
        
        for (int pass = 0; pass < cleanupPasses; pass++)
        {
            if (enableDetailedLogging)
                Debug.Log($"DOTweenSceneManager: Cleanup pass {pass + 1}/{cleanupPasses}");
            
            // Kill all tweens with different approaches each pass
            switch (pass)
            {
                case 0:
                    DOTween.KillAll(false); // Don't complete tweens
                    break;
                case 1:
                    DOTween.KillAll(true); // Complete tweens
                    CleanupTrackedTransforms();
                    break;
                case 2:
                    DOTween.Clear(true); // Destroy all sequences
                    break;
                default:
                    SafeKillAllAnimations();
                    break;
            }
            
            // Wait between passes
            if (pass < cleanupPasses - 1)
            {
                yield return new WaitForSeconds(cleanupInterval);
            }
        }
        
        // Final comprehensive cleanup
        DOTween.Clear();
        System.GC.Collect(); // Force garbage collection
        
        if (enableDetailedLogging)
            Debug.Log("DOTweenSceneManager: Advanced cleanup completed");
        
        isCleanupInProgress = false;
    }
    
    // Safe animation killing with null checks
    private void SafeKillAllAnimations()
    {
        try
        {
            // Get all active tweens and check their targets
            var activeTweens = DOTween.TotalActiveTweens();
            if (enableDetailedLogging)
                Debug.Log($"DOTweenSceneManager: Found {activeTweens} active tweens to clean up");
            
            DOTween.KillAll();
            
            // Additional cleanup for specific Unity objects
            var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var transform in allTransforms)
            {
                if (transform != null)
                {
                    transform.DOKill(true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"DOTweenSceneManager: Error during safe cleanup: {e.Message}");
        }
    }
    
    // Clean up tracked transforms that no longer exist
    private void CleanupTrackedTransforms()
    {
        var toRemove = new List<Transform>();
        
        foreach (var transform in trackedTransforms)
        {
            if (transform == null)
            {
                toRemove.Add(transform);
            }
            else
            {
                // Kill animations on this specific transform
                try
                {
                    transform.DOKill(true);
                    DOTween.Kill(transform, true);
                }
                catch (Exception e)
                {
                    if (enableDetailedLogging)
                        Debug.LogWarning($"DOTweenSceneManager: Error killing animation for transform: {e.Message}");
                }
            }
        }
        
        foreach (var transform in toRemove)
        {
            trackedTransforms.Remove(transform);
        }
        
        if (enableDetailedLogging && toRemove.Count > 0)
            Debug.Log($"DOTweenSceneManager: Cleaned up {toRemove.Count} null transform references");
    }
    
    // Final cleanup when manager is being destroyed
    private IEnumerator FinalCleanup()
    {
        if (enableDetailedLogging)
            Debug.Log("DOTweenSceneManager: Performing final cleanup before destruction");
        
        DOTween.KillAll(false);
        yield return new WaitForEndOfFrame();
        
        DOTween.Clear(true);
        trackedTransforms.Clear();
        
        if (enableDetailedLogging)
            Debug.Log("DOTweenSceneManager: Final cleanup completed");
    }
    
    // Synchronous version of final cleanup for OnDestroy (can't use coroutines there)
    private void PerformFinalCleanupSync()
    {
        try
        {
            if (enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: Performing synchronous final cleanup before destruction");
            
            // Kill all tweens immediately
            DOTween.KillAll(false);
            
            // Clear all sequences and cached data
            DOTween.Clear(true);
            
            // Clear tracked transforms
            if (trackedTransforms != null)
                trackedTransforms.Clear();
            
            // Force garbage collection to clean up immediately
            System.GC.Collect();
            
            if (enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: Synchronous final cleanup completed");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DOTweenSceneManager: Error during synchronous cleanup: {e.Message}");
        }
    }
    
    // Method to kill all DOTween animations globally
    public static void KillAllDOTweenAnimations()
    {
        try
        {
            // Kill all active tweens
            DOTween.KillAll(false);
            
            // Clear all cached tweens
            DOTween.Clear(true);
            
            if (instance != null && instance.enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: All DOTween animations killed globally");
        }
        catch (Exception e)
        {
            Debug.LogError($"DOTweenSceneManager: Error during global cleanup: {e.Message}");
        }
    }
    
    // Enhanced method to track and kill specific transform animations
    public static void RegisterTransform(Transform transform)
    {
        if (instance != null && transform != null)
        {
            instance.trackedTransforms.Add(transform);
            if (instance.enableDetailedLogging)
                Debug.Log($"DOTweenSceneManager: Registered transform {transform.name} for tracking");
        }
    }
    
    public static void UnregisterTransform(Transform transform)
    {
        if (instance != null && transform != null)
        {
            instance.trackedTransforms.Remove(transform);
            if (instance.enableDetailedLogging)
                Debug.Log($"DOTweenSceneManager: Unregistered transform {transform.name} from tracking");
        }
    }
    
    // Safe method to kill animations on a specific transform
    public static void SafeKillTransform(Transform transform)
    {
        if (transform == null) return;
        
        try
        {
            // Multiple approaches to ensure cleanup
            transform.DOKill(true);
            DOTween.Kill(transform, true);
            DOTween.Kill(transform.gameObject, true);
            
            if (instance != null && instance.enableDetailedLogging)
                Debug.Log($"DOTweenSceneManager: Safely killed animations for {transform.name}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"DOTweenSceneManager: Error killing transform animations: {e.Message}");
        }
    }
    
    // Method to perform immediate cleanup (for critical situations)
    public static void EmergencyCleanup()
    {
        Debug.LogWarning("DOTweenSceneManager: Performing emergency cleanup!");
        
        try
        {
            DOTween.KillAll(false);
            DOTween.Clear(true);
            
            // Force immediate garbage collection
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            
            if (instance != null)
            {
                instance.trackedTransforms.Clear();
                instance.isCleanupInProgress = false;
            }
            
            Debug.Log("DOTweenSceneManager: Emergency cleanup completed");
        }
        catch (Exception e)
        {
            Debug.LogError($"DOTweenSceneManager: Error during emergency cleanup: {e.Message}");
        }
    }
    
    // Public method to manually kill all animations
    public void ManualKillAll()
    {
        if (useAdvancedCleanup)
        {
            StartCoroutine(AdvancedCleanupCoroutine());
        }
        else
        {
            KillAllDOTweenAnimations();
        }
    }
    
    // Method to pause all DOTween animations
    public static void PauseAllDOTweenAnimations()
    {
        try
        {
            DOTween.PauseAll();
            if (instance != null && instance.enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: All DOTween animations paused");
        }
        catch (Exception e)
        {
            Debug.LogError($"DOTweenSceneManager: Error pausing animations: {e.Message}");
        }
    }
    
    // Method to resume all DOTween animations
    public static void ResumeAllDOTweenAnimations()
    {
        try
        {
            DOTween.PlayAll();
            if (instance != null && instance.enableDetailedLogging)
                Debug.Log("DOTweenSceneManager: All DOTween animations resumed");
        }
        catch (Exception e)
        {
            Debug.LogError($"DOTweenSceneManager: Error resuming animations: {e.Message}");
        }
    }
    
    // Get statistics about active tweens
    public static void LogDOTweenStats()
    {
        try
        {
            int activeTweens = DOTween.TotalActiveTweens();
            int totalTweens = DOTween.TotalPlayingTweens();
            int pausedTweens = activeTweens - totalTweens;
            
            Debug.Log($"DOTween Stats - Active: {activeTweens}, Playing: {totalTweens}, Paused: {pausedTweens}");
            
            if (instance != null)
            {
                Debug.Log($"Tracked Transforms: {instance.trackedTransforms.Count}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"DOTweenSceneManager: Error getting stats: {e.Message}");
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PauseAllDOTweenAnimations();
        }
        else
        {
            ResumeAllDOTweenAnimations();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            PauseAllDOTweenAnimations();
        }
        else
        {
            ResumeAllDOTweenAnimations();
        }
    }
    
    // Enhanced context menu methods for testing
    [ContextMenu("Kill All DOTween Animations (Basic)")]
    void TestKillAllBasic()
    {
        KillAllDOTweenAnimations();
    }
    
    [ContextMenu("Kill All DOTween Animations (Advanced)")]
    void TestKillAllAdvanced()
    {
        StartCoroutine(AdvancedCleanupCoroutine());
    }
    
    [ContextMenu("Emergency Cleanup")]
    void TestEmergencyCleanup()
    {
        EmergencyCleanup();
    }
    
    [ContextMenu("Show DOTween Statistics")]
    void TestShowStats()
    {
        LogDOTweenStats();
    }
    
    [ContextMenu("Clean Tracked Transforms")]
    void TestCleanTracked()
    {
        CleanupTrackedTransforms();
    }
    
    [ContextMenu("Pause All DOTween Animations")]
    void TestPauseAll()
    {
        PauseAllDOTweenAnimations();
    }
    
    [ContextMenu("Resume All DOTween Animations")]
    void TestResumeAll()
    {
        ResumeAllDOTweenAnimations();
    }
    
    [ContextMenu("Toggle Advanced Cleanup")]
    void TestToggleAdvanced()
    {
        useAdvancedCleanup = !useAdvancedCleanup;
        Debug.Log($"Advanced cleanup is now: {(useAdvancedCleanup ? "ENABLED" : "DISABLED")}");
    }
    
    [ContextMenu("Toggle Detailed Logging")]
    void TestToggleLogging()
    {
        enableDetailedLogging = !enableDetailedLogging;
        Debug.Log($"Detailed logging is now: {(enableDetailedLogging ? "ENABLED" : "DISABLED")}");
    }
}
