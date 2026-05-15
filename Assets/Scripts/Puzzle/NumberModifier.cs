using UnityEngine;

public class NumberModifier : MonoBehaviour
{
    [SerializeField] private OperationType operation;

    public OperationType Operation => operation;

    void OnValidate()
    {
        GetComponent<OperationMaterialSwitcher>()?.Apply();
        GetComponent<OperandChildDisplay>()?.Refresh();
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

    public void Apply() => PuzzleNumberManager.Instance.Apply(operation, operandValue);
}
