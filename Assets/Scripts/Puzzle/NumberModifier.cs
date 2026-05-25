using UnityEngine;

public class NumberModifier : MonoBehaviour, IInteractCondition
{
    [SerializeField] private OperationType operation;

    public OperationType Operation => operation;

    void OnValidate()
    {
        GetComponent<OperationMaterialSwitcher>()?.Apply();
        GetComponent<OperandChildDisplay>()?.Refresh();
        GetComponentInChildren<NumberModifierLabel>()?.UpdateLabel();
    }
    [SerializeField] private int operandValue = 1;
    public int OperandValue => operandValue;

    public string Label => operation switch
    {
        OperationType.Add      => $"{operandValue}",
        OperationType.Subtract => $"{operandValue}",
        OperationType.Multiply => $"{operandValue}",
        OperationType.Divide   => $"{operandValue}",
        OperationType.Reset    => "↺",
        OperationType.Set      => $"={operandValue}",
        _                      => "?",
    };

    public bool CanInteract()
    {
        if (operation != OperationType.Divide) return true;
        int current = PuzzleNumberManager.Instance.CurrentValue;
        return operandValue != 0 && current % operandValue == 0;
    }

    public void Apply() => PuzzleNumberManager.Instance.Apply(operation, operandValue);
}
