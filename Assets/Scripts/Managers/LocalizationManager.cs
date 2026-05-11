using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private const string CsvResourcePath = "Translations/translations";
    private const string KeyColumn = "Key";

    private Dictionary<string, string> localizedTexts = new();

    protected override void Awake()
    {
        base.Awake();
        LoadLanguage(LanguageEnum.English);
    }

    public void LoadLanguage(LanguageEnum language)
    {
        string code = language.ToLanguageCode();
        var asset = Resources.Load<TextAsset>(CsvResourcePath);
        if (asset == null) { Debug.LogError("[Localization] translations.csv no encontrado"); return; }

        var rows = CsvParser.ParseFromText(asset.text);
        if (rows.Count == 0 || !rows[0].ContainsKey(code)) { Debug.LogError($"[Localization] Columna '{code}' no encontrada"); return; }

        localizedTexts.Clear();
        foreach (var row in rows)
        {
            if (!row.TryGetValue(KeyColumn, out string key) || string.IsNullOrEmpty(key)) continue;
            if (row.TryGetValue(code, out string value))
                localizedTexts[key] = value;
        }
    }

    public string GetLocalizedText(string key)
    {
        return localizedTexts.TryGetValue(key, out string value) ? value : key;
    }
}
