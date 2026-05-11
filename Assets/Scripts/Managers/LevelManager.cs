using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public LevelConfig CurrentLevel { get; private set; }

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void LoadLevel(LevelConfig config)
    {
        CurrentLevel = config;
        GameManager.Instance.StartLevel(config.Name);
        NotificationQueue.SendMessage(new(NotificationType.LevelLoaded, config.Name, "LevelManager"));
        // TODO: cargar la escena del nivel si es necesario
    }

    public void CompleteLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelCompleted, CurrentLevel?.Name ?? "", "LevelManager"));
    }

    public void FailLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelFailed, CurrentLevel?.Name ?? "", "LevelManager"));
    }

    private void OnMessage(Notification n)
    {
        // TODO: reaccionar a eventos de nivel
    }
}
