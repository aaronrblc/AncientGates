public static class PuzzleMath
{
    public static int Apply(int current, OperationType op, int operand, int initialValue) => op switch
    {
        OperationType.Add      => current + operand,
        OperationType.Subtract => current - operand,
        OperationType.Multiply => current * operand,
        OperationType.Divide   => current / operand,
        OperationType.Reset    => initialValue,
        OperationType.Set      => operand,
        _                      => current,
    };

    public static bool Evaluate(int value, ConditionType cond, int condValue) => cond switch
    {
        ConditionType.Equals         => value == condValue,
        ConditionType.NotEquals      => value != condValue,
        ConditionType.GreaterThan    => value > condValue,
        ConditionType.LessThan       => value < condValue,
        ConditionType.GreaterOrEqual => value >= condValue,
        ConditionType.LessOrEqual    => value <= condValue,
        ConditionType.DivisibleBy    => condValue != 0 && value % condValue == 0,
        _                            => false,
    };
}
