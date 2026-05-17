using UnityEngine;

public class PermanentDeactivator : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    [SerializeField] private bool allowDeactivation = true;
    [SerializeField] private GameObject resetTrigger;

    private bool _deactivated;

    private void Awake()
    {
        if (!allowDeactivation && resetTrigger != null)
            resetTrigger.SetActive(false);
    }

    public void Deactivate()
    {
        if (!allowDeactivation) return;
        if (_deactivated) return;
        _deactivated = true;
        foreach (var c in componentsToDisable)
            if (c != null) c.enabled = false;
    }
}
