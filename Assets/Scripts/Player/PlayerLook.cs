using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private bool invertY = false;

    private float pitch;
    private bool isPaused;

    private void Start() => Cursor.lockState = CursorLockMode.Locked;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused)
        {
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (n.Type == NotificationType.GameResumed)
        {
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        Vector2 delta = PlayerActionsManager.Instance.InputActions.Player.Look.ReadValue<Vector2>() * sensitivity;

        transform.Rotate(Vector3.up * delta.x);

        float pitchDelta = invertY ? delta.y : -delta.y;
        pitch = Mathf.Clamp(pitch + pitchDelta, -89f, 89f);
        if (cameraAnchor != null)
            cameraAnchor.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
