using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public LevelConfig CurrentLevel { get; private set; }
    public RunState RunState { get; private set; }

    private string _currentLevelSceneName;

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void Update()
    {
        if (AppManager.Instance.IsTheGamePaused || RunState == null) return;
        RunState.TimeElapsed += Time.deltaTime;
    }

    public void LoadLevel(LevelConfig config)
    {
        CurrentLevel = config;
        _currentLevelSceneName = SceneUtils.GetLoadedLevelSceneName();
        RunState = new RunState { CurrentLevelId = config.Name };
        NotificationQueue.SendMessage(new(NotificationType.LevelLoaded, config.Name, "GameManager"));
    }

    public void CompleteLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelCompleted, CurrentLevel?.Name ?? "", "GameManager"));
    }

    public void FailLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelFailed, CurrentLevel?.Name ?? "", "GameManager"));
    }

    public void ResetLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelReset, CurrentLevel?.Name ?? "", "GameManager"));
        SceneLoader.ReloadSceneAdditive(_currentLevelSceneName);
    }

    private void OnMessage(Notification n)
    {
        // TODO: reaccionar a LevelCompleted (guardar stats, etc.)
    }
}
