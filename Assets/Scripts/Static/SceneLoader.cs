using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadScene(SceneType sceneType, System.Action onSceneLoaded = null)
    {
        AppManager.Instance.StartCoroutine(LoadSceneAsync(sceneType, onSceneLoaded));
    }

    // Carga GameScene y encima el escenario del nivel de forma aditiva
    public static void LoadLevel(string levelSceneName, System.Action onSceneLoaded = null)
    {
        AppManager.Instance.StartCoroutine(LoadLevelAsync(levelSceneName, onSceneLoaded));
    }

    public static void ReturnToMainMenu()
    {
        AppManager.Instance.ResumeGame();
        SceneManager.LoadScene("MainMenuScene");
    }

    private static IEnumerator LoadSceneAsync(SceneType sceneType, System.Action onSceneLoaded)
    {
        SceneInfo info = sceneType.GetSceneInfo();
        yield return SceneManager.LoadSceneAsync(info.SceneName, LoadSceneMode.Single);
        onSceneLoaded?.Invoke();
    }

    private static IEnumerator LoadLevelAsync(string levelSceneName, System.Action onSceneLoaded)
    {
        yield return SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
        yield return SceneManager.LoadSceneAsync(levelSceneName, LoadSceneMode.Additive);
        onSceneLoaded?.Invoke();
    }
}
