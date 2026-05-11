using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        SceneLoader.LoadScene(SceneType.MainMenuScene);
    }
}
