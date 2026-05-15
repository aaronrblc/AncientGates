using UnityEngine;

[RequireComponent(typeof(NumberModifier))]
public class OperandChildDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] items;

    private void OnValidate() => Refresh();
    private void Awake() => Refresh();

    public void Refresh()
    {
        var modifier = GetComponent<NumberModifier>();
        if (modifier == null || items == null) return;
        int count = modifier.OperandValue;
        for (int i = 0; i < items.Length; i++)
            if (items[i] != null) items[i].SetActive(i < count);
    }
}
