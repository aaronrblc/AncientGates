using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private float sphereRadius = 0.3f;

    private PlayerState state;
    private bool isPaused;
    private InteractableHighlight _currentHighlight;

    public void Initialize(PlayerState playerState) => state = playerState;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable()
    {
        NotificationQueue.Unsubscribe(OnMessage);
        SetHighlight(null);
    }

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused) isPaused = true;
        else if (n.Type == NotificationType.GameResumed) isPaused = false;
    }

    private void Update()
    {
        if (isPaused || state == null) return;

        Camera cam = cameraTransform != null
            ? cameraTransform.GetComponent<Camera>()
            : Camera.main;

        if (cam == null) return;

        Vector3 origin = cam.transform.position - cam.transform.forward * sphereRadius;
        Ray ray = new(origin, cam.transform.forward);
        bool hit = Physics.SphereCast(ray, sphereRadius, out RaycastHit hitInfo, state.InteractRange + sphereRadius, interactableLayers);

        InteractableHighlight highlight = null;
        if (hit)
        {
            var trigger = hitInfo.collider.GetComponentInParent<InteractableTrigger>();
            if (trigger != null && !trigger.IsUsed && trigger.CanInteract())
                highlight = hitInfo.collider.GetComponentInParent<InteractableHighlight>();
        }

        SetHighlight(highlight);

        if (hit && PlayerActionsManager.Instance.InputActions.Player.Interact.WasPressedThisFrame())
        {
            var interactable = hitInfo.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract(gameObject);
                NotificationQueue.SendMessage(new(NotificationType.InteractionPerformed, hitInfo.collider.name, "PlayerInteraction"));
            }
        }
    }

    private void SetHighlight(InteractableHighlight highlight)
    {
        if (highlight == _currentHighlight) return;
        _currentHighlight?.Hide();
        _currentHighlight = highlight;
        _currentHighlight?.Show();
    }
}
