# Bootstrap: Nuevo Proyecto Unity 6 — FPS Puzzle 3D

## Objetivo

Crea el esqueleto de arquitectura para un juego de puzzles 3D en primera persona usando **Unity 6 (URP, new Input System, TextMeshPro)**. La arquitectura está basada en un proyecto de referencia y mantiene estos patrones:

- **Singleton<T>** para managers globales
- **NotificationQueue** (pub/sub estático) para comunicación entre sistemas
- **ScriptableObjects** en `Resources/SO/` para configuración
- **Composición de componentes** en el Player (un orquestador + subsistemas hermanos)
- **Pipeline CSV→SO** para editar valores de balance en tablas externas
- **Carga de escenas aditiva** con mapa de dependencias declarativo

**No generes enemigos, armas, XP, upgrades roguelite ni nada de combat.** Es un juego de puzzles: el jugador camina en primera persona por escenarios 3D, interactúa con objetos y resuelve puzzles.

Stack requerido: Unity 6 (6000.3.x), URP, New Input System (com.unity.inputsystem), TextMeshPro, Cinemachine (opcional para mejoras de cámara).

---

## Estructura de Carpetas

Crea esta estructura bajo `Assets/`:

```
Assets/
├── Actions/                        ← PlayerActions.inputactions + clase generada
├── Animations/
├── Audio/
├── Materials/
├── Models/                         ← modelos 3D (sustituto de Sprites/)
├── Prefabs/
├── Resources/
│   ├── Data/                       ← CSVs de balance (Levels.csv, Interactables.csv...)
│   ├── Prefabs/                    ← prefabs cargados en runtime
│   ├── SO/
│   │   ├── AppConfig.asset         ← creado desde el Editor tras compilar
│   │   ├── GameConfig.asset
│   │   ├── Levels/                 ← un LevelConfig.asset por puzzle
│   │   └── Player/                 ← PlayerConfig.asset
│   └── Translations/
│       └── translations.csv        ← tabla de localización Key,en,es
├── Scenes/                         ← BootstrapScene BaseScene BaseMenuScene StartScene LevelScene
├── Scripts/
│   ├── Bootstrap/                  ← GameBootstrap.cs
│   ├── Camera/                     ← FpsCameraController.cs
│   ├── Editor/                     ← scripts del Editor (CSV pipeline, SceneLoader menu)
│   ├── Environment/                ← Door.cs PressurePlate.cs etc. (triggers de mundo)
│   ├── Interactables/              ← Pickable.cs, Button.cs, Lever.cs...
│   ├── Managers/                   ← AppManager RunManager LevelManager AudioManager...
│   ├── Models/
│   │   ├── Classes/                ← Singleton Notification SceneInfo SceneUtils PlayerState RunState
│   │   ├── Enums/                  ← NotificationEnums SceneEnums GameEnums
│   │   └── Interfaces/             ← IInteractable.cs
│   ├── Player/                     ← PlayerController PlayerMovement PlayerLook PlayerInteraction
│   ├── ScriptableObjects/          ← definición de clases SO (AppConfig LevelConfig...)
│   ├── Static/                     ← NotificationQueues SceneLoader DebugFlags
│   ├── Triggers/                   ← LevelExitTrigger.cs (template)
│   ├── UI/                         ← UIController PauseMenu StartMenu LocalizedText ScreenFader
│   └── Utils/                      ← CsvParser LanguageUtils GeneralUtils Constants/
├── Settings/                       ← URP assets
└── docs/
    └── ReleaseNotes.md
```

---

## Checklist de Creación (ejecutar en orden para evitar errores de compilación)

1. Enums: `NotificationEnums.cs`, `SceneEnums.cs`, `GameEnums.cs`
2. Modelos: `Notification.cs`, `SceneInfo.cs`, `PlayerState.cs`, `RunState.cs`, `IInteractable.cs`
3. `Singleton.cs`
4. `NotificationQueues.cs`, `SceneLoader.cs`, `SceneUtils.cs`
5. ScriptableObjects: `AppConfig.cs`, `GameConfig.cs`, `LevelConfig.cs`, `PlayerConfig.cs`
6. Managers: `AppManager` → `RunManager` → `LevelManager` → `AudioManager` → `ConfigManager` → `LocalizationManager` → `PlayerActionsManager`
7. Player: `PlayerController` → `PlayerMovement` → `PlayerLook` → `PlayerInteraction`
8. Camera: `FpsCameraController.cs`
9. Editor: `SoFieldWriter`, `CsvToSoImporter`, `CsvPostprocessor`, `SceneLoaderMenu`
10. Triggers: `LevelExitTrigger.cs`
11. Utils: `CsvParser.cs`, `LanguageUtils.cs`
12. Localización: `LocalizationManager.cs`, `LocalizedText.cs`
13. UI: `UIController.cs` (esqueleto), `PauseMenu.cs` (esqueleto), `ScreenFader.cs` (esqueleto)
14. Bootstrap: `GameBootstrap.cs`
15. **En el Editor de Unity** (tras compilar sin errores):
    - Crear las 5 escenas en `Assets/Scenes/`
    - Crear los assets SO: `AppConfig`, `GameConfig`, `PlayerConfig` en `Resources/SO/`
    - Crear `translations.csv` en `Resources/Translations/`
    - Crear `Levels.csv` en `Resources/Data/`
    - Configurar Build Settings (BootstrapScene = índice 0)
    - Configurar Physics Layers: Player, Ground, Interactable, IgnorePlayer
    - Configurar `.inputactions` asset (ver sección Input System)

---

## 1. Enums

### `Scripts/Models/Enums/NotificationEnums.cs`
```csharp
public enum NotificationType
{
    // General
    GameLoaded,
    GamePaused,
    GameResumed,
    GameOver,
    LanguageChanged,

    // Nivel
    LevelLoaded,
    LevelCompleted,
    LevelFailed,
    CheckpointReached,

    // Jugador
    PlayerDied,
    MovePerformed,
    HintRequested,
    HintUsed,

    // Interacción
    InteractionPerformed,
    ItemPicked,
    DoorOpened,
    PuzzleStateChanged,
}

public enum QueueType { Notification, Player }
```

### `Scripts/Models/Enums/SceneEnums.cs`
```csharp
public enum SceneType
{
    BootstrapScene,
    BaseScene,
    BaseMenuScene,
    StartScene,
    LevelScene,
}
```

### `Scripts/Models/Enums/GameEnums.cs`
```csharp
public enum LanguageEnum
{
    English,
    Spanish,
}
```

---

## 2. Modelos Core

### `Scripts/Models/Classes/Notification.cs`
```csharp
public class Notification
{
    public NotificationType Type { get; private set; }
    public string Content { get; private set; }
    public string Sender { get; private set; }

    public Notification(NotificationType type, string content, string sender)
    {
        Type = type;
        Content = content;
        Sender = sender;
    }
}
```

### `Scripts/Models/Classes/SceneInfo.cs`
```csharp
using System.Collections.Generic;

public class SceneInfo
{
    public string SceneName { get; }
    public List<string> Dependencies { get; }

    public SceneInfo(string sceneName, params string[] dependencies)
    {
        SceneName = sceneName;
        Dependencies = new List<string>(dependencies);
    }
}
```

### `Scripts/Models/Classes/PlayerState.cs`
```csharp
public class PlayerState
{
    public float MoveSpeed;
    public float SprintMultiplier;
    public float JumpForce;
    public float InteractRange;

    public PlayerState(PlayerConfig config)
    {
        MoveSpeed = config.MoveSpeed;
        SprintMultiplier = config.SprintMultiplier;
        JumpForce = config.JumpForce;
        InteractRange = config.InteractRange;
    }
}
```

### `Scripts/Models/Classes/RunState.cs`
```csharp
public class RunState
{
    public float TimeElapsed;
    public int MovesUsed;
    public int HintsUsed;
    public string CurrentLevelId;
}
```

### `Scripts/Models/Interfaces/IInteractable.cs`
```csharp
using UnityEngine;

public interface IInteractable
{
    void OnInteract(GameObject interactor);
    string PromptKey { get; }
}
```

---

## 3. Singleton

### `Scripts/Models/Classes/Singleton.cs`
```csharp
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObj = new();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = FindAnyObjectByType<T>();
                        if (instance == null)
                        {
                            GameObject singletonObject = new(typeof(T).Name);
                            instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
```

---

## 4. Sistema de Notificaciones

### `Scripts/Static/NotificationQueues.cs`
```csharp
using System;
using System.Collections.Generic;

public static class NotificationQueue
{
    private static readonly List<Action<Notification>> subscribers = new();

    public static void Subscribe(Action<Notification> subscriber)
    {
        if (!subscribers.Contains(subscriber))
            subscribers.Add(subscriber);
    }

    public static void Unsubscribe(Action<Notification> subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public static void SendMessage(Notification message)
    {
        var snapshot = new List<Action<Notification>>(subscribers);
        foreach (var s in snapshot)
            s.Invoke(message);
    }
}

public static class PlayerNotificationQueue
{
    private static readonly List<Action<Notification>> subscribers = new();

    public static void Subscribe(Action<Notification> subscriber)
    {
        if (!subscribers.Contains(subscriber))
            subscribers.Add(subscriber);
    }

    public static void Unsubscribe(Action<Notification> subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public static void SendMessage(Notification message)
    {
        var snapshot = new List<Action<Notification>>(subscribers);
        foreach (var s in snapshot)
            s.Invoke(message);
    }
}
```

---

## 5. Scene Loader

### `Scripts/Models/Classes/SceneUtils.cs`
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
    private static readonly Dictionary<SceneType, SceneInfo> SceneInfoMap = new()
    {
        { SceneType.BootstrapScene, new SceneInfo("BootstrapScene") },
        { SceneType.BaseScene,      new SceneInfo("BaseScene") },
        { SceneType.BaseMenuScene,  new SceneInfo("BaseMenuScene", "BaseScene") },
        { SceneType.StartScene,     new SceneInfo("StartScene", "BaseMenuScene", "BaseScene") },
        { SceneType.LevelScene,     new SceneInfo("LevelScene", "BaseScene") },
    };

    public static SceneInfo GetSceneInfo(this SceneType sceneType)
    {
        if (SceneInfoMap.TryGetValue(sceneType, out var info))
            return info;
        throw new ArgumentOutOfRangeException(nameof(sceneType), sceneType, null);
    }

    public static string GetLastAdditiveSceneName()
    {
        Scene last = new();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s != SceneManager.GetActiveScene())
                last = s;
        }
        return last.name;
    }
}
```

### `Scripts/Static/SceneLoader.cs`
```csharp
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(SceneType sceneType, System.Action onSceneLoaded = null)
    {
        AppManager.Instance.StartCoroutine(LoadSceneAsync(sceneType, onSceneLoaded));
    }

    public static void ReturnToMainMenu()
    {
        AppManager.Instance.ResumeGame();
        if (RunManager.Instance != null) Object.Destroy(RunManager.Instance.gameObject);
        if (LevelManager.Instance != null) Object.Destroy(LevelManager.Instance.gameObject);
        AudioManager.Instance.StopAllBgm();
        SceneManager.LoadScene("BootstrapScene");
    }

    private static IEnumerator LoadSceneAsync(SceneType sceneType, System.Action onSceneLoaded)
    {
        SceneInfo info = sceneType.GetSceneInfo();
        bool first = false;
        foreach (string dep in info.Dependencies.AsEnumerable().Reverse())
        {
            if (!first)
            {
                first = true;
                yield return SceneManager.LoadSceneAsync(dep, LoadSceneMode.Single);
            }
            else
            {
                yield return SceneManager.LoadSceneAsync(dep, LoadSceneMode.Additive);
            }
        }

        if (info.Dependencies.Count > 0)
            yield return SceneManager.LoadSceneAsync(info.SceneName, LoadSceneMode.Additive);
        else
            yield return SceneManager.LoadSceneAsync(info.SceneName, LoadSceneMode.Single);

        onSceneLoaded?.Invoke();
    }
}
```

### `Scripts/Bootstrap/GameBootstrap.cs`
```csharp
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SceneType InitialScene = SceneType.StartScene;

    private void Start()
    {
        SceneLoader.LoadScene(InitialScene);
    }
}
```

---

## 6. ScriptableObjects

### `Scripts/ScriptableObjects/AppConfig.cs`
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "SO/AppConfig")]
public class AppConfig : ScriptableObject
{
    public LanguageEnum Language = LanguageEnum.English;
    public string MainFont;
    public float MouseSensitivity = 2f;
    public bool InvertY = false;
}
```

### `Scripts/ScriptableObjects/GameConfig.cs`
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "SO/GameConfig")]
public class GameConfig : ScriptableObject
{
    // TODO: configuración global del juego
}
```

### `Scripts/ScriptableObjects/PlayerConfig.cs`
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SO/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public int Id;
    public string PlayerName;
    public float MoveSpeed = 5f;
    public float SprintMultiplier = 1.5f;
    public float JumpForce = 5f;
    public float InteractRange = 2.5f;
}
```

### `Scripts/ScriptableObjects/LevelConfig.cs`
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string Name;
    public string DisplayNameKey;
    public float ParTime;
    public int ParMoves;
}
```

---

## 7. Localización

### `Scripts/Utils/LanguageUtils.cs`
```csharp
public static class LanguageEnumExtensions
{
    public static string ToLanguageCode(this LanguageEnum language)
    {
        return language switch
        {
            LanguageEnum.English => "en",
            LanguageEnum.Spanish => "es",
            _ => "en"
        };
    }
}
```

### `Scripts/Managers/LocalizationManager.cs`
```csharp
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
```

### `Scripts/UI/LocalizedText.cs`
```csharp
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string Key;
    private TMP_Text label;

    private void Awake() => label = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        NotificationQueue.Subscribe(OnMessage);
        Refresh();
    }

    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LanguageChanged) Refresh();
    }

    private void Refresh() => label.text = LocalizationManager.Instance.GetLocalizedText(Key);
}
```

---

## 8. Utils y CSV Parser

### `Scripts/Utils/CsvParser.cs`
```csharp
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class CsvParser
{
    public static List<Dictionary<string, string>> ParseFromText(string csvContent)
    {
        return ParseLines(csvContent.Split('\n'));
    }

    public static List<Dictionary<string, string>> Parse(string filePath)
    {
        return ParseLines(ReadAllLinesSafe(filePath));
    }

    static List<Dictionary<string, string>> ParseLines(string[] lines)
    {
        var results = new List<Dictionary<string, string>>();
        if (lines.Length < 2) return results;

        var headers = SplitCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            var values = SplitCsvLine(line);
            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length && j < values.Length; j++)
                row[headers[j].Trim()] = values[j].Trim();
            results.Add(row);
        }
        return results;
    }

    static string[] ReadAllLinesSafe(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line) lines.Add(line);
        return lines.ToArray();
    }

    static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                    else inQuotes = false;
                }
                else current.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(current.ToString()); current.Clear(); }
                else current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
```

---

## 9. Editor — Pipeline CSV→SO

### `Scripts/Editor/SoFieldWriter.cs`
```csharp
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
```

### `Scripts/Editor/CsvToSoImporter.cs`
```csharp
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
```

### `Scripts/Editor/CsvPostprocessor.cs`
```csharp
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
```

### `Scripts/Editor/SceneLoaderMenu.cs`
```csharp
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneLoaderMenu
{
    [MenuItem("Tools/Scene Loader/Bootstrap")]
    public static void LoadBootstrap() => LoadWithDeps(SceneType.BootstrapScene);

    [MenuItem("Tools/Scene Loader/Base")]
    public static void LoadBase() => LoadWithDeps(SceneType.BaseScene);

    [MenuItem("Tools/Scene Loader/Start")]
    public static void LoadStart() => LoadWithDeps(SceneType.StartScene);

    [MenuItem("Tools/Scene Loader/Level")]
    public static void LoadLevel() => LoadWithDeps(SceneType.LevelScene);

    [MenuItem("Tools/Play from Bootstrap")]
    public static void PlayFromBootstrap()
    {
        LoadWithDeps(SceneType.BootstrapScene);
        EditorApplication.isPlaying = true;
    }

    static void LoadWithDeps(SceneType sceneType)
    {
        SceneInfo info = sceneType.GetSceneInfo();
        bool first = false;
        foreach (string dep in info.Dependencies.AsEnumerable().Reverse())
        {
            if (!first) { first = true; EditorSceneManager.OpenScene($"Assets/Scenes/{dep}.unity"); }
            else EditorSceneManager.OpenScene($"Assets/Scenes/{dep}.unity", OpenSceneMode.Additive);
        }

        if (info.Dependencies.Count > 0)
            EditorSceneManager.OpenScene($"Assets/Scenes/{info.SceneName}.unity", OpenSceneMode.Additive);
        else
            EditorSceneManager.OpenScene($"Assets/Scenes/{info.SceneName}.unity");
    }
}
```

---

## 10. Managers (Esqueletos)

### `Scripts/Managers/AppManager.cs`
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : Singleton<AppManager>
{
    [Header("Config")]
    [SerializeField] private AppConfig appConfig;

    public bool IsTheGamePaused { get; private set; }
    private LanguageEnum language = LanguageEnum.English;

    protected override void Awake() => base.Awake();

    private void Start()
    {
        AssignDependencies();
        IsTheGamePaused = false;
        Time.timeScale = 1f;
        language = appConfig != null ? appConfig.Language : LanguageEnum.English;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void PauseGame()
    {
        IsTheGamePaused = true;
        Time.timeScale = 0f;
        NotificationQueue.SendMessage(new(NotificationType.GamePaused, "", "AppManager"));
    }

    public void ResumeGame()
    {
        IsTheGamePaused = false;
        Time.timeScale = 1f;
        NotificationQueue.SendMessage(new(NotificationType.GameResumed, "", "AppManager"));
    }

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            case NotificationType.GameLoaded: ResumeGame(); break;
            case NotificationType.GameOver: PauseGame(); break;
        }
    }

    private void AssignDependencies()
    {
        if (appConfig == null)
            appConfig = Resources.Load<AppConfig>("SO/AppConfig");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single) AssignDependencies();
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
```

### `Scripts/Managers/RunManager.cs`
```csharp
using UnityEngine;

public class RunManager : Singleton<RunManager>
{
    public RunState RunState { get; private set; }

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void StartNewRun(string levelId)
    {
        RunState = new RunState { CurrentLevelId = levelId };
    }

    private void Update()
    {
        if (AppManager.Instance.IsTheGamePaused || RunState == null) return;
        RunState.TimeElapsed += Time.deltaTime;
    }

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            case NotificationType.LevelCompleted:
                // TODO: guardar stats del nivel completado
                break;
            case NotificationType.MovePerformed:
                if (RunState != null) RunState.MovesUsed++;
                break;
            case NotificationType.HintUsed:
                if (RunState != null) RunState.HintsUsed++;
                break;
        }
    }
}
```

### `Scripts/Managers/LevelManager.cs`
```csharp
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public LevelConfig CurrentLevel { get; private set; }

    protected override void Awake() => base.Awake();

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void LoadLevel(LevelConfig config)
    {
        CurrentLevel = config;
        RunManager.Instance.StartNewRun(config.Name);
        NotificationQueue.SendMessage(new(NotificationType.LevelLoaded, config.Name, "LevelManager"));
        // TODO: cargar la escena del nivel si es necesario
    }

    public void CompleteLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelCompleted, CurrentLevel?.Name ?? "", "LevelManager"));
    }

    public void FailLevel()
    {
        NotificationQueue.SendMessage(new(NotificationType.LevelFailed, CurrentLevel?.Name ?? "", "LevelManager"));
    }

    private void OnMessage(Notification n)
    {
        // TODO: reaccionar a eventos de nivel
    }
}
```

### `Scripts/Managers/AudioManager.cs`
```csharp
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    protected override void Awake() => base.Awake();

    public void PlaySfx(string soundId) { /* TODO */ }
    public void PlayBgm(string soundId) { /* TODO */ }
    public void StopAllBgm() { /* TODO */ }
}
```

### `Scripts/Managers/ConfigManager.cs`
```csharp
using UnityEngine;

public class ConfigManager : Singleton<ConfigManager>
{
    [SerializeField] private GameConfig gameConfig;

    public GameConfig GameConfig => gameConfig;

    protected override void Awake()
    {
        base.Awake();
        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("SO/GameConfig");
    }
}
```

### `Scripts/Managers/PlayerActionsManager.cs`
```csharp
using UnityEngine;

public class PlayerActionsManager : Singleton<PlayerActionsManager>
{
    public PlayerActions PlayerActions { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PlayerActions = new PlayerActions();
        PlayerActions.Enable();
    }

    private void OnDestroy() => PlayerActions?.Disable();
    private void OnApplicationQuit() => PlayerActions?.Disable();
}
```

> **Nota:** `PlayerActions` es la clase C# generada automáticamente por Unity desde el asset `.inputactions`. Ver sección "Input System Action Map" para la estructura.

---

## 11. Player — Implementación FPS

> **Diferencia con el proyecto 2D de referencia:** Se usa `CharacterController` en lugar de `Rigidbody2D` — es el enfoque estándar para FPS en 3D. El patrón de composición (`PlayerController` orquestando subsistemas hermanos via `Initialize(state)`) es idéntico.

### `Scripts/Player/PlayerController.cs`
```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerLook look;
    private PlayerInteraction interaction;

    public void Initialize(PlayerState state)
    {
        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        interaction = GetComponent<PlayerInteraction>();

        movement.Initialize(state);
        interaction.Initialize(state);
    }

    private void OnEnable() => PlayerNotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => PlayerNotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            // TODO: routing de notificaciones del jugador
            default: break;
        }
    }
}
```

### `Scripts/Player/PlayerMovement.cs`
```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;
    private PlayerState state;
    private bool isPaused;

    private const float Gravity = -20f;
    private float verticalVelocity;

    public void Initialize(PlayerState playerState)
    {
        state = playerState;
        cc = GetComponent<CharacterController>();
    }

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused) isPaused = true;
        else if (n.Type == NotificationType.GameResumed) isPaused = false;
    }

    private void Update()
    {
        if (isPaused || state == null) return;

        Vector2 input = PlayerActionsManager.Instance.PlayerActions.Player.Move.ReadValue<Vector2>();
        bool sprinting = PlayerActionsManager.Instance.PlayerActions.Player.Sprint.IsPressed();

        float speed = state.MoveSpeed * (sprinting ? state.SprintMultiplier : 1f);
        Vector3 move = (transform.forward * input.y + transform.right * input.x).normalized * speed;

        if (cc.isGrounded)
            verticalVelocity = -2f;
        verticalVelocity += Gravity * Time.deltaTime;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }
}
```

### `Scripts/Player/PlayerLook.cs`
```csharp
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private bool invertY = false;

    private float pitch;
    private bool isPaused;

    private void Start() => Cursor.lockState = CursorLockMode.Locked;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused)
        {
            isPaused = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (n.Type == NotificationType.GameResumed)
        {
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (isPaused) return;

        Vector2 delta = PlayerActionsManager.Instance.PlayerActions.Player.Look.ReadValue<Vector2>() * sensitivity;

        transform.Rotate(Vector3.up * delta.x);

        float pitchDelta = invertY ? delta.y : -delta.y;
        pitch = Mathf.Clamp(pitch + pitchDelta, -89f, 89f);
        if (cameraAnchor != null)
            cameraAnchor.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
```

### `Scripts/Player/PlayerInteraction.cs`
```csharp
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    private PlayerState state;
    private bool isPaused;

    public void Initialize(PlayerState playerState) => state = playerState;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused) isPaused = true;
        else if (n.Type == NotificationType.GameResumed) isPaused = false;
    }

    private void Update()
    {
        if (isPaused || state == null) return;

        if (!PlayerActionsManager.Instance.PlayerActions.Player.Interact.WasPressedThisFrame()) return;

        Camera cam = cameraTransform != null
            ? cameraTransform.GetComponent<Camera>()
            : Camera.main;

        if (cam == null) return;

        Ray ray = new(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, state.InteractRange))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.OnInteract(gameObject);
                NotificationQueue.SendMessage(new(NotificationType.InteractionPerformed, hit.collider.name, "PlayerInteraction"));
            }
        }
    }
}
```

### `Scripts/Camera/FpsCameraController.cs`
```csharp
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FpsCameraController : MonoBehaviour
{
    [SerializeField] private float defaultFov = 70f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = defaultFov;
    }

    // TODO: shake, FOV lerp al sprintar, efectos de cámara
}
```

### Prefab del Player — Jerarquía recomendada

```
Player (GameObject)
├── CharacterController        ← componente
├── PlayerController           ← componente
├── PlayerMovement             ← componente
├── PlayerLook                 ← componente (referencia CameraAnchor)
├── PlayerInteraction          ← componente (referencia Camera)
└── CameraAnchor (Transform)   ← hijo vacío a altura de los ojos
    └── MainCamera (Camera)
        └── FpsCameraController ← componente
```

- Tag del Player: `"Player"`
- Layer del Player: `Player`
- CharacterController: Center Y = 0.9, Height = 1.8, Radius = 0.4

---

## 12. Trigger — Template

### `Scripts/Triggers/LevelExitTrigger.cs`
```csharp
using UnityEngine;

// Coloca este componente en un GameObject con Collider (isTrigger = true).
// Cuando el jugador entra, emite LevelCompleted.
// Patrón a seguir para todos los triggers: detectar → emitir notificación → NO llamar a managers directamente.
public class LevelExitTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LevelLoaded)
            triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        LevelManager.Instance.CompleteLevel();
    }
}
```

**Otros triggers a crear** (siguiendo el mismo patrón):
- `Environment/PressurePlate.cs` — activa/desactiva mecanismos al entrar/salir
- `Triggers/CheckpointTrigger.cs` — emite `CheckpointReached`
- `Environment/InteractionZone.cs` — muestra prompt UI cuando el jugador está en rango

---

## 13. Pipeline CSV→SO — Ejemplo Completo

### `Resources/Data/Levels.csv`
```
Name,DisplayNameKey,ParTime,ParMoves
// Formato: Name = nombre exacto del .asset en SO/Levels/
Level01,level.01.name,120,15
Level02,level.02.name,90,12
Level03,level.03.name,180,20
```

**Para añadir un nuevo tipo de SO al pipeline**, añade una entrada en el array `Routes` de `CsvToSoImporter.cs`:

```csharp
new()
{
    CsvFile = "NombreArchivo.csv",
    AssetFolder = "Assets/Resources/SO/Carpeta",
    SoType = typeof(MiScriptableObject),
    AllowedFields = new HashSet<string> { "Campo1", "Campo2" }
}
```

Reglas:
- Los assets deben existir previamente (el importer solo actualiza, nunca crea)
- La columna `Name` debe coincidir exactamente con el nombre del `.asset`
- Campos de tipo Prefab/Sprite/Texture deben excluirse del whitelist

---

## 14. Input System — Action Map

Crea el asset `.inputactions` en `Assets/Actions/PlayerActions.inputactions` con esta estructura:

```
Action Maps:
├── Player
│   ├── Move            → Vector2   (Binding: WASD / Left Stick)
│   ├── Look            → Vector2   (Binding: Mouse Delta / Right Stick)
│   ├── Jump            → Button    (Binding: Space / Button South)
│   ├── Sprint          → Button    (Binding: Left Shift / Left Stick Press, Hold)
│   ├── Interact        → Button    (Binding: E / Button West)
│   └── Pause           → Button    (Binding: Escape / Start)
└── UI
    ├── Navigate        → Vector2
    ├── Submit          → Button
    └── Cancel          → Button
```

Pasos en el Editor de Unity:
1. Crear asset: `Assets/Actions/ → (clic derecho) → Create → Input Actions`
2. Nombrar `PlayerActions`
3. En el Inspector: activar "Generate C# Class", nombre de clase `PlayerActions`
4. Aplicar y guardar — Unity genera `PlayerActions.cs` automáticamente
5. El `PlayerActionsManager` instancia esta clase y la expone como `PlayerActions.Player.Move`, etc.

---

## 15. Adaptaciones 2D → 3D (Notas Críticas)

- `Rigidbody2D` → no existe; usar `CharacterController` (FPS) o `Rigidbody` (física pura)
- `Collider2D` → `Collider`; `OnTriggerEnter2D(Collider2D)` → `OnTriggerEnter(Collider)`
- `Vector2` para movimiento → `Vector3`; input de dirección proyectado sobre `transform.forward` / `transform.right`
- No hay "flip de sprite" en 3D — usar Animator State Machines sobre `SkinnedMeshRenderer`
- Camera en FPS: montar como hija de `CameraAnchor` — pitch gestionado por `PlayerLook`, yaw rota todo el GO del Player
- `Cursor.lockState = CursorLockMode.Locked` mientras hay gameplay; `None` al pausar
- **Physics Settings:** configurar capas en `Edit → Project Settings → Physics` (3D, no Physics2D)
  - Capas recomendadas: `Default, Player, Ground, Interactable, IgnorePlayer`
  - En la collision matrix: `Player` ignora `Player`; `IgnorePlayer` ignora `Player` y `Interactable`
- Project Settings → Physics2D no se usa; configurar solo la sección Physics (3D)

---

## 16. Convenciones Obligatorias

1. **Suscripción:** siempre en `OnEnable`, desuscripción en `OnDisable`. NUNCA en `Awake` o `Start`.
2. **Notification.Content** es siempre `string`. Parsear con `float.Parse(val, CultureInfo.InvariantCulture)`.
3. **ScriptableObjects:** acceder via `Resources.Load<T>("SO/...")`. Los managers cargan en `AssignDependencies()` con fallback automático.
4. **Triggers:** solo detectan y emiten notificaciones. NUNCA llaman directamente a managers.
5. **Editor scripts:** deben estar en carpeta `Editor/` o dentro de `#if UNITY_EDITOR`. No se incluyen en builds.
6. **Managers:** heredan de `Singleton<T>`. Se crean automáticamente si no existen. `DontDestroyOnLoad` automático.
7. **Player subsistemas:** reciben `Initialize(PlayerState)` desde `PlayerController`. No se buscan entre sí; el orquestador centraliza las referencias.
8. **Idioma:** comentarios, nombres de variables en los TODOs y commits en castellano.

---

## 17. Escena: Composición y Setup

### Árbol de carga aditiva
```
BootstrapScene (índice 0 en Build Settings)
  └── carga → StartScene
        ├── dep BaseScene     (Single — se carga primero)
        └── dep BaseMenuScene (Additive)
              └── StartScene  (Additive — escena principal del menú)

Para gameplay:
  SceneLoader.LoadScene(SceneType.LevelScene)
        ├── BaseScene (Single)
        └── LevelScene (Additive)
```

### BaseScene — GameObjects necesarios
- `AppManager` (con `AppManager.cs`)
- `RunManager` (con `RunManager.cs`)
- `LevelManager` (con `LevelManager.cs`)
- `AudioManager`
- `ConfigManager`
- `LocalizationManager`
- `PlayerActionsManager`

Estos singletons persisten via `DontDestroyOnLoad`. La BootstrapScene los inicializa indirectamente al cargar BaseScene.

---

## 18. CLAUDE.md — Pegar en el Nuevo Proyecto

Copia el siguiente bloque como `CLAUDE.md` en la raíz del nuevo repositorio Unity:

```markdown
# CLAUDE.md

Este archivo guía a Claude Code cuando trabaja en este repositorio.

## Proyecto

Unity 6 (6000.3.x) — juego de puzzles 3D en primera persona. Sin tests automatizados. El build se hace desde el Editor de Unity.

## Herramientas del Editor

- **Importar CSVs de balance:** `Tools > Balance > Import All CSVs`
  - Los CSVs en `Assets/Resources/Data/` también se auto-importan al guardar via `CsvPostprocessor`
- **Cargar escenas:** `Tools > Scene Loader > [Escena]`

## Arquitectura

### Sistema de Eventos (patrón principal de comunicación)

Toda comunicación entre sistemas usa un pub/sub estático en `Assets/Scripts/Static/NotificationQueues.cs`. Dos colas:
- `NotificationQueue` — eventos generales del juego
- `PlayerNotificationQueue` — eventos específicos del jugador

```csharp
NotificationQueue.Subscribe(OnMessage);
NotificationQueue.SendMessage(new(NotificationType.LevelCompleted, levelId, "LevelManager"));
NotificationQueue.Unsubscribe(OnMessage);
```

Suscribir siempre en `OnEnable`, desuscribir en `OnDisable`. Los tipos están en `NotificationEnums.cs`.

### Managers (Singletons)

Usan la base genérica `Singleton<T>` (`Models/Classes/Singleton.cs`) — se auto-crea y persiste con `DontDestroyOnLoad`.

- **AppManager** — pausa/resume (`Time.timeScale`), idioma
- **RunManager** — estado de la partida actual (`RunState`: tiempo, movimientos, hints)
- **LevelManager** — carga y completa niveles, emite notificaciones de nivel
- **AudioManager** — sfx y bgm
- **ConfigManager** — acceso a `GameConfig` SO
- **LocalizationManager** — carga `translations.csv`, resuelve claves de texto
- **PlayerActionsManager** — instancia la clase generada `PlayerActions` del new Input System

### ScriptableObjects (Configuración)

Todos los datos del juego viven en `Assets/Resources/SO/`. Cargar via `Resources.Load<T>()`.

| SO | Ubicación | Uso |
|---|---|---|
| `AppConfig` | `SO/` | Idioma, fuente, sensibilidad de ratón |
| `GameConfig` | `SO/` | Configuración global |
| `PlayerConfig` | `SO/Player/` | Velocidad, sprint, salto, rango de interacción |
| `LevelConfig` | `SO/Levels/` | Nombre, par de tiempo, par de movimientos |

### Player (FPS)

`PlayerController` orquesta subsistemas hermanos en el mismo prefab:
- `PlayerMovement` — `CharacterController` 3D, movimiento relativo a cámara, sprint
- `PlayerLook` — rotación yaw del body, pitch de `CameraAnchor` con clamp ±89°, cursor lock
- `PlayerInteraction` — raycast desde cámara, detecta `IInteractable`, emite `InteractionPerformed`

El Player tiene un hijo `CameraAnchor` (Transform vacío) con una `Camera` hija. `PlayerLook` rota el anchor.

### Triggers

Los triggers son `MonoBehaviour` en objetos con `Collider` (`isTrigger = true`). En `OnTriggerEnter(Collider)` comprueban el tag `"Player"`, usan un flag de debounce, y emiten una notificación. **NUNCA llaman a managers directamente.**

### Pipeline CSV→SO

Cuatro scripts en `Editor/`: `CsvParser`, `SoFieldWriter`, `CsvToSoImporter`, `CsvPostprocessor`. Solo actualiza assets existentes, nunca crea. Los campos de Prefab/Sprite deben excluirse del whitelist.

### Escenas (carga aditiva)

`BootstrapScene (0)` → `StartScene` (con deps `BaseMenuScene`, `BaseScene`)
Para gameplay: `LevelScene` (con dep `BaseScene`)

`SceneUtils.cs` declara el mapa de dependencias. `SceneLoader.cs` ejecuta la carga en coroutina.

## Convenciones

- Suscribir en `OnEnable`, desuscribir en `OnDisable`
- `Notification.Content` siempre es `string` — parsear con `CultureInfo.InvariantCulture`
- Editor scripts en `Assets/Scripts/Editor/` (excluidos del build automáticamente)
- Scripts sin namespace (convención del proyecto)
- Comentarios, TODOs y commits en castellano

## Release Notes

Actualizar `Assets/docs/ReleaseNotes.md` al final de cada sesión con cambios relevantes.
Formato orientado a producto: features nuevas, bugs corregidos, mejoras visibles. Sin mencionar clases internas ni refactors.
```

---

*Fin del documento de bootstrap. Ejecuta los pasos del Checklist en orden.*
