using UnityEngine;

// Coloca este componente en un GameObject con Collider (isTrigger = true).
// Cuando el jugador entra, emite LevelCompleted.
// Patrón a seguir para todos los triggers: detectar → emitir notificación → NO llamar a managers directamente.
public class LevelExitTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LevelLoaded)
            triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        LevelManager.Instance.CompleteLevel();
    }
}
