using UnityEngine;

public class RoomChangeTrigger : MonoBehaviour
{
    [SerializeField] string roomId;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        NotificationQueue.SendMessage(new(NotificationType.RoomEntered, roomId, "RoomChangeTrigger"));
    }
}
