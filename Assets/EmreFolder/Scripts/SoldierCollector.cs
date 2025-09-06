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
    public GameObject collectEffect;

    [Header("Animation Settings")]
    public float collectAnimationDuration = 0.5f;
    public float textBounceScale = 1.2f;
    public Ease collectEase = Ease.OutBack;

    private bool hasBeenCollected = false;

    void Start()
    {
        if (showValueText)
        {
            if (valueText == null)
            {
                GameObject textObj = new GameObject("ValueText");
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = Vector3.up * 2f;

                valueText = textObj.AddComponent<TextMeshPro>();
                valueText.fontSize = 4;
                valueText.alignment = TextAlignmentOptions.Center;
                valueText.color = Color.green;
            }

            valueText.text = "+" + soldierCount;

            valueText.transform.DOMoveY(valueText.transform.position.y + 0.3f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        transform.DOScale(transform.localScale * 1.1f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;

        ArmyManager armyManager = other.GetComponent<ArmyManager>();
        if (armyManager == null)
            armyManager = other.GetComponentInParent<ArmyManager>();

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

        transform.DOKill();
        if (valueText != null) valueText.transform.DOKill();

        Sequence collectSequence = DOTween.Sequence();

        collectSequence.Append(transform.DOScale(transform.localScale * 1.3f, collectAnimationDuration * 0.3f).SetEase(Ease.OutBack));
        collectSequence.Append(transform.DOScale(Vector3.zero, collectAnimationDuration * 0.7f).SetEase(Ease.InBack));

        if (valueText != null)
        {
            collectSequence.Insert(0, valueText.transform.DOScale(textBounceScale, collectAnimationDuration * 0.4f).SetEase(collectEase));
            collectSequence.Insert(collectAnimationDuration * 0.3f, valueText.DOFade(0f, collectAnimationDuration * 0.5f));
            collectSequence.Insert(0, valueText.transform.DOMoveY(valueText.transform.position.y + 2f, collectAnimationDuration).SetEase(Ease.OutQuart));
        }

        armyManager.AddSoldiers(soldierCount);

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }

        collectSequence.OnComplete(() =>
        {
            if (destroyOnCollect)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        });
    }

    public void SetSoldierCount(int count)
    {
        soldierCount = count;
        if (valueText != null)
            valueText.text = "+" + soldierCount;
    }
}
