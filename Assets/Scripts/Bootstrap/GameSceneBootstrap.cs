using UnityEngine;

public class GameSceneBootstrap : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerConfig playerConfig;

    private void Start()
    {
        if (playerConfig == null)
            playerConfig = Resources.Load<PlayerConfig>("SO/Player/PlayerConfig");

        player.Initialize(new PlayerState(playerConfig));
    }
}
