using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneLoaderMenu
{
    [MenuItem("Tools/Scene Loader/Bootstrap")]
    public static void LoadBootstrap() => OpenSingle("BootstrapScene");

    [MenuItem("Tools/Scene Loader/MainMenu")]
    public static void LoadMainMenu() => OpenSingle("MainMenuScene");

    [MenuItem("Tools/Scene Loader/Game")]
    public static void LoadGame()
    {
        OpenSingle("BootstrapScene");
        OpenAdditive("GameScene");
    }

    [MenuItem("Tools/Play from Bootstrap")]
    public static void PlayFromBootstrap()
    {
        OpenSingle("BootstrapScene");
        EditorApplication.isPlaying = true;
    }

    static void OpenSingle(string sceneName) =>
        EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity");

    static void OpenAdditive(string sceneName) =>
        EditorSceneManager.OpenScene($"Assets/Scenes/{sceneName}.unity", OpenSceneMode.Additive);
}
