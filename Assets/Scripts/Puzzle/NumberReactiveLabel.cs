using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class NumberReactiveLabel : MonoBehaviour
{
    [SerializeField] private NumberReactive reactive;

    private void OnValidate()
    {
        if (reactive == null) reactive = GetComponentInParent<NumberReactive>();
        UpdateLabel();
    }

    private void Awake()
    {
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        var label = GetComponent<TMP_Text>();
        if (label == null || reactive == null) return;
        label.text = reactive.Label;
    }
}
