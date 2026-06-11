using UnityEngine;

public class LightingController : MonoBehaviour
{
    [SerializeField] string startRoomId;

    string _currentRoomId;

    void Start() => SetRoom(startRoomId);

    void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.RoomEntered)
            SetRoom(n.Content);
    }

    void SetRoom(string roomId)
    {
        if (roomId == _currentRoomId) return;
        _currentRoomId = roomId;

        foreach (Transform room in transform)
        {
            bool active = room.name == roomId;
            foreach (var light in room.GetComponentsInChildren<Light>())
                light.enabled = active;
        }
    }
}
