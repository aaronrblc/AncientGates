using UnityEngine;

public class LevelExitTrigger : MonoBehaviour
{
    private bool _triggered;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnDrawGizmos()
    {
        var col = GetComponent<Collider>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0.4f, 0.2f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(0f, 1f, 0.4f, 0.9f);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else
        {
            Gizmos.DrawSphere(Vector3.zero, 0.5f);
        }
    }

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LevelLoaded) _triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        NotificationQueue.SendMessage(new(NotificationType.LevelCompleted, "", "LevelExitTrigger"));
    }
}
