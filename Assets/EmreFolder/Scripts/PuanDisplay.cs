using UnityEngine;
using UnityEngine.UI;
using Murat;

public class PuanDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Text puanText;

    [Header("Display Settings")]
    public string displayPrefix = "Puan: ";
    public float updateInterval = 0.1f;
    public bool autoUpdate = true;

    private BellekYonetim bellekYonetim;
    private int lastPuanValue = -1;

    void Start()
    {
        bellekYonetim = new BellekYonetim();

        if (puanText == null)
        {
            puanText = GetComponent<Text>();
            if (puanText == null)
            {
                puanText = GetComponentInChildren<Text>();
            }
        }

        if (puanText == null) return;

        UpdatePuanDisplay();

        if (autoUpdate)
        {
            InvokeRepeating(nameof(UpdatePuanDisplay), updateInterval, updateInterval);
        }
    }

    void UpdatePuanDisplay()
    {
        if (puanText == null || bellekYonetim == null) return;

        int currentPuan = bellekYonetim.VeriOku_i("Puan");

        if (currentPuan != lastPuanValue)
        {
            puanText.text = displayPrefix + currentPuan.ToString();
            lastPuanValue = currentPuan;
        }
    }

    public void ForceUpdate()
    {
        lastPuanValue = -1;
        UpdatePuanDisplay();
    }

    public void AddPoints(int pointsToAdd)
    {
        if (bellekYonetim == null) return;

        int currentPoints = bellekYonetim.VeriOku_i("Puan");
        int newPoints = currentPoints + pointsToAdd;
        bellekYonetim.VeriKaydet_int("Puan", newPoints);

        ForceUpdate();
    }

    public void SetPoints(int newPoints)
    {
        if (bellekYonetim == null) return;

        bellekYonetim.VeriKaydet_int("Puan", newPoints);
        ForceUpdate();
    }

    public int GetCurrentPuan()
    {
        if (bellekYonetim == null) return 0;
        return bellekYonetim.VeriOku_i("Puan");
    }

    void OnDestroy()
    {
        if (autoUpdate)
        {
            CancelInvoke(nameof(UpdatePuanDisplay));
        }
    }

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

    public void SetUpdateInterval(float newInterval)
    {
        updateInterval = newInterval;

        if (autoUpdate)
        {
            CancelInvoke(nameof(UpdatePuanDisplay));
            InvokeRepeating(nameof(UpdatePuanDisplay), updateInterval, updateInterval);
        }
    }

    public void SetDisplayPrefix(string newPrefix)
    {
        displayPrefix = newPrefix;
        ForceUpdate();
    }
}
