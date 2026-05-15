using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

public class NumberReactive : MonoBehaviour
{
    [SerializeField] private ConditionType condition;
    [SerializeField] private int conditionValue;

    [Space]
    public UnityEvent OnConditionMet;
    public UnityEvent OnConditionUnmet;

    private bool _lastResult;
    private bool _initialized;

    private void OnValidate()
    {
        foreach (var label in GetComponentsInChildren<NumberReactiveLabel>())
            label.UpdateLabel();
    }

    private void OnEnable()
    {
        NotificationQueue.Subscribe(OnMessage);
        EvaluateAndFire(PuzzleNumberManager.Instance.CurrentValue, forceFireOnEnable: true);
    }

    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.NumberChanged) return;
        int value = int.Parse(n.Content, CultureInfo.InvariantCulture);
        EvaluateAndFire(value, forceFireOnEnable: false);
    }

    private void EvaluateAndFire(int value, bool forceFireOnEnable)
    {
        bool result = Evaluate(value);
        if (!_initialized || result != _lastResult || forceFireOnEnable)
        {
            _initialized = true;
            _lastResult = result;
            if (result) OnConditionMet?.Invoke();
            else        OnConditionUnmet?.Invoke();
        }
    }

    public string Label => condition switch
    {
        ConditionType.Equals         => $"= {conditionValue}",
        ConditionType.NotEquals      => $"≠ {conditionValue}",
        ConditionType.GreaterThan    => $"> {conditionValue}",
        ConditionType.LessThan       => $"< {conditionValue}",
        ConditionType.GreaterOrEqual => $"≥ {conditionValue}",
        ConditionType.LessOrEqual    => $"≤ {conditionValue}",
        ConditionType.DivisibleBy    => $"% {conditionValue}",
        _                            => "?",
    };

    private bool Evaluate(int value) => condition switch
    {
        ConditionType.Equals         => value == conditionValue,
        ConditionType.NotEquals      => value != conditionValue,
        ConditionType.GreaterThan    => value > conditionValue,
        ConditionType.LessThan       => value < conditionValue,
        ConditionType.GreaterOrEqual => value >= conditionValue,
        ConditionType.LessOrEqual    => value <= conditionValue,
        ConditionType.DivisibleBy    => conditionValue != 0 && value % conditionValue == 0,
        _                            => false,
    };
}
