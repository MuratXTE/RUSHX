using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class DOTweenSceneManager : MonoBehaviour
{
    [Header("DOTween Scene Management")]
    public bool autoKillOnSceneChange = true;
    public bool dontDestroyOnLoad = true;
    public bool useAdvancedCleanup = true;
    [Range(0.1f, 2f)]
    public float cleanupInterval = 0.5f;
    [Range(1, 5)]
    public int cleanupPasses = 3;

    private static DOTweenSceneManager instance;
    private HashSet<Transform> trackedTransforms = new HashSet<Transform>();
    private bool isCleanupInProgress = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            if (autoKillOnSceneChange)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }

            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultUpdateType = UpdateType.Normal;
            DOTween.defaultTimeScaleIndependent = false;
            DOTween.useSafeMode = true;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (autoKillOnSceneChange)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        PerformFinalCleanupSync();

        if (instance == this)
            instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (useAdvancedCleanup)
            StartCoroutine(AdvancedCleanupCoroutine());
        else
            KillAllDOTweenAnimations();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        KillAllDOTweenAnimations();
        trackedTransforms.Clear();
    }

    private IEnumerator AdvancedCleanupCoroutine()
    {
        if (isCleanupInProgress) yield break;
        isCleanupInProgress = true;

        for (int pass = 0; pass < cleanupPasses; pass++)
        {
            switch (pass)
            {
                case 0: DOTween.KillAll(false); break;
                case 1: DOTween.KillAll(true); CleanupTrackedTransforms(); break;
                case 2: DOTween.Clear(true); break;
                default: SafeKillAllAnimations(); break;
            }

            if (pass < cleanupPasses - 1)
                yield return new WaitForSeconds(cleanupInterval);
        }

        DOTween.Clear();
        System.GC.Collect();
        isCleanupInProgress = false;
    }

    private void SafeKillAllAnimations()
    {
        try
        {
            DOTween.KillAll();
            var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var transform in allTransforms)
            {
                if (transform != null)
                    transform.DOKill(true);
            }
        }
        catch { }
    }

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
                try
                {
                    transform.DOKill(true);
                    DOTween.Kill(transform, true);
                }
                catch { }
            }
        }

        foreach (var transform in toRemove)
            trackedTransforms.Remove(transform);
    }

    private void PerformFinalCleanupSync()
    {
        try
        {
            DOTween.KillAll(false);
            DOTween.Clear(true);
            trackedTransforms.Clear();
            System.GC.Collect();
        }
        catch { }
    }

    public static void KillAllDOTweenAnimations()
    {
        try
        {
            DOTween.KillAll(false);
            DOTween.Clear(true);
        }
        catch { }
    }

    public static void RegisterTransform(Transform transform)
    {
        if (instance != null && transform != null)
            instance.trackedTransforms.Add(transform);
    }

    public static void UnregisterTransform(Transform transform)
    {
        if (instance != null && transform != null)
            instance.trackedTransforms.Remove(transform);
    }

    public static void SafeKillTransform(Transform transform)
    {
        if (transform == null) return;
        try
        {
            transform.DOKill(true);
            DOTween.Kill(transform, true);
            DOTween.Kill(transform.gameObject, true);
        }
        catch { }
    }

    public static void EmergencyCleanup()
    {
        try
        {
            DOTween.KillAll(false);
            DOTween.Clear(true);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            if (instance != null)
            {
                instance.trackedTransforms.Clear();
                instance.isCleanupInProgress = false;
            }
        }
        catch { }
    }

    public void ManualKillAll()
    {
        if (useAdvancedCleanup)
            StartCoroutine(AdvancedCleanupCoroutine());
        else
            KillAllDOTweenAnimations();
    }

    public static void PauseAllDOTweenAnimations()
    {
        try { DOTween.PauseAll(); }
        catch { }
    }

    public static void ResumeAllDOTweenAnimations()
    {
        try { DOTween.PlayAll(); }
        catch { }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            PauseAllDOTweenAnimations();
        else
            ResumeAllDOTweenAnimations();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            PauseAllDOTweenAnimations();
        else
            ResumeAllDOTweenAnimations();
    }
}
