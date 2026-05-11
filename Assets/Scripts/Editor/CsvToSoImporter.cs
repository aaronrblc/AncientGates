using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CsvToSoImporter
{
    const string DataFolder = "Assets/Resources/Data";

    struct Route
    {
        public string CsvFile;
        public string AssetFolder;
        public Type SoType;
        public HashSet<string> AllowedFields;
        public Action<List<Dictionary<string, string>>> CustomHandler;
    }

    static readonly Route[] Routes =
    {
        new()
        {
            CsvFile = "Levels.csv",
            AssetFolder = "Assets/Resources/SO/Levels",
            SoType = typeof(LevelConfig),
            AllowedFields = new HashSet<string> { "Name", "DisplayNameKey", "ParTime", "ParMoves" }
        },
        // Añadir más routes aquí según el juego necesite
    };

    [MenuItem("Tools/Balance/Import All CSVs")]
    public static void ImportAll()
    {
        int total = 0;
        foreach (var route in Routes)
        {
            string path = Path.Combine(DataFolder, route.CsvFile);
            if (!File.Exists(path)) { Debug.LogWarning($"[CSV] Archivo no encontrado: {path}"); continue; }

            var rows = CsvParser.Parse(path);
            if (rows.Count == 0) { Debug.LogWarning($"[CSV] Sin datos en {route.CsvFile}"); continue; }

            if (route.CustomHandler != null)
                route.CustomHandler(rows);
            else
            {
                int count = ImportStandard(rows, route.AssetFolder, route.SoType, route.AllowedFields);
                total += count;
                Debug.Log($"[CSV] {route.CsvFile}: {count} assets actualizados");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CSV] Import completo. {total} assets actualizados.");
    }

    public static void Import(string csvFileName)
    {
        foreach (var route in Routes)
        {
            if (!route.CsvFile.Equals(csvFileName, StringComparison.OrdinalIgnoreCase)) continue;

            string path = Path.Combine(DataFolder, route.CsvFile);
            if (!File.Exists(path)) { Debug.LogWarning($"[CSV] Archivo no encontrado: {path}"); return; }

            var rows = CsvParser.Parse(path);
            if (rows.Count == 0) { Debug.LogWarning($"[CSV] Sin datos en {route.CsvFile}"); return; }

            if (route.CustomHandler != null) route.CustomHandler(rows);
            else
            {
                int count = ImportStandard(rows, route.AssetFolder, route.SoType, route.AllowedFields);
                Debug.Log($"[CSV] {route.CsvFile}: {count} assets actualizados");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }
        Debug.LogWarning($"[CSV] No hay route configurado para {csvFileName}");
    }

    static int ImportStandard(List<Dictionary<string, string>> rows, string assetFolder, Type soType, HashSet<string> allowedFields)
    {
        int count = 0;
        foreach (var row in rows)
        {
            if (!row.TryGetValue("Name", out string assetName) || string.IsNullOrEmpty(assetName))
            { Debug.LogWarning("[CSV] Fila sin columna 'Name', ignorada"); continue; }

            string assetPath = $"{assetFolder}/{assetName}.asset";
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, soType) as ScriptableObject;
            if (asset == null) { Debug.LogWarning($"[CSV] Asset no encontrado: {assetPath}"); continue; }

            SoFieldWriter.WriteFields(asset, row, allowedFields);
            count++;
        }
        return count;
    }
}
