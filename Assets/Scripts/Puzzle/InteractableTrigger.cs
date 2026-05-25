using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private bool singleUse = true;
    public UnityEvent OnInteracted;

    public bool IsUsed { get; private set; }

    public bool CanInteract()
    {
        var condition = GetComponent<IInteractCondition>();
        return condition == null || condition.CanInteract();
    }

    public void OnInteract(GameObject interactor)
    {
        if (IsUsed) return;
        if (!CanInteract()) return;
        if (singleUse) IsUsed = true;
        OnInteracted?.Invoke();
    }

    public void Reset() => IsUsed = false;
}
