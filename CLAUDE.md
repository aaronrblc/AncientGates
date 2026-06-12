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

### Modificadores de número (puzzles)

Prefab base: `Assets/Prefabs/Interactives/NumberModifiers/GeneralModifier.prefab`

Estructura fija de cada modificador:
```
GeneralModifier          ← InteractableTrigger + NumberModifier (lógica del puzzle)
├── Text Number          ← TextMeshPro con el valor operando
├── Text Operation       ← TextMeshPro con el símbolo de operación
└── Decoration
    ├── Dynamic          ← Objetos que reaccionan al uso; se cablea en OnInteracted del raíz
    │   ├── Light        ← Tiene su propio Animator (ej: light_active → light_using → light_used)
    │   └── FX_Fire_01   ← Tiene su propio Animator o se activa vía SetActive
    └── Static           ← Decoración visual pura, sin lógica ni Animator
```

**Patrón de reacción al uso:** `InteractableTrigger.OnInteracted` (UnityEvent en el Inspector) llama a los métodos de cada subobjeto en `Dynamic` — `Animator.SetTrigger`, `GameObject.SetActive`, etc. Sin scripts adicionales. Para añadir un nuevo efecto: añadir `+` en `OnInteracted` y apuntar al subobjeto correspondiente.

El `GeneralModifier` raíz **no tiene Animator**. Cada subobjeto en `Dynamic` gestiona su propia animación.

Para crear un nuevo modificador: duplicar `GeneralModifier`, cambiar meshes en `Static`, añadir/quitar objetos en `Dynamic`, y cablear el `OnInteracted`.

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
