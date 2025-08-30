using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Murat;

public class PuanDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Legacy Text component to display the current Puan value")]
    public Text puanText;
    
    [Header("Display Settings")]
    [Tooltip("Prefix text to show before the puan value (e.g., 'Points: ')")]
    public string displayPrefix = "Puan: ";
    
    [Tooltip("How often to update the display (in seconds)")]
    public float updateInterval = 0.1f;
    
    [Tooltip("Should the display update automatically?")]
    public bool autoUpdate = true;
    
    private BellekYonetim bellekYonetim;
    private int lastPuanValue = -1;
    
    void Start()
    {
        // Initialize BellekYonetim
        bellekYonetim = new BellekYonetim();
        
        // Find Text component if not assigned
        if (puanText == null)
        {
            puanText = GetComponent<Text>();
            if (puanText == null)
            {
                puanText = GetComponentInChildren<Text>();
            }
        }
        
        if (puanText == null)
        {
            Debug.LogError("PuanDisplay: No Text component found! Please assign a Text component to puanText field.");
            return;
        }
        
        // Initial update
        UpdatePuanDisplay();
        
        // Start automatic updates if enabled
        if (autoUpdate)
        {
            InvokeRepeating(nameof(UpdatePuanDisplay), updateInterval, updateInterval);
        }
    }
    
    void UpdatePuanDisplay()
    {
        if (puanText == null || bellekYonetim == null) return;
        
        try
        {
            // Read current puan value
            int currentPuan = bellekYonetim.VeriOku_i("Puan");
            
            // Only update if value changed to save performance
            if (currentPuan != lastPuanValue)
            {
                puanText.text = displayPrefix + currentPuan.ToString();
                lastPuanValue = currentPuan;
                
                Debug.Log($"PuanDisplay: Updated to {currentPuan}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PuanDisplay: Error reading Puan value: {e.Message}");
        }
    }
    
    // Public method to manually force an update
    public void ForceUpdate()
    {
        lastPuanValue = -1; // Force update next time
        UpdatePuanDisplay();
    }
    
    // Public method to add points and update display
    public void AddPoints(int pointsToAdd)
    {
        if (bellekYonetim == null) return;
        
        try
        {
            int currentPoints = bellekYonetim.VeriOku_i("Puan");
            int newPoints = currentPoints + pointsToAdd;
            bellekYonetim.VeriKaydet_int("Puan", newPoints);
            
            Debug.Log($"PuanDisplay: Added {pointsToAdd} points. Total: {newPoints}");
            
            // Force immediate update
            ForceUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PuanDisplay: Error adding points: {e.Message}");
        }
    }
    
    // Public method to set points and update display
    public void SetPoints(int newPoints)
    {
        if (bellekYonetim == null) return;
        
        try
        {
            bellekYonetim.VeriKaydet_int("Puan", newPoints);
            
            Debug.Log($"PuanDisplay: Set points to {newPoints}");
            
            // Force immediate update
            ForceUpdate();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PuanDisplay: Error setting points: {e.Message}");
        }
    }
    
    // Get current puan value
    public int GetCurrentPuan()
    {
        if (bellekYonetim == null) return 0;
        
        try
        {
            return bellekYonetim.VeriOku_i("Puan");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PuanDisplay: Error getting current puan: {e.Message}");
            return 0;
        }
    }
    
    void OnDestroy()
    {
        // Cancel repeating invoke when object is destroyed
        if (autoUpdate)
        {
            CancelInvoke(nameof(UpdatePuanDisplay));
        }
    }
    
    // Enable/disable automatic updates
    public void SetAutoUpdate(bool enabled)
    {
        if (autoUpdate == enabled) return;
        
        autoUpdate = enabled;
        
        if (autoUpdate)
        {
            InvokeRepeating(nameof(UpdatePuanDisplay), updateInterval, updateInterval);
        }
        else
        {
            CancelInvoke(nameof(UpdatePuanDisplay));
        }
    }
    
    // Change update interval (only affects if auto-update is enabled)
    public void SetUpdateInterval(float newInterval)
    {
        updateInterval = newInterval;
        
        if (autoUpdate)
        {
            // Restart with new interval
            CancelInvoke(nameof(UpdatePuanDisplay));
            InvokeRepeating(nameof(UpdatePuanDisplay), updateInterval, updateInterval);
        }
    }
    
    // Set display prefix
    public void SetDisplayPrefix(string newPrefix)
    {
        displayPrefix = newPrefix;
        ForceUpdate(); // Update immediately with new prefix
    }
    
    // Context menu methods for testing in the editor
    [ContextMenu("Add 100 Points")]
    void TestAdd100Points()
    {
        AddPoints(100);
    }
    
    [ContextMenu("Add 1000 Points")]
    void TestAdd1000Points()
    {
        AddPoints(1000);
    }
    
    [ContextMenu("Reset Points to 0")]
    void TestResetPoints()
    {
        SetPoints(0);
    }
    
    [ContextMenu("Force Update Display")]
    void TestForceUpdate()
    {
        ForceUpdate();
    }
}
