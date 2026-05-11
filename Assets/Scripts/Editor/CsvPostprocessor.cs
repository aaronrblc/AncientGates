using System.IO;
using UnityEditor;

public class CsvPostprocessor : AssetPostprocessor
{
    const string DataFolder = "Assets/Resources/Data/";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var path in importedAssets)
        {
            if (!path.StartsWith(DataFolder) || !path.EndsWith(".csv")) continue;
            string fileName = Path.GetFileName(path);
            UnityEngine.Debug.Log($"[CSV] Cambio detectado en {fileName}, auto-importando...");
            CsvToSoImporter.Import(fileName);
        }
    }
}
