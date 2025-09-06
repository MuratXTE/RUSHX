using UnityEngine;
using TMPro;
using DG.Tweening;

public enum GateOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

public class MathGate : MonoBehaviour
{
    [Header("Gate Settings")]
    public GateOperation operation = GateOperation.Add;
    public int value = 2;
    public bool destroyOnUse = false;
    public bool showOperationText = true;

    [Header("Visual Settings")]
    public TextMeshPro operationText;
    public Color addColor = Color.green;
    public Color subtractColor = Color.red;
    public Color multiplyColor = Color.blue;
    public Color divideColor = Color.yellow;

    [Header("Gate Parts")]
    public Transform leftGate;
    public Transform rightGate;
    public float gateWidth = 3f;

    [Header("Animation Settings")]
    public float gateAnimationDuration = 0.5f;
    public float textBounceScale = 1.2f;
    public Ease gateEase = Ease.OutBack;
    public GameObject operationEffect;

    private bool hasBeenUsed = false;
    private string operationSymbol;
    private Color gateColor;

    void Start()
    {
        SetupGateDisplay();
        SetupGateAnimation();
    }

    void SetupGateDisplay()
    {
        switch (operation)
        {
            case GateOperation.Add:
                operationSymbol = "+";
                gateColor = addColor;
                break;
            case GateOperation.Subtract:
                operationSymbol = "-";
                gateColor = subtractColor;
                break;
            case GateOperation.Multiply:
                operationSymbol = "×";
                gateColor = multiplyColor;
                break;
            case GateOperation.Divide:
                operationSymbol = "÷";
                gateColor = divideColor;
                break;
        }

        if (showOperationText)
        {
            if (operationText == null)
            {
                GameObject textObj = new GameObject("OperationText");
                textObj.transform.SetParent(transform);
                textObj.transform.localPosition = Vector3.up * 2f;

                operationText = textObj.AddComponent<TextMeshPro>();
                operationText.fontSize = 6;
                operationText.alignment = TextAlignmentOptions.Center;
            }

            operationText.text = operationSymbol + value.ToString();
            operationText.color = gateColor;

            operationText.transform.DOMoveY(operationText.transform.position.y + 0.2f, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        if (leftGate != null)
        {
            Renderer leftRenderer = leftGate.GetComponent<Renderer>();
            if (leftRenderer != null)
                leftRenderer.material.color = gateColor;
        }

        if (rightGate != null)
        {
            Renderer rightRenderer = rightGate.GetComponent<Renderer>();
            if (rightRenderer != null)
                rightRenderer.material.color = gateColor;
        }
    }

    void SetupGateAnimation()
    {
        if (leftGate != null && rightGate != null)
        {
            Vector3 leftOriginalPos = leftGate.localPosition;
            Vector3 rightOriginalPos = rightGate.localPosition;

            leftGate.DOLocalMoveX(leftOriginalPos.x - 0.1f, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            rightGate.DOLocalMoveX(rightOriginalPos.x + 0.1f, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed) return;
        if (!other.CompareTag("Player")) return;

        ArmyManager armyManager = other.GetComponent<ArmyManager>();
        if (armyManager == null)
        {
            armyManager = other.GetComponentInParent<ArmyManager>();
        }
        if (armyManager == null)
        {
#if UNITY_2023_1_OR_NEWER
            armyManager = FindFirstObjectByType<ArmyManager>();
#else
            armyManager = FindObjectOfType<ArmyManager>();
#endif
        }

        if (armyManager != null)
        {
            ApplyOperation(armyManager);
        }
    }

    private void ApplyOperation(ArmyManager armyManager)
    {
        hasBeenUsed = true;

        if (leftGate != null) leftGate.DOKill();
        if (rightGate != null) rightGate.DOKill();
        if (operationText != null) operationText.transform.DOKill();

        int currentSoldiers = armyManager.GetArmySize() - 1;
        int newSoldierCount = currentSoldiers;

        switch (operation)
        {
            case GateOperation.Add:
                newSoldierCount = currentSoldiers + value;
                break;
            case GateOperation.Subtract:
                newSoldierCount = Mathf.Max(0, currentSoldiers - value);
                break;
            case GateOperation.Multiply:
                newSoldierCount = currentSoldiers * value;
                break;
            case GateOperation.Divide:
                if (value != 0)
                    newSoldierCount = currentSoldiers / value;
                break;
        }

        int soldierDifference = newSoldierCount - currentSoldiers;

        if (soldierDifference > 0)
        {
            armyManager.AddSoldiers(soldierDifference);
        }
        else if (soldierDifference < 0)
        {
            armyManager.RemoveRandomSoldiers(-soldierDifference, false);
        }

        PlayGateSound();
        AnimateGateEffect();

        if (operationEffect != null)
        {
            Instantiate(operationEffect, transform.position, transform.rotation);
        }
    }

    void AnimateGateEffect()
    {
        Sequence gateSequence = DOTween.Sequence();

        if (leftGate != null && rightGate != null)
        {
            Vector3 leftPos = leftGate.localPosition;
            Vector3 rightPos = rightGate.localPosition;

            gateSequence.Append(leftGate.DOLocalMoveX(0f, gateAnimationDuration * 0.5f).SetEase(gateEase));
            gateSequence.Join(rightGate.DOLocalMoveX(0f, gateAnimationDuration * 0.5f).SetEase(gateEase));

            gateSequence.Append(leftGate.DOLocalMoveX(leftPos.x, gateAnimationDuration * 0.5f).SetEase(gateEase));
            gateSequence.Join(rightGate.DOLocalMoveX(rightPos.x, gateAnimationDuration * 0.5f).SetEase(gateEase));
        }

        if (operationText != null)
        {
            gateSequence.Insert(0, operationText.transform.DOScale(textBounceScale, gateAnimationDuration * 0.3f).SetEase(gateEase));
            gateSequence.Insert(gateAnimationDuration * 0.2f, operationText.transform.DOScale(1f, gateAnimationDuration * 0.3f));

            gateSequence.Insert(0, operationText.DOColor(Color.white, gateAnimationDuration * 0.2f));
            gateSequence.Insert(gateAnimationDuration * 0.2f, operationText.DOColor(gateColor, gateAnimationDuration * 0.3f));
        }

        if (destroyOnUse)
        {
            gateSequence.OnComplete(() => Destroy(gameObject));
        }
    }

    void PlayGateSound()
    {
        if (SoundManager.Instance != null)
        {
            if (operation == GateOperation.Add || operation == GateOperation.Multiply)
            {
                SoundManager.Instance.PlayPositiveGateSound();
            }
            else if (operation == GateOperation.Subtract || operation == GateOperation.Divide)
            {
                SoundManager.Instance.PlayNegativeGateSound();
            }
        }
    }

    public void SetOperation(GateOperation newOperation, int newValue)
    {
        operation = newOperation;
        value = newValue;
        SetupGateDisplay();
    }
}
