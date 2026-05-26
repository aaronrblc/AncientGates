using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoMapa", menuName = "SO/LevelDesign")]
public class LevelDesign : ScriptableObject
{
    public int width  = 5;
    public int height = 5;
    public int initialValue = 1;
    public Vector2Int startCell = Vector2Int.zero;
    public Vector2Int exitCell  = new(4, 4);

    public List<CellData> cells = new();
    public List<EdgeData> edges = new();

    public CellData GetCell(Vector2Int pos) =>
        cells.Find(c => c.position == pos);

    public EdgeData GetEdge(Vector2Int cellA, EdgeDirection dir) =>
        edges.Find(e => e.cellA == cellA && e.dir == dir);

    public void SetCell(CellData data)
    {
        int idx = cells.FindIndex(c => c.position == data.position);
        if (idx >= 0) cells[idx] = data;
        else          cells.Add(data);
    }

    public void RemoveCell(Vector2Int pos) =>
        cells.RemoveAll(c => c.position == pos);

    public void SetEdge(EdgeData data)
    {
        int idx = edges.FindIndex(e => e.cellA == data.cellA && e.dir == data.dir);
        if (idx >= 0) edges[idx] = data;
        else          edges.Add(data);
    }

    public void RemoveEdge(Vector2Int cellA, EdgeDirection dir) =>
        edges.RemoveAll(e => e.cellA == cellA && e.dir == dir);

    public bool InBounds(Vector2Int pos) =>
        pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;

    // Normaliza a arista canonical (solo N y E), devuelve false si la arista
    // es un borde exterior del grid (sin celda vecina válida del otro lado).
    public static bool NormalizeEdge(int gridWidth, int gridHeight,
        Vector2Int cell, EdgeDirection dir,
        out Vector2Int outCell, out EdgeDirection outDir)
    {
        switch (dir)
        {
            case EdgeDirection.South:
                outCell = new Vector2Int(cell.x, cell.y + 1);
                outDir  = EdgeDirection.North;
                return outCell.y < gridHeight;
            case EdgeDirection.West:
                outCell = new Vector2Int(cell.x - 1, cell.y);
                outDir  = EdgeDirection.East;
                return outCell.x >= 0;
            default:
                outCell = cell;
                outDir  = dir;
                return true;
        }
    }
}

public enum CellKind { Modifier, Reset }

public enum EdgeKind { Wall, Door }

public enum EdgeDirection { North, East, South, West }

[Serializable]
public class CellData
{
    public Vector2Int  position;
    public CellKind    kind;
    public OperationType op      = OperationType.Add;
    public int         operand   = 1;
    public int         groupId   = 0;
    public string      notes     = "";
}

[Serializable]
public class EdgeData
{
    public Vector2Int  cellA;
    public EdgeDirection dir;       // solo North o East en datos; South/West se normalizan al guardar
    public EdgeKind    kind        = EdgeKind.Wall;
    public ConditionType cond      = ConditionType.Equals;
    public int         condValue   = 0;
    public bool        overridesValue  = false;
    public int         overrideValue   = 0;
}
