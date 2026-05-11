using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public RunState RunState { get; private set; }

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void StartLevel(string levelId)
    {
        RunState = new RunState { CurrentLevelId = levelId };
    }

    private void Update()
    {
        if (AppManager.Instance.IsTheGamePaused || RunState == null) return;
        RunState.TimeElapsed += Time.deltaTime;
    }

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            case NotificationType.LevelCompleted:
                // TODO: guardar stats del nivel completado
                break;
            case NotificationType.MovePerformed:
                if (RunState != null) RunState.MovesUsed++;
                break;
            case NotificationType.HintUsed:
                if (RunState != null) RunState.HintsUsed++;
                break;
        }
    }
}
