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
