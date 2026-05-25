using System.Globalization;
using UnityEngine;

public class PuzzleNumberManager : Singleton<PuzzleNumberManager>
{
    public int CurrentValue { get; private set; }
    public int InitialValue { get; private set; }

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void Apply(OperationType operation, int operand)
    {
        CurrentValue = PuzzleMath.Apply(CurrentValue, operation, operand, InitialValue);
        BroadcastCurrentValue();
    }

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LevelLoaded)
        {
            InitialValue = GameManager.Instance.CurrentLevel.puzzleInitialValue;
            CurrentValue = InitialValue;
            BroadcastCurrentValue();
        }
        else if (n.Type == NotificationType.LevelReset)
        {
            CurrentValue = InitialValue;
            BroadcastCurrentValue();
        }
    }

    private void BroadcastCurrentValue()
    {
        NotificationQueue.SendMessage(new Notification(
            NotificationType.NumberChanged,
            CurrentValue.ToString(CultureInfo.InvariantCulture),
            "PuzzleNumberManager"));
    }
}
