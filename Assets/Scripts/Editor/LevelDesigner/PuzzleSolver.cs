using System.Collections.Generic;
using System.Text;

public static class PuzzleSolver
{
    public const int MaxModifiers = 7;

    // ── Resultado de cadena multi-puerta ─────────────────────────────────────
    public class ChainResult
    {
        public List<ChainZone> zones = new();

        public string Label()
        {
            var sb = new StringBuilder();
            foreach (var zone in zones)
            {
                foreach (var (op, operand) in zone.sequence)
                { sb.Append(ModLabel(op, operand)); sb.Append(' '); }
                sb.Append('-');
                sb.Append(CondLabel(zone.cond, zone.condValue));
                sb.Append("→ ");
            }
            sb.Append("E");
            return sb.ToString().Trim();
        }
    }

    public class ChainZone
    {
        public List<(OperationType op, int operand)> sequence = new();
        public ConditionType cond;
        public int condValue;
    }

    // doors: lista ordenada de (condición, valor, override activo, valor override)
    public static List<ChainResult> SolveChain(
        int levelInitial,
        List<(OperationType op, int operand)> allMods,
        List<(ConditionType cond, int condValue, bool overrides, int overrideVal)> doors,
        int maxResults = 3)
    {
        var results = new List<ChainResult>();
        if (doors.Count == 0 || allMods.Count > MaxModifiers) return results;
        Recurse(levelInitial, levelInitial, new List<(OperationType op, int operand)>(allMods),
                doors, 0, new List<ChainZone>(), results, maxResults);
        return results;
    }

    private static void Recurse(
        int cur, int init,
        List<(OperationType op, int operand)> avail,
        List<(ConditionType cond, int condValue, bool overrides, int overrideVal)> doors,
        int di, List<ChainZone> chain,
        List<ChainResult> results, int max)
    {
        if (results.Count >= max) return;

        if (di == doors.Count)
        {
            var copy = new List<ChainZone>();
            foreach (var z in chain)
                copy.Add(new ChainZone
                {
                    sequence = new List<(OperationType op, int operand)>(z.sequence),
                    cond = z.cond, condValue = z.condValue
                });
            results.Add(new ChainResult { zones = copy });
            return;
        }

        var door = doors[di];
        int n = avail.Count;

        for (int mask = 0; mask < (1 << n) && results.Count < max; mask++)
        {
            var sub = new List<(OperationType op, int operand)>();
            var rem = new List<(OperationType op, int operand)>();
            for (int i = 0; i < n; i++)
                ((mask >> i & 1) == 1 ? sub : rem).Add(avail[i]);

            bool comm = sub.TrueForAll(m =>
                m.op == OperationType.Add || m.op == OperationType.Subtract);

            if (comm || sub.Count <= 1)
            {
                TryChainStep(cur, init, sub, rem, door, doors, di, chain, results, max);
            }
            else
            {
                foreach (var perm in Permutations(sub))
                {
                    if (results.Count >= max) break;
                    TryChainStep(cur, init, perm, rem, door, doors, di, chain, results, max);
                }
            }
        }
    }

    private static void TryChainStep(
        int cur, int init,
        List<(OperationType op, int operand)> perm,
        List<(OperationType op, int operand)> rem,
        (ConditionType cond, int condValue, bool overrides, int overrideVal) door,
        List<(ConditionType cond, int condValue, bool overrides, int overrideVal)> doors,
        int di, List<ChainZone> chain,
        List<ChainResult> results, int max)
    {
        int val = Sim(cur, init, perm);
        if (!PuzzleMath.Evaluate(val, door.cond, door.condValue)) return;
        int next = door.overrides ? door.overrideVal : val;
        chain.Add(new ChainZone { sequence = perm, cond = door.cond, condValue = door.condValue });
        Recurse(next, init, rem, doors, di + 1, chain, results, max);
        chain.RemoveAt(chain.Count - 1);
    }

    private static int Sim(int start, int init, List<(OperationType op, int operand)> seq)
    {
        int val = start;
        foreach (var (op, operand) in seq)
            val = PuzzleMath.Apply(val, op, operand, init);
        return val;
    }

    private static IEnumerable<List<(OperationType op, int operand)>> Permutations(List<(OperationType op, int operand)> items)
    {
        if (items.Count <= 1) { yield return new List<(OperationType op, int operand)>(items); yield break; }
        for (int i = 0; i < items.Count; i++)
        {
            var rest = new List<(OperationType op, int operand)>(items);
            var first = rest[i];
            rest.RemoveAt(i);
            foreach (var perm in Permutations(rest))
            { perm.Insert(0, first); yield return perm; }
        }
    }

    internal static string ModLabel(OperationType op, int operand) => op switch
    {
        OperationType.Add      => $"+{operand}",
        OperationType.Subtract => $"-{operand}",
        OperationType.Multiply => $"×{operand}",
        OperationType.Divide   => $"÷{operand}",
        OperationType.Reset    => "↺",
        OperationType.Set      => $"={operand}",
        _                      => "?",
    };

    internal static string CondLabel(ConditionType cond, int val) => cond switch
    {
        ConditionType.Equals         => $"={val}",
        ConditionType.NotEquals      => $"≠{val}",
        ConditionType.GreaterThan    => $">{val}",
        ConditionType.LessThan       => $"<{val}",
        ConditionType.GreaterOrEqual => $"≥{val}",
        ConditionType.LessOrEqual    => $"≤{val}",
        ConditionType.DivisibleBy    => $"%{val}",
        _                            => "?",
    };
}
