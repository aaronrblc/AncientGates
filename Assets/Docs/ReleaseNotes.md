# Release Notes — AncientGates

## v0.1.0 — Bootstrap inicial (2026-05-11)

- Arquitectura base creada: Singleton<T>, NotificationQueue, SceneLoader con carga aditiva
- Managers: AppManager, RunManager, LevelManager, AudioManager, ConfigManager, LocalizationManager, PlayerActionsManager
- Player FPS: PlayerController, PlayerMovement, PlayerLook, PlayerInteraction con CharacterController
- Pipeline CSV→SO: CsvParser, SoFieldWriter, CsvToSoImporter, CsvPostprocessor
- ScriptableObjects: AppConfig, GameConfig, PlayerConfig, LevelConfig
- UI: UIController, PauseMenu, ScreenFader, LocalizedText (TMP)
- Trigger template: LevelExitTrigger
- Herramientas de editor: Tools > Balance > Import All CSVs, Tools > Scene Loader
- Datos iniciales: translations.csv (en/es), Levels.csv (3 niveles placeholder)
