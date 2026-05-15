using UnityEngine;
using UnityEngine.Events;

public class InteractableTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private bool singleUse = true;
    public UnityEvent OnInteracted;

    public bool IsUsed { get; private set; }

    public void OnInteract(GameObject interactor)
    {
        if (IsUsed) return;
        if (singleUse) IsUsed = true;
        OnInteracted?.Invoke();
    }

    public void Reset() => IsUsed = false;
}
