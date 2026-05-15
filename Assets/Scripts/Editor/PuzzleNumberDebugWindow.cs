using UnityEditor;
using UnityEngine;

public class PuzzleNumberDebugWindow : EditorWindow
{
    private OperationType _operation = OperationType.Add;
    private int _operand = 1;

    [MenuItem("Tools/Puzzle/Number Debugger")]
    private static void Open() => GetWindow<PuzzleNumberDebugWindow>("Number Debugger");

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Solo disponible en Play Mode.", MessageType.Info);
            return;
        }

        var manager = PuzzleNumberManager.Instance;
        if (manager == null)
        {
            EditorGUILayout.HelpBox("PuzzleNumberManager no encontrado en escena.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Valor actual", manager.CurrentValue.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Valor inicial", manager.InitialValue.ToString());

        EditorGUILayout.Space(8);
        _operation = (OperationType)EditorGUILayout.EnumPopup("Operación", _operation);

        using (new EditorGUI.DisabledScope(_operation == OperationType.Reset))
            _operand = EditorGUILayout.IntField("Operando", _operand);

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Aplicar"))
            manager.Apply(_operation, _operand);
    }

    private void OnInspectorUpdate() => Repaint();
}
