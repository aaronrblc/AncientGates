using UnityEngine;

public class PermanentDeactivator : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] componentsToDisable;

    private bool _deactivated;

    public void Deactivate()
    {
        if (_deactivated) return;
        _deactivated = true;
        foreach (var c in componentsToDisable)
            if (c != null) c.enabled = false;
    }
}
