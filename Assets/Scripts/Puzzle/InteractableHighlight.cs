using UnityEngine;

public class InteractableHighlight : MonoBehaviour
{
    [SerializeField] private Light highlightLight;

    public void Show() { if (highlightLight) highlightLight.enabled = true; }
    public void Hide() { if (highlightLight) highlightLight.enabled = false; }

    private void Awake() => Hide();
}
