using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Tipos de datos
// ─────────────────────────────────────────────────────────────────────────────

// Un estado posible de un NPC (qué operación aplica en ese estado)
[Serializable]
public struct EstadoNPC
{
    public OperationType Operacion;
    public int Valor;

    // Division solo válida si el resultado es entero exacto
    public bool Aplicar(int actual, int inicial, out int resultado)
    {
        resultado = actual;
        switch (Operacion)
        {
            case OperationType.Add:      resultado = actual + Valor; return true;
            case OperationType.Subtract: resultado = actual - Valor; return true;
            case OperationType.Multiply: resultado = actual * Valor; return true;
            case OperationType.Divide:
                if (Valor != 0 && actual % Valor == 0) { resultado = actual / Valor; return true; }
                return false;
            case OperationType.Reset:    resultado = inicial; return true;
            case OperationType.Set:      resultado = Valor;   return true;
        }
        return false;
    }

    public override string ToString()
    {
        return Operacion switch
        {
            OperationType.Add      => $"+{Valor}",
            OperationType.Subtract => $"-{Valor}",
            OperationType.Multiply => $"×{Valor}",
            OperationType.Divide   => $"÷{Valor}",
            OperationType.Reset    => "↺",
            OperationType.Set      => $"={Valor}",
            _                      => "?",
        };
    }
}

// Un modificador (NPC) con uno o más estados posibles; se consume al usarlo
[Serializable]
public class ModificadorNPC
{
    public string       Nombre  = "NPC";
    public EstadoNPC[]  Estados = Array.Empty<EstadoNPC>();
    public int NumEstados => Estados?.Length ?? 0;
}

public class ResultadoBFS
{
    public bool EsSoluble;
    public List<(string Nombre, EstadoNPC Estado)> Camino; // null = no soluble

    public string DescripcionCamino()
    {
        if (!EsSoluble) return "Sin solución con los modificadores disponibles.";
        if (Camino == null || Camino.Count == 0) return "Número inicial = objetivo.";
        var sb = new StringBuilder("Solución: ");
        foreach (var paso in Camino) sb.Append($"[{paso.Nombre} {paso.Estado}]  ");
        return sb.ToString().TrimEnd();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Calculadora estática
// ─────────────────────────────────────────────────────────────────────────────

public static class KhemetPathCalculator
{
    // ── Combinatoria ─────────────────────────────────────────────────────────
    //
    // f([]) = 1
    // f([s0,...,sk]) = 1 + Σ_i ( si * f(mods sin índice i) )
    //
    // ContarCaminos = f - 1  (excluye la secuencia vacía)
    //
    // Esto cuenta TODAS las secuencias ordenadas de cualquier subconjunto
    // no vacío de modificadores, multiplicando por las elecciones de estado.
    // Incluye caminos parciales (el jugador no tiene que usar todos).

    private static readonly Dictionary<string, long> _memo = new();

    /// <summary>Número de caminos (secuencias no vacías) con los modificadores dados.</summary>
    public static long ContarCaminos(int[] numEstados)
    {
        if (numEstados == null || numEstados.Length == 0) return 0;
        return ContarF(numEstados) - 1;
    }

    /// <summary>
    /// Peor caso de caminos restantes tras la primera jugada.
    /// Retorna el máximo de ContarCaminos(restantes) para cada posible primera elección de NPC.
    /// </summary>
    public static long PeorCasoTrasPrimera(int[] numEstados)
    {
        if (numEstados == null || numEstados.Length == 0) return 0;
        long peor = 0;
        for (int i = 0; i < numEstados.Length; i++)
        {
            long c = ContarCaminos(EliminarEn(numEstados, i));
            if (c > peor) peor = c;
        }
        return peor;
    }

    /// <summary>Caminos restantes para cada posible primera elección de NPC.</summary>
    public static long[] CaminosPorPrimera(int[] numEstados)
    {
        if (numEstados == null) return Array.Empty<long>();
        var resultado = new long[numEstados.Length];
        for (int i = 0; i < numEstados.Length; i++)
            resultado[i] = ContarCaminos(EliminarEn(numEstados, i));
        return resultado;
    }

    private static long ContarF(int[] cuentas)
    {
        if (cuentas == null || cuentas.Length == 0) return 1;
        string clave = ClaveCanonica(cuentas);
        if (_memo.TryGetValue(clave, out long cached)) return cached;

        long total = 1;
        for (int i = 0; i < cuentas.Length; i++)
            total += cuentas[i] * ContarF(EliminarEn(cuentas, i));

        _memo[clave] = total;
        return total;
    }

    // Clave reproducible para memo: el mismo multiset da siempre la misma clave
    private static string ClaveCanonica(int[] arr)
    {
        int[] ord = (int[])arr.Clone();
        Array.Sort(ord);
        return string.Join(",", ord);
    }

    private static int[] EliminarEn(int[] arr, int idx)
    {
        int[] res = new int[arr.Length - 1];
        int j = 0;
        for (int i = 0; i < arr.Length; i++)
            if (i != idx) res[j++] = arr[i];
        return res;
    }

    // ── BFS ──────────────────────────────────────────────────────────────────

    private const int LimiteValorAbsoluto = 1_000_000;

    /// <summary>
    /// Comprueba si los modificadores dados pueden transformar valorInicial en objetivo.
    /// Devuelve el camino más corto (en número de NPCs usados).
    /// </summary>
    public static ResultadoBFS Resolver(int valorInicial, int objetivo, ModificadorNPC[] mods)
    {
        if (valorInicial == objetivo)
            return new ResultadoBFS { EsSoluble = true, Camino = new List<(string, EstadoNPC)>() };

        int n = mods?.Length ?? 0;
        if (n == 0) return new ResultadoBFS { EsSoluble = false };
        if (n > 20)  throw new ArgumentException("BFS soporta máximo 20 modificadores.");

        var cola      = new Queue<NodoBFS>();
        var visitados = new HashSet<(int valor, int usados)>();

        cola.Enqueue(new NodoBFS(valorInicial, 0, new List<(string, EstadoNPC)>()));
        visitados.Add((valorInicial, 0));

        while (cola.Count > 0)
        {
            var nodo = cola.Dequeue();

            for (int i = 0; i < n; i++)
            {
                if ((nodo.Usados & (1 << i)) != 0) continue;
                int usadosSig = nodo.Usados | (1 << i);

                foreach (var estado in mods[i].Estados)
                {
                    if (!estado.Aplicar(nodo.Valor, valorInicial, out int sigValor)) continue;
                    if (Math.Abs(sigValor) > LimiteValorAbsoluto) continue;

                    var nuevoCamino = new List<(string, EstadoNPC)>(nodo.Camino) { (mods[i].Nombre, estado) };

                    if (sigValor == objetivo)
                        return new ResultadoBFS { EsSoluble = true, Camino = nuevoCamino };

                    var clave = (sigValor, usadosSig);
                    if (!visitados.Contains(clave))
                    {
                        visitados.Add(clave);
                        cola.Enqueue(new NodoBFS(sigValor, usadosSig, nuevoCamino));
                    }
                }
            }
        }

        return new ResultadoBFS { EsSoluble = false };
    }

    private readonly struct NodoBFS
    {
        public readonly int Valor;
        public readonly int Usados;
        public readonly List<(string, EstadoNPC)> Camino;

        public NodoBFS(int valor, int usados, List<(string, EstadoNPC)> camino)
        {
            Valor  = valor;
            Usados = usados;
            Camino = camino;
        }
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Ventana de editor
// ─────────────────────────────────────────────────────────────────────────────

public class KhemetPathCalculatorWindow : EditorWindow
{
    private int                _valorInicial = 6;
    private int                _objetivo     = 3;
    private readonly List<DatosMod> _mods    = new();

    // Resultados
    private long           _peorCaso;
    private long[]         _caminosPorMod;
    private ResultadoBFS   _resultado;

    private Vector2 _scroll;

    [MenuItem("Tools/Khemet/Calculadora de Caminos")]
    public static void Abrir() => GetWindow<KhemetPathCalculatorWindow>("Khemet · Caminos");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Calculadora de Caminos de Sala", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ─ Sala ─────────────────────────────────────────────────────────────
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Sala", EditorStyles.miniLabel);
            using var chk = new EditorGUI.ChangeCheckScope();
            _valorInicial = EditorGUILayout.IntField("Número inicial", _valorInicial);
            _objetivo     = EditorGUILayout.IntField("Objetivo",       _objetivo);
            if (chk.changed) _resultado = null;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("NPCs / Modificadores", EditorStyles.boldLabel);

        // ─ Lista de modificadores ────────────────────────────────────────────
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MaxHeight(320));
        for (int i = 0; i < _mods.Count; i++)
        {
            if (!DibujarMod(i)) i--;
            EditorGUILayout.Space(2);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("+ Añadir NPC"))
        {
            _mods.Add(new DatosMod { Nombre = $"NPC {_mods.Count + 1}" });
            _resultado = null;
        }

        EditorGUILayout.Space(8);

        var prevBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
        if (GUILayout.Button("Calcular caminos", GUILayout.Height(28)))
            Calcular();
        GUI.backgroundColor = prevBg;

        if (_resultado != null)
            DibujarResultados();
    }

    // Devuelve false si el NPC fue eliminado (para corregir el índice del bucle)
    private bool DibujarMod(int idx)
    {
        using var box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
        var mod = _mods[idx];

        // Cabecera
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"NPC {idx + 1}", EditorStyles.boldLabel, GUILayout.Width(44));
        mod.Nombre = EditorGUILayout.TextField(mod.Nombre);
        if (GUILayout.Button("✕", GUILayout.Width(24)))
        {
            _mods.RemoveAt(idx);
            _resultado = null;
            EditorGUILayout.EndHorizontal();
            return false;
        }
        EditorGUILayout.EndHorizontal();

        // Estados
        EditorGUI.indentLevel++;
        for (int s = 0; s < mod.Estados.Count; s++)
        {
            EditorGUILayout.BeginHorizontal();
            var st = mod.Estados[s];
            EditorGUILayout.LabelField($"Estado {s + 1}", GUILayout.Width(58));
            st.Operacion  = (OperationType)EditorGUILayout.EnumPopup(st.Operacion, GUILayout.Width(130));
            bool tieneValor = st.Operacion != OperationType.Reset;
            GUI.enabled     = tieneValor;
            st.Valor        = EditorGUILayout.IntField(tieneValor ? st.Valor : 0, GUILayout.Width(50));
            GUI.enabled     = true;
            mod.Estados[s]  = st;
            if (GUILayout.Button("−", GUILayout.Width(22)))
            {
                mod.Estados.RemoveAt(s--);
                _resultado = null;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;

        if (GUILayout.Button("+ Estado", GUILayout.Width(80)))
            mod.Estados.Add(new EstadoNPC { Operacion = OperationType.Add, Valor = 1 });

        return true;
    }

    private void DibujarResultados()
    {
        EditorGUILayout.Space(8);
        using var box = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Resultados", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        // Solubilidad
        var prevColor = GUI.color;
        GUI.color = _resultado.EsSoluble ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.4f, 0.4f);
        EditorGUILayout.LabelField(
            _resultado.EsSoluble ? "✓  SOLUBLE" : "✗  NO SOLUBLE",
            EditorStyles.boldLabel);
        GUI.color = prevColor;

        if (_resultado.EsSoluble && _resultado.Camino?.Count > 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(_resultado.DescripcionCamino(), EditorStyles.wordWrappedLabel);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        // Caminos
        int n = _mods.Count;
        EditorGUILayout.LabelField(
            n == 0 ? "Sin NPCs configurados." :
            n == 1 ? "Solo 1 NPC: no hay caminos restantes tras la primera jugada." :
            $"Caminos restantes tras 1ª elección — peor caso: {_peorCaso}",
            EditorStyles.boldLabel);

        if (_caminosPorMod != null && _caminosPorMod.Length == n && n > 1)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < _caminosPorMod.Length; i++)
                EditorGUILayout.LabelField(
                    $"Si primero → {_mods[i].Nombre}: {_caminosPorMod[i]} caminos restantes");
            EditorGUI.indentLevel--;
        }
    }

    private void Calcular()
    {
        var mods = new ModificadorNPC[_mods.Count];
        for (int i = 0; i < _mods.Count; i++)
            mods[i] = new ModificadorNPC { Nombre = _mods[i].Nombre, Estados = _mods[i].Estados.ToArray() };

        int[] numEstados = Array.ConvertAll(mods, m => m.NumEstados);
        _peorCaso      = KhemetPathCalculator.PeorCasoTrasPrimera(numEstados);
        _caminosPorMod = KhemetPathCalculator.CaminosPorPrimera(numEstados);
        _resultado     = KhemetPathCalculator.Resolver(_valorInicial, _objetivo, mods);
    }

    private class DatosMod
    {
        public string           Nombre  = "NPC";
        public List<EstadoNPC>  Estados = new() { new EstadoNPC { Operacion = OperationType.Add, Valor = 1 } };
    }
}
