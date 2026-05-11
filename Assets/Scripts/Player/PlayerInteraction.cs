using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private PlayerState state;
    private bool isPaused;

    public void Initialize(PlayerState playerState) => state = playerState;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused) isPaused = true;
        else if (n.Type == NotificationType.GameResumed) isPaused = false;
    }

    private void Update()
    {
        if (isPaused || state == null) return;

        if (!PlayerActionsManager.Instance.InputActions.Player.Interact.WasPressedThisFrame()) return;

        Camera cam = cameraTransform != null
            ? cameraTransform.GetComponent<Camera>()
            : Camera.main;

        if (cam == null) return;

        Ray ray = new(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, state.InteractRange))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnInteract(gameObject);
                NotificationQueue.SendMessage(new(NotificationType.InteractionPerformed, hit.collider.name, "PlayerInteraction"));
            }
        }
    }
}
