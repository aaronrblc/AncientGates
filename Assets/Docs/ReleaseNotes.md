# Release Notes — AncientGates

## v0.2.0 — Editor visual de esquemas de puzzles (2026-05-24)

- Nueva herramienta: `Tools > Level Designer` — editor 2D top-down para diseñar esquemas de niveles sin tocar la escena
- Dibuja paredes y puertas en aristas entre celdas, coloca modificadores (+/-/×/÷/=) y resets en celdas, marca Start y Exit
- Las puertas soportan condición de apertura (`= N`, `> N`, `% N`…) y override opcional del número al cruzar (útil para separar zonas de puzzle)
- Solver integrado: enumera todas las combinaciones de subconjunto × orden de modificadores y marca cuáles cumplen la condición de la puerta objetivo; funciona con hasta 7 modificadores
- Esquemas se guardan como ScriptableObject `.asset` en `Assets/LevelDesigns/` — abribles y editables en cualquier momento
- Refactor interno: lógica matemática del puzzle (`Apply`, `Evaluate`) extraída a `PuzzleMath` para ser compartida entre runtime y editor

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
