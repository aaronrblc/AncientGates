using UnityEditor;
using UnityEngine;

public class DebugToolsWindow : EditorWindow
{
    private OperationType _operation = OperationType.Add;
    private int _operand = 1;

    [MenuItem("Tools/Debug Tools")]
    private static void Open() => GetWindow<DebugToolsWindow>("Debug Tools");

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
        EditorGUILayout.LabelField("— Número —", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Valor actual", manager.CurrentValue.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Valor inicial", manager.InitialValue.ToString());

        EditorGUILayout.Space(8);
        _operation = (OperationType)EditorGUILayout.EnumPopup("Operación", _operation);

        using (new EditorGUI.DisabledScope(_operation == OperationType.Reset))
            _operand = EditorGUILayout.IntField("Operando", _operand);

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Aplicar"))
            manager.Apply(_operation, _operand);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("— Nivel —", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (GUILayout.Button("Reset nivel (sin mover player)"))
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                LevelController.OverridePosition = player.transform.position;
                LevelController.OverrideRotation = player.transform.rotation;
            }
            GameManager.Instance.ResetLevel();
        }
    }

    private void OnInspectorUpdate() => Repaint();
}
