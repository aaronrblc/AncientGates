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
        // Si ya fue inicializado (re-enable mid-game), sincronizar estado actual
        if (_initialized)
        {
            bool result = Evaluate(PuzzleNumberManager.Instance.CurrentValue);
            if (result != _lastResult)
            {
                _lastResult = result;
                if (result) OnConditionMet?.Invoke();
                else        OnConditionUnmet?.Invoke();
            }
        }
        // Si !_initialized: esperar NumberChanged — CurrentValue puede ser 0 antes de LevelLoaded
    }

    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.NumberChanged) return;
        int value = int.Parse(n.Content, CultureInfo.InvariantCulture);
        bool result = Evaluate(value);
        if (!_initialized)
        {
            _initialized = true;
            _lastResult = result;
            if (result) OnConditionMet?.Invoke();
            // Si no está met, la puerta ya está en estado base
        }
        else if (result != _lastResult)
        {
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

    private bool Evaluate(int value) => PuzzleMath.Evaluate(value, condition, conditionValue);
}
