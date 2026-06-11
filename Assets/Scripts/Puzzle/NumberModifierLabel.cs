using TMPro;
using UnityEngine;

public class NumberModifierLabel : MonoBehaviour
{
    [SerializeField] private NumberModifier modifier;
    [SerializeField] private TMP_Text numberLabel;
    [SerializeField] private TMP_Text operationLabel;

    private void OnValidate()
    {
        if (modifier == null) modifier = GetComponent<NumberModifier>();
        UpdateLabel();
    }

    private void Awake() => UpdateLabel();

    public void UpdateLabel()
    {
        if (modifier == null) return;
        if (numberLabel != null) numberLabel.text = modifier.Label;
        if (operationLabel != null) operationLabel.text = modifier.OperationLabel;
    }
}
