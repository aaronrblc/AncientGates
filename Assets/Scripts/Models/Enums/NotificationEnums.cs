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
    LevelReset,
    CheckpointReached,
    RoomEntered,

    // Jugador
    PlayerDied,

    // Interacción
    InteractionPerformed,
    ItemPicked,
    DoorOpened,
    PuzzleStateChanged,

    // Puzzle — El Número
    NumberChanged,

    // NPC
    NpcPlayerDetected,
    NpcPlayerLost,
}

public enum QueueType { Notification, Player }
