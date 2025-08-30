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
    public GameObject operationEffect; // Particle effect when used
    
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
        // Determine operation symbol and color
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
        
        // Create or update the operation text
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
            
            // Add floating animation to the text
            operationText.transform.DOMoveY(operationText.transform.position.y + 0.2f, 1.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        // Color the gate parts
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
        // Subtle breathing animation for the gate
        if (leftGate != null && rightGate != null)
        {
            Vector3 leftOriginalPos = leftGate.localPosition;
            Vector3 rightOriginalPos = rightGate.localPosition;
            
            // Breathing effect - gates move slightly closer and farther
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
        
        // Only respond to the player - check for Player tag first
        if (!other.CompareTag("Player"))
        {
            return; // Ignore soldiers and other objects
        }
        
        // Find the army manager from the player
        ArmyManager armyManager = other.GetComponent<ArmyManager>();
        if (armyManager == null)
        {
            armyManager = other.GetComponentInParent<ArmyManager>();
        }
        
        // If still not found, search for it in the scene
        if (armyManager == null)
        {
            armyManager = FindFirstObjectByType<ArmyManager>();
        }
        
        if (armyManager != null)
        {
            ApplyOperation(armyManager);
        }
        else
        {
            Debug.LogWarning("Player triggered math gate but no ArmyManager found!");
        }
    }
    
    private void ApplyOperation(ArmyManager armyManager)
    {
        hasBeenUsed = true;
        
        // Kill all tweens on this object
        if (leftGate != null) leftGate.DOKill();
        if (rightGate != null) rightGate.DOKill();
        if (operationText != null) operationText.transform.DOKill();
        
        // Calculate current army size (excluding player)
        int currentSoldiers = armyManager.GetArmySize() - 1; // -1 for player
        int newSoldierCount = currentSoldiers;
        
        // Apply the mathematical operation
        switch (operation)
        {
            case GateOperation.Add:
                newSoldierCount = currentSoldiers + value;
                break;
                
            case GateOperation.Subtract:
                newSoldierCount = Mathf.Max(0, currentSoldiers - value); // Don't go below 0
                break;
                
            case GateOperation.Multiply:
                newSoldierCount = currentSoldiers * value;
                break;
                
            case GateOperation.Divide:
                if (value != 0)
                    newSoldierCount = currentSoldiers / value;
                else
                    newSoldierCount = currentSoldiers; // Avoid division by zero
                break;
        }
        
        // Calculate the difference
        int soldierDifference = newSoldierCount - currentSoldiers;
        
        Debug.Log($"Gate operation: {currentSoldiers} {operationSymbol} {value} = {newSoldierCount} (difference: {soldierDifference})");
        
        // Apply the change
        if (soldierDifference > 0)
        {
            // Add soldiers
            armyManager.AddSoldiers(soldierDifference);
        }
        else if (soldierDifference < 0)
        {
            // Remove soldiers without particles (clean mathematical removal)
            armyManager.RemoveRandomSoldiers(-soldierDifference, false);
        }
        
        // Play appropriate sound effect
        PlayGateSound();
        
        // Animate the gate effect
        AnimateGateEffect();
        
        // Play operation effect
        if (operationEffect != null)
        {
            Instantiate(operationEffect, transform.position, transform.rotation);
        }
    }
    
    void AnimateGateEffect()
    {
        Sequence gateSequence = DOTween.Sequence();
        
        // Gate closing animation
        if (leftGate != null && rightGate != null)
        {
            Vector3 leftPos = leftGate.localPosition;
            Vector3 rightPos = rightGate.localPosition;
            
            gateSequence.Append(leftGate.DOLocalMoveX(0f, gateAnimationDuration * 0.5f).SetEase(gateEase));
            gateSequence.Join(rightGate.DOLocalMoveX(0f, gateAnimationDuration * 0.5f).SetEase(gateEase));
            
            // Gate opening animation
            gateSequence.Append(leftGate.DOLocalMoveX(leftPos.x, gateAnimationDuration * 0.5f).SetEase(gateEase));
            gateSequence.Join(rightGate.DOLocalMoveX(rightPos.x, gateAnimationDuration * 0.5f).SetEase(gateEase));
        }
        
        // Text animation - bounce and flash
        if (operationText != null)
        {
            gateSequence.Insert(0, operationText.transform.DOScale(textBounceScale, gateAnimationDuration * 0.3f).SetEase(gateEase));
            gateSequence.Insert(gateAnimationDuration * 0.2f, operationText.transform.DOScale(1f, gateAnimationDuration * 0.3f));
            
            // Flash effect
            gateSequence.Insert(0, operationText.DOColor(Color.white, gateAnimationDuration * 0.2f));
            gateSequence.Insert(gateAnimationDuration * 0.2f, operationText.DOColor(gateColor, gateAnimationDuration * 0.3f));
        }
        
        // Destroy or deactivate after animation
        if (destroyOnUse)
        {
            gateSequence.OnComplete(() => Destroy(gameObject));
        }
    }
    
    void PlayGateSound()
    {
        // Check if SoundManager exists
        if (SoundManager.Instance != null)
        {
            // Play positive sound for addition and multiplication
            if (operation == GateOperation.Add || operation == GateOperation.Multiply)
            {
                SoundManager.Instance.PlayPositiveGateSound();
            }
            // Play negative sound for subtraction and division
            else if (operation == GateOperation.Subtract || operation == GateOperation.Divide)
            {
                SoundManager.Instance.PlayNegativeGateSound();
            }
        }
    }
    
    // Method to manually set operation and value
    public void SetOperation(GateOperation newOperation, int newValue)
    {
        operation = newOperation;
        value = newValue;
        SetupGateDisplay();
    }
}
