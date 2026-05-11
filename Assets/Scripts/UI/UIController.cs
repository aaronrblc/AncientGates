using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private ScreenFader screenFader;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            case NotificationType.GamePaused: pauseMenu?.Show(); break;
            case NotificationType.GameResumed: pauseMenu?.Hide(); break;
            // TODO: reaccionar a LevelCompleted, LevelFailed, etc.
        }
    }
}
