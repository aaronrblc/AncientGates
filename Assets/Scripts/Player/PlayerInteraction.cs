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

        Ray ray = new(cam.transform.position, cam.transform.forward);
        bool hit = Physics.SphereCast(ray, sphereRadius, out RaycastHit hitInfo, state.InteractRange, interactableLayers);

        InteractableHighlight highlight = null;
        if (hit)
        {
            var trigger = hitInfo.collider.GetComponent<InteractableTrigger>();
            if (trigger != null && !trigger.IsUsed)
                highlight = hitInfo.collider.GetComponent<InteractableHighlight>();
        }

        SetHighlight(highlight);

        if (hit && PlayerActionsManager.Instance.InputActions.Player.Interact.WasPressedThisFrame())
        {
            if (hitInfo.collider.TryGetComponent<IInteractable>(out var interactable))
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
