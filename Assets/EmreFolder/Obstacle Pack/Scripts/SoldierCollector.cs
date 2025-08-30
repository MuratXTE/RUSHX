using UnityEngine;
using TMPro;
using DG.Tweening;

public class SoldierCollector : MonoBehaviour
{
    [Header("Collector Settings")]
    public int soldierCount = 36;
    public bool destroyOnCollect = true;
    public bool showValueText = true;
    
    [Header("Visual Settings")]
    public TextMeshPro valueText;
    public GameObject collectEffect; // Optional particle effect
    
    [Header("Animation Settings")]
    public float collectAnimationDuration = 0.5f;
    public float textBounceScale = 1.2f;
    public Ease collectEase = Ease.OutBack;
    
    private bool hasBeenCollected = false;
    
    void Start()
    {
        // Create or update the display text
        if (showValueText)
        {
            if (valueText == null)
            {
                // Create a text display if none exists
                GameObject textObj = new GameObject("ValueText");
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = Vector3.up * 2f;
                
                valueText = textObj.AddComponent<TextMeshPro>();
                valueText.fontSize = 4;
                valueText.alignment = TextAlignmentOptions.Center;
                valueText.color = Color.green;
            }
            
            valueText.text = "+" + soldierCount.ToString();
            
            // Add floating animation to the text
            if (valueText != null)
            {
                valueText.transform.DOMoveY(valueText.transform.position.y + 0.3f, 1f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        
        // Add a subtle scale pulse to the collector
        transform.DOScale(transform.localScale * 1.1f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;
        
        // Check if the collider belongs to the player or army manager
        ArmyManager armyManager = other.GetComponent<ArmyManager>();
        if (armyManager == null)
        {
            // Try to find army manager in parent
            armyManager = other.GetComponentInParent<ArmyManager>();
        }
        
        // Also check for PlayerController tag
        if (armyManager == null && other.CompareTag("Player"))
        {
            armyManager = other.GetComponent<ArmyManager>();
            if (armyManager == null)
                armyManager = FindFirstObjectByType<ArmyManager>();
        }
        
        if (armyManager != null)
        {
            CollectSoldiers(armyManager);
        }
    }
    
    private void CollectSoldiers(ArmyManager armyManager)
    {
        hasBeenCollected = true;
        
        // Kill all tweens on this object
        transform.DOKill();
        if (valueText != null)
            valueText.transform.DOKill();
        
        // Animate collection with scale bounce and text popup
        Sequence collectSequence = DOTween.Sequence();
        
        // Scale bounce effect
        collectSequence.Append(transform.DOScale(transform.localScale * 1.3f, collectAnimationDuration * 0.3f).SetEase(Ease.OutBack));
        collectSequence.Append(transform.DOScale(Vector3.zero, collectAnimationDuration * 0.7f).SetEase(Ease.InBack));
        
        // Text animation - bounce up and fade
        if (valueText != null)
        {
            collectSequence.Insert(0, valueText.transform.DOScale(textBounceScale, collectAnimationDuration * 0.4f).SetEase(collectEase));
            collectSequence.Insert(collectAnimationDuration * 0.3f, valueText.DOFade(0f, collectAnimationDuration * 0.5f));
            collectSequence.Insert(0, valueText.transform.DOMoveY(valueText.transform.position.y + 2f, collectAnimationDuration).SetEase(Ease.OutQuart));
        }
        
        // Add soldiers to army
        armyManager.AddSoldiers(soldierCount);
        
        // Play collection effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }
        
        // Destroy after animation completes
        collectSequence.OnComplete(() => {
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        });
    }
    
    // Method to manually set soldier count (useful for procedural generation)
    public void SetSoldierCount(int count)
    {
        soldierCount = count;
        if (valueText != null)
        {
            valueText.text = "+" + soldierCount.ToString();
        }
    }
}
