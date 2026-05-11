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
