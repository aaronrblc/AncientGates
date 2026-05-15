using System;
using UnityEngine;

public class OperationMaterialSwitcher : MonoBehaviour
{
    [Serializable]
    public struct Entry
    {
        public OperationType operation;
        public Material material;
    }

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Entry[] entries;

    void Awake() => Apply();

    void OnValidate() => Apply();

    public void Apply()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        var modifier = GetComponent<NumberModifier>();
        if (modifier == null) return;

        foreach (var entry in entries)
        {
            if (entry.operation == modifier.Operation)
            {
                targetRenderer.sharedMaterial = entry.material;
                return;
            }
        }
    }
}
