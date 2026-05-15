using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class NumberModifierLabel : MonoBehaviour
{
    [SerializeField] private NumberModifier modifier;

    private void OnValidate()
    {
        if (modifier == null) modifier = GetComponentInParent<NumberModifier>();
        UpdateLabel();
    }

    private void Awake() => UpdateLabel();

    private void UpdateLabel()
    {
        var label = GetComponent<TMP_Text>();
        if (label == null || modifier == null) return;
        label.text = modifier.Label;
    }
}
