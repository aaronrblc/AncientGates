using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelDesignerWindow : EditorWindow
{
    // ── Estado ──────────────────────────────────────────────────────────────
    private LevelDesign _design;
    private bool        _dirty;

    private enum Tool { Select, Wall, Door, Modifier, Reset, StartExit }
    private Tool _tool = Tool.Wall;

    // Selección
    private bool        _selectionIsEdge;
    private Vector2Int  _selectedCell;
    private Vector2Int  _selectedEdgeCell;
    private EdgeDirection _selectedEdgeDir;

    // Solver
    private List<PuzzleSolver.ChainResult> _chainResults;
    private List<(Vector2Int cellA, EdgeDirection dir)> _solverDoorOrder = new();
    private bool _solverFoldout = true;

    // Layout
    private const float CellPx    = 48f;
    private const float EdgeThick  = 6f;
    private const float EdgeHit    = 12f;  // zona de clic en aristas
    private Vector2     _canvasScroll;

    // Colores
    private static readonly Color ColFloor     = new(0.22f, 0.22f, 0.22f);
    private static readonly Color ColWall      = new(0.85f, 0.75f, 0.55f);
    private static readonly Color ColDoor      = new(0.35f, 0.65f, 0.90f);
    private static readonly Color ColModifier  = new(0.40f, 0.80f, 0.45f);
    private static readonly Color ColReset     = new(0.80f, 0.55f, 0.35f);
    private static readonly Color ColStart     = new(0.35f, 0.90f, 0.45f);
    private static readonly Color ColExit      = new(0.90f, 0.35f, 0.35f);
    private static readonly Color ColGrid      = new(0.35f, 0.35f, 0.35f);
    private static readonly Color ColSelection = new(1f, 0.85f, 0f);

    // ── Apertura ────────────────────────────────────────────────────────────
    [MenuItem("Tools/Level Designer")]
    public static void Open() => GetWindow<LevelDesignerWindow>("Level Designer");

    // ── GUI ─────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawToolbar();
        if (_design == null) { EditorGUILayout.HelpBox("Crea o abre un LevelDesign para empezar.", MessageType.Info); return; }
        EditorGUILayout.BeginHorizontal();
        DrawCanvas();
        DrawInspectorPanel();
        EditorGUILayout.EndHorizontal();
        DrawSolverPanel();
    }

    // ── Toolbar ─────────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("Nuevo", EditorStyles.toolbarButton, GUILayout.Width(50)))
            CreateNew();
        if (GUILayout.Button("Abrir", EditorStyles.toolbarButton, GUILayout.Width(50)))
            OpenPicker();
        GUI.enabled = _design != null && _dirty;
        if (GUILayout.Button("Guardar", EditorStyles.toolbarButton, GUILayout.Width(60)))
            Save();
        GUI.enabled = true;

        GUILayout.Space(8);

        if (_design != null)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("W:", GUILayout.Width(16));
            int w  = EditorGUILayout.IntField(_design.width,        GUILayout.Width(32));
            GUILayout.Label("H:", GUILayout.Width(16));
            int h  = EditorGUILayout.IntField(_design.height,       GUILayout.Width(32));
            GUILayout.Label("Inicial:", GUILayout.Width(48));
            int iv = EditorGUILayout.IntField(_design.initialValue, GUILayout.Width(32));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_design, "Cambiar tamaño");
                _design.width        = Mathf.Max(1, w);
                _design.height       = Mathf.Max(1, h);
                _design.initialValue = iv;
                ClampSpecialCells();
                PruneCells();
                _dirty = true;
                RefreshSolver();
            }
        }

        GUILayout.FlexibleSpace();

        _tool = (Tool)GUILayout.Toolbar((int)_tool,
            new[] { "Select", "Pared", "Puerta", "Mod", "Reset", "S/E" },
            EditorStyles.toolbarButton, GUILayout.Width(300));

        EditorGUILayout.EndHorizontal();
    }

    // ── Canvas ──────────────────────────────────────────────────────────────
    private void DrawCanvas()
    {
        float canvasW = _design.width  * CellPx + EdgeThick + 8;
        float canvasH = _design.height * CellPx + EdgeThick + 8;

        _canvasScroll = EditorGUILayout.BeginScrollView(
            _canvasScroll,
            GUILayout.Width(Mathf.Min(canvasW + 20, position.width - 220)),
            GUILayout.ExpandHeight(true));

        Rect canvasRect = GUILayoutUtility.GetRect(canvasW, canvasH);

        if (Event.current.type == EventType.Repaint)
            DrawGrid(canvasRect);

        HandleCanvasInput(canvasRect);

        EditorGUILayout.EndScrollView();
    }

    private void DrawGrid(Rect origin)
    {
        for (int y = 0; y < _design.height; y++)
        {
            for (int x = 0; x < _design.width; x++)
            {
                var pos = new Vector2Int(x, y);
                Rect cell = CellRect(origin, x, y);

                // Suelo
                EditorGUI.DrawRect(cell, ColFloor);

                // Contenido
                var cd = _design.GetCell(pos);
                if (pos == _design.startCell) DrawCenteredLabel(cell, "S", ColStart);
                else if (pos == _design.exitCell) DrawCenteredLabel(cell, "E", ColExit);
                else if (cd != null)
                {
                    Color c = cd.kind == CellKind.Modifier ? ColModifier : ColReset;
                    EditorGUI.DrawRect(cell.Shrink(6), c);
                    string lbl = cd.kind == CellKind.Reset ? "↺" : OpLabelShort(cd.op, cd.operand);
                    DrawCenteredLabel(cell, lbl, Color.black);
                }

                // Borde de selección
                if (!_selectionIsEdge && _selectedCell == pos)
                    DrawBorder(cell, ColSelection, 2f);
            }
        }

        // Línea de cuadrícula
        for (int y = 0; y <= _design.height; y++)
        {
            float py = origin.y + y * CellPx;
            EditorGUI.DrawRect(new Rect(origin.x, py, _design.width * CellPx, 1), ColGrid);
        }
        for (int x = 0; x <= _design.width; x++)
        {
            float px = origin.x + x * CellPx;
            EditorGUI.DrawRect(new Rect(px, origin.y, 1, _design.height * CellPx), ColGrid);
        }

        // Aristas (paredes / puertas)
        foreach (var edge in _design.edges)
            DrawEdge(origin, edge);

        // Arista seleccionada
        if (_selectionIsEdge)
        {
            var selEdge = _design.GetEdge(_selectedEdgeCell, _selectedEdgeDir);
            Rect er = EdgeRect(origin, _selectedEdgeCell, _selectedEdgeDir);
            EditorGUI.DrawRect(er.Expand(2), ColSelection);
            if (selEdge != null) DrawEdgeRect(er, selEdge.kind);
        }
    }

    private void DrawEdge(Rect origin, EdgeData edge)
    {
        Rect er = EdgeRect(origin, edge.cellA, edge.dir);
        DrawEdgeRect(er, edge.kind);

        if (edge.kind == EdgeKind.Door)
        {
            string lbl = CondLabel(edge.cond, edge.condValue);
            if (edge.overridesValue) lbl += $"→{edge.overrideValue}";

            // Etiqueta encima del extremo superior de la arista con fondo oscuro
            const float lh = 13f;
            const float lw = 44f;
            Rect labelRect = edge.dir == EdgeDirection.East || edge.dir == EdgeDirection.West
                ? new Rect(er.center.x - lw / 2f, er.y - lh - 2f, lw, lh)   // arista vertical
                : new Rect(er.center.x - lw / 2f, er.y - lh - 2f, lw, lh);  // arista horizontal
            EditorGUI.DrawRect(labelRect.Expand(1f), new Color(0f, 0f, 0f, 0.75f));
            DrawCenteredLabel(labelRect, lbl, ColDoor, 9);
        }
    }

    private void DrawEdgeRect(Rect er, EdgeKind kind)
    {
        Color c = kind == EdgeKind.Wall ? ColWall : ColDoor;
        EditorGUI.DrawRect(er, c);
    }

    // ── Input en canvas ─────────────────────────────────────────────────────
    private void HandleCanvasInput(Rect origin)
    {
        Event e = Event.current;
        bool isContextClick = e.type == EventType.ContextClick;
        if (e.type != EventType.MouseDown && e.type != EventType.MouseUp && !isContextClick) return;
        if (e.button != 0 && e.button != 1) return;

        Vector2 mp = e.mousePosition;
        if (!origin.Contains(mp)) return;

        bool isDelete = e.button == 1 || isContextClick;
        bool isDown   = e.type == EventType.MouseDown || isContextClick;

        // ¿clic cerca de una arista?
        if (TryPickEdge(origin, mp, out Vector2Int eCell, out EdgeDirection eDir))
        {
            if (isDelete) { if (isDown) DeleteEdge(eCell, eDir); }
            else HandleEdgeClick(eCell, eDir, isDown);
        }
        else if (TryPickCell(origin, mp, out Vector2Int cell))
        {
            if (isDelete) { if (isDown) DeleteCell(cell); }
            else HandleCellClick(cell, isDown);
        }

        e.Use();
        Repaint();
    }

    private bool TryPickEdge(Rect origin, Vector2 mp, out Vector2Int cell, out EdgeDirection dir)
    {
        // Comprueba las 4 aristas de todas las celdas; devuelve la primera dentro del threshold
        for (int y = 0; y < _design.height; y++)
        {
            for (int x = 0; x < _design.width; x++)
            {
                foreach (EdgeDirection d in System.Enum.GetValues(typeof(EdgeDirection)))
                {
                    // Solo aristas interiores o del borde (todas son válidas para pintar)
                    Rect er = EdgeRect(origin, new Vector2Int(x, y), d).Expand(EdgeHit);
                    if (er.Contains(mp))
                    {
                        // Normalizar
                        LevelDesign.NormalizeEdge(_design.width, _design.height,
                            new Vector2Int(x, y), d, out cell, out dir);
                        return true;
                    }
                }
            }
        }
        cell = default; dir = default; return false;
    }

    private bool TryPickCell(Rect origin, Vector2 mp, out Vector2Int cell)
    {
        for (int y = 0; y < _design.height; y++)
            for (int x = 0; x < _design.width; x++)
            {
                if (CellRect(origin, x, y).Contains(mp))
                { cell = new Vector2Int(x, y); return true; }
            }
        cell = default; return false;
    }

    private void HandleEdgeClick(Vector2Int cellA, EdgeDirection dir, bool isDown)
    {
        if (!isDown) return;
        _selectionIsEdge   = true;
        _selectedEdgeCell  = cellA;
        _selectedEdgeDir   = dir;

        if (_tool == Tool.Wall)
        {
            Undo.RecordObject(_design, "Toggle pared");
            var existing = _design.GetEdge(cellA, dir);
            if (existing == null)
                _design.SetEdge(new EdgeData { cellA = cellA, dir = dir, kind = EdgeKind.Wall });
            else
                _design.RemoveEdge(cellA, dir);
            _dirty = true;
        }
        else if (_tool == Tool.Door)
        {
            Undo.RecordObject(_design, "Colocar puerta");
            var existing = _design.GetEdge(cellA, dir);
            if (existing == null || existing.kind == EdgeKind.Wall)
            {
                _design.SetEdge(new EdgeData
                {
                    cellA = cellA, dir = dir,
                    kind = EdgeKind.Door,
                    cond = ConditionType.Equals, condValue = 0
                });
            }
            else
            {
                _design.RemoveEdge(cellA, dir);
            }
            _dirty = true;
            RefreshSolver();
        }
    }

    private void HandleCellClick(Vector2Int cell, bool isDown)
    {
        if (!isDown) return;
        _selectionIsEdge = false;
        _selectedCell    = cell;

        if (_tool == Tool.Modifier)
        {
            Undo.RecordObject(_design, "Colocar modificador");
            if (_design.GetCell(cell) != null)
                _design.RemoveCell(cell);
            else
                _design.SetCell(new CellData { position = cell, kind = CellKind.Modifier });
            _dirty = true;
            RefreshSolver();
        }
        else if (_tool == Tool.Reset)
        {
            Undo.RecordObject(_design, "Colocar reset");
            if (_design.GetCell(cell) != null)
                _design.RemoveCell(cell);
            else
                _design.SetCell(new CellData { position = cell, kind = CellKind.Reset, op = OperationType.Reset });
            _dirty = true;
            RefreshSolver();
        }
        else if (_tool == Tool.StartExit)
        {
            Undo.RecordObject(_design, "Marcar S/E");
            if (_design.startCell == cell)      _design.startCell = new Vector2Int(-1, -1);
            else if (_design.exitCell == cell)  _design.exitCell  = new Vector2Int(-1, -1);
            else if (!_design.InBounds(_design.startCell)) _design.startCell = cell;
            else if (!_design.InBounds(_design.exitCell))  _design.exitCell  = cell;
            else _design.startCell = cell;
            _dirty = true;
        }
    }

    private void DeleteEdge(Vector2Int cellA, EdgeDirection dir)
    {
        if (_design.GetEdge(cellA, dir) == null) return;
        Undo.RecordObject(_design, "Borrar arista");
        _design.RemoveEdge(cellA, dir);
        _dirty = true;
        RefreshSolver();
        Repaint();
    }

    private void DeleteCell(Vector2Int cell)
    {
        bool changed = false;
        if (_design.GetCell(cell) != null)
        {
            Undo.RecordObject(_design, "Borrar celda");
            _design.RemoveCell(cell);
            changed = true;
        }
        if (_design.startCell == cell) { _design.startCell = new Vector2Int(-1, -1); changed = true; }
        if (_design.exitCell  == cell) { _design.exitCell  = new Vector2Int(-1, -1); changed = true; }
        if (changed) { _dirty = true; RefreshSolver(); Repaint(); }
    }

    // ── Panel inspector ──────────────────────────────────────────────────────
    private void DrawInspectorPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));
        EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);

        if (_selectionIsEdge)
            DrawEdgeInspector();
        else
            DrawCellInspector();

        EditorGUILayout.EndVertical();
    }

    private void DrawCellInspector()
    {
        if (!_design.InBounds(_selectedCell)) { EditorGUILayout.LabelField("—"); return; }

        EditorGUILayout.LabelField($"Celda ({_selectedCell.x},{_selectedCell.y})");

        if (_selectedCell == _design.startCell) { EditorGUILayout.LabelField("Tipo: Start"); return; }
        if (_selectedCell == _design.exitCell)  { EditorGUILayout.LabelField("Tipo: Exit");  return; }

        var cd = _design.GetCell(_selectedCell);
        if (cd == null) { EditorGUILayout.LabelField("Tipo: Vacía"); return; }

        EditorGUI.BeginChangeCheck();
        string kindStr = cd.kind == CellKind.Reset ? "Reset" : "Modificador";
        EditorGUILayout.LabelField($"Tipo: {kindStr}");

        if (cd.kind == CellKind.Modifier)
        {
            cd.op      = (OperationType)EditorGUILayout.EnumPopup("Op", cd.op);
            cd.operand = EditorGUILayout.IntField("Operando", cd.operand);
        }
        cd.notes = EditorGUILayout.TextField("Notas", cd.notes);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_design, "Editar celda");
            _design.SetCell(cd);
            _dirty = true;
            RefreshSolver();
        }
    }

    private void DrawEdgeInspector()
    {
        var ed = _design.GetEdge(_selectedEdgeCell, _selectedEdgeDir);
        if (ed == null)
        {
            EditorGUILayout.LabelField($"Arista ({_selectedEdgeCell.x},{_selectedEdgeCell.y}) {_selectedEdgeDir}");
            EditorGUILayout.LabelField("Tipo: Abierta");
            return;
        }

        EditorGUILayout.LabelField($"Arista ({ed.cellA.x},{ed.cellA.y}) {ed.dir}");
        EditorGUILayout.LabelField($"Tipo: {ed.kind}");

        if (ed.kind == EdgeKind.Door)
        {
            EditorGUI.BeginChangeCheck();
            ed.cond       = (ConditionType)EditorGUILayout.EnumPopup("Condición", ed.cond);
            ed.condValue  = EditorGUILayout.IntField("Valor", ed.condValue);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Override al cruzar", EditorStyles.miniBoldLabel);
            ed.overridesValue = EditorGUILayout.Toggle("Activo", ed.overridesValue);
            if (ed.overridesValue)
                ed.overrideValue = EditorGUILayout.IntField("Fijar a", ed.overrideValue);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_design, "Editar puerta");
                _design.SetEdge(ed);
                _dirty = true;
                RefreshSolver();
            }
        }
    }

    // ── Panel solver ─────────────────────────────────────────────────────────
    private void DrawSolverPanel()
    {
        _solverFoldout = EditorGUILayout.Foldout(_solverFoldout, "Solver de combinaciones", true, EditorStyles.foldoutHeader);
        if (!_solverFoldout) return;

        EditorGUILayout.BeginVertical("box");

        // Orden de puertas + botón recalcular
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Puertas en orden:", EditorStyles.miniBoldLabel);
        if (GUILayout.Button("Recalcular", GUILayout.Width(80)))
            RefreshSolver();
        EditorGUILayout.EndHorizontal();

        if (_solverDoorOrder.Count == 0)
        {
            EditorGUILayout.LabelField("  No hay puertas en el mapa.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        for (int i = 0; i < _solverDoorOrder.Count; i++)
        {
            var k  = _solverDoorOrder[i];
            var e  = _design.GetEdge(k.cellA, k.dir);
            string condStr = e != null ? CondLabel(e.cond, e.condValue) : "?";
            string overStr = (e != null && e.overridesValue) ? $" →{e.overrideValue}" : "";
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"  {i + 1}. ({k.cellA.x},{k.cellA.y}) {k.dir}  {condStr}{overStr}", EditorStyles.miniLabel);
            GUI.enabled = i > 0;
            if (GUILayout.Button("↑", GUILayout.Width(22)))
            { (_solverDoorOrder[i], _solverDoorOrder[i - 1]) = (_solverDoorOrder[i - 1], _solverDoorOrder[i]); RefreshSolver(); }
            GUI.enabled = i < _solverDoorOrder.Count - 1;
            if (GUILayout.Button("↓", GUILayout.Width(22)))
            { (_solverDoorOrder[i], _solverDoorOrder[i + 1]) = (_solverDoorOrder[i + 1], _solverDoorOrder[i]); RefreshSolver(); }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);

        if (_design.cells.Count > PuzzleSolver.MaxModifiers)
        {
            EditorGUILayout.HelpBox($"Demasiados modificadores ({_design.cells.Count} > {PuzzleSolver.MaxModifiers}).", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        if (_chainResults == null) RefreshSolver();

        if (_chainResults != null)
        {
            Color prev = GUI.color;
            if (_chainResults.Count == 0)
            {
                GUI.color = new Color(1f, 0.4f, 0.4f);
                EditorGUILayout.LabelField("Sin solución válida hacia E", EditorStyles.boldLabel);
            }
            else
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField($"{_chainResults.Count} solución{(_chainResults.Count == 1 ? "" : "es")} hacia E{(_chainResults.Count == 3 ? " (mostrando hasta 3)" : "")}:", EditorStyles.boldLabel);
                GUI.color = prev;
                foreach (var r in _chainResults)
                    EditorGUILayout.LabelField($"  {r.Label()}", EditorStyles.miniLabel);
            }
            GUI.color = prev;
        }

        EditorGUILayout.EndVertical();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void RefreshSolver()
    {
        if (_design == null) { _chainResults = null; return; }

        // Sincronizar lista de puertas: quitar borradas, añadir nuevas al final
        _solverDoorOrder.RemoveAll(k => _design.GetEdge(k.cellA, k.dir)?.kind != EdgeKind.Door);
        foreach (var e in _design.edges)
        {
            if (e.kind != EdgeKind.Door) continue;
            var key = (e.cellA, e.dir);
            if (!_solverDoorOrder.Contains(key)) _solverDoorOrder.Add(key);
        }

        if (_solverDoorOrder.Count == 0 || _design.cells.Count > PuzzleSolver.MaxModifiers)
        { _chainResults = null; return; }

        var mods = new List<(OperationType, int)>();
        foreach (var cd in _design.cells)
            mods.Add((cd.op, cd.kind == CellKind.Reset ? 0 : cd.operand));

        var doors = new List<(ConditionType cond, int condValue, bool overrides, int overrideVal)>();
        foreach (var k in _solverDoorOrder)
        {
            var e = _design.GetEdge(k.cellA, k.dir);
            if (e != null) doors.Add((e.cond, e.condValue, e.overridesValue, e.overrideValue));
        }

        _chainResults = PuzzleSolver.SolveChain(_design.initialValue, mods, doors, 3);
    }

    private void CreateNew()
    {
        if (!ConfirmUnsaved()) return;
        string path = EditorUtility.SaveFilePanelInProject(
            "Nuevo LevelDesign", "NuevoMapa", "asset",
            "Guardar nuevo esquema de nivel", "Assets/LevelDesigns");
        if (string.IsNullOrEmpty(path)) return;

        EnsureFolder("Assets/LevelDesigns");
        var ld = CreateInstance<LevelDesign>();
        AssetDatabase.CreateAsset(ld, path);
        AssetDatabase.SaveAssets();
        _design           = ld;
        _dirty            = false;
        _chainResults     = null;
        _solverDoorOrder.Clear();
    }

    private void OpenPicker()
    {
        if (!ConfirmUnsaved()) return;
        var guids = AssetDatabase.FindAssets("t:LevelDesign");
        if (guids.Length == 0) { EditorUtility.DisplayDialog("Level Designer", "No hay LevelDesign assets en el proyecto.", "OK"); return; }

        var menu = new GenericMenu();
        foreach (var guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(p);
            menu.AddItem(new GUIContent(name), false, () =>
            {
                _design           = AssetDatabase.LoadAssetAtPath<LevelDesign>(p);
                _dirty            = false;
                _chainResults     = null;
                _solverDoorOrder.Clear();
                _selectionIsEdge  = false;
                Repaint();
            });
        }
        menu.ShowAsContext();
    }

    private void Save()
    {
        if (_design == null) return;
        EditorUtility.SetDirty(_design);
        AssetDatabase.SaveAssetIfDirty(_design);
        _dirty = false;
    }

    private bool ConfirmUnsaved()
    {
        if (!_dirty) return true;
        return EditorUtility.DisplayDialog("Cambios sin guardar",
            "¿Descartar los cambios actuales?", "Descartar", "Cancelar");
    }

    private void ClampSpecialCells()
    {
        if (!_design.InBounds(_design.startCell)) _design.startCell = Vector2Int.zero;
        if (!_design.InBounds(_design.exitCell))
            _design.exitCell = new Vector2Int(_design.width - 1, _design.height - 1);
    }

    private void PruneCells()
    {
        _design.cells.RemoveAll(c => !_design.InBounds(c.position));
        _design.edges.RemoveAll(e => !_design.InBounds(e.cellA));
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(
                System.IO.Path.GetDirectoryName(path).Replace('\\', '/'),
                System.IO.Path.GetFileName(path));
    }

    // ── Geometría ────────────────────────────────────────────────────────────
    private static Rect CellRect(Rect origin, int x, int y) =>
        new(origin.x + x * CellPx + 1,
            origin.y + y * CellPx + 1,
            CellPx - 2, CellPx - 2);

    private static Rect EdgeRect(Rect origin, Vector2Int cell, EdgeDirection dir)
    {
        float cx = origin.x + cell.x * CellPx;
        float cy = origin.y + cell.y * CellPx;
        return dir switch
        {
            EdgeDirection.North => new Rect(cx + EdgeThick, cy,              CellPx - EdgeThick * 2, EdgeThick),
            EdgeDirection.South => new Rect(cx + EdgeThick, cy + CellPx,     CellPx - EdgeThick * 2, EdgeThick),
            EdgeDirection.East  => new Rect(cx + CellPx,   cy + EdgeThick,   EdgeThick, CellPx - EdgeThick * 2),
            _                   => new Rect(cx,             cy + EdgeThick,   EdgeThick, CellPx - EdgeThick * 2),
        };
    }

    private static void DrawCenteredLabel(Rect r, string text, Color col, int fontSize = 11)
    {
        var style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize  = fontSize,
            normal    = { textColor = col }
        };
        GUI.Label(r, text, style);
    }

    private static void DrawBorder(Rect r, Color col, float thickness)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), col);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), col);
        EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), col);
        EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), col);
    }

    private static string OpLabelShort(OperationType op, int operand) => op switch
    {
        OperationType.Add      => $"+{operand}",
        OperationType.Subtract => $"-{operand}",
        OperationType.Multiply => $"×{operand}",
        OperationType.Divide   => $"÷{operand}",
        OperationType.Set      => $"={operand}",
        OperationType.Reset    => "↺",
        _                      => "?",
    };

    private static string CondLabel(ConditionType cond, int val) => cond switch
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

// ── Extensiones de Rect ─────────────────────────────────────────────────────
internal static class RectExtensions
{
    public static Rect Shrink(this Rect r, float amount) =>
        new(r.x + amount, r.y + amount, r.width - amount * 2, r.height - amount * 2);

    public static Rect Expand(this Rect r, float amount) =>
        new(r.x - amount, r.y - amount, r.width + amount * 2, r.height + amount * 2);
}
