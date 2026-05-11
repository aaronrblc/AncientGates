using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public static class SoFieldWriter
{
    public static void WriteFields(ScriptableObject target, Dictionary<string, string> values, HashSet<string> allowedFields)
    {
        var so = new SerializedObject(target);
        foreach (var kvp in values)
        {
            if (!allowedFields.Contains(kvp.Key)) continue;
            var prop = so.FindProperty(kvp.Key);
            if (prop == null) { Debug.LogWarning($"[CSV] Campo '{kvp.Key}' no encontrado en {target.name}"); continue; }
            try { WriteProperty(prop, kvp.Value, target.name, kvp.Key); }
            catch (Exception e) { Debug.LogWarning($"[CSV] Error en '{kvp.Key}' de {target.name}: {e.Message}"); }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    static void WriteProperty(SerializedProperty prop, string raw, string assetName, string fieldName)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                if (int.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out int i)) prop.intValue = i;
                else Debug.LogWarning($"[CSV] No se puede parsear int '{raw}' en {assetName}.{fieldName}");
                break;
            case SerializedPropertyType.Float:
                if (float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) prop.floatValue = f;
                else Debug.LogWarning($"[CSV] No se puede parsear float '{raw}' en {assetName}.{fieldName}");
                break;
            case SerializedPropertyType.String:
                prop.stringValue = raw;
                break;
            case SerializedPropertyType.Boolean:
                if (bool.TryParse(raw, out bool b)) prop.boolValue = b;
                else prop.boolValue = raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case SerializedPropertyType.Enum:
                for (int idx = 0; idx < prop.enumNames.Length; idx++)
                    if (prop.enumNames[idx].Equals(raw, StringComparison.OrdinalIgnoreCase)) { prop.enumValueIndex = idx; break; }
                break;
            default:
                Debug.LogWarning($"[CSV] Tipo no soportado '{prop.propertyType}' en {assetName}.{fieldName}");
                break;
        }
    }
}
