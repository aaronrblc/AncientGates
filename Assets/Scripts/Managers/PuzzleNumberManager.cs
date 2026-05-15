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
        switch (operation)
        {
            case OperationType.Add:      CurrentValue += operand; break;
            case OperationType.Subtract: CurrentValue -= operand; break;
            case OperationType.Multiply: CurrentValue *= operand; break;
            case OperationType.Divide:   CurrentValue /= operand; break;
            case OperationType.Reset:    CurrentValue = InitialValue; break;
            case OperationType.Set:      CurrentValue = operand; break;
        }
        BroadcastCurrentValue();
    }

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.LevelLoaded) return;
        InitialValue = GameManager.Instance.CurrentLevel.puzzleInitialValue;
        CurrentValue = InitialValue;
        BroadcastCurrentValue();
    }

    private void BroadcastCurrentValue()
    {
        NotificationQueue.SendMessage(new Notification(
            NotificationType.NumberChanged,
            CurrentValue.ToString(CultureInfo.InvariantCulture),
            "PuzzleNumberManager"));
    }
}
