using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
    private static readonly Dictionary<SceneType, SceneInfo> SceneInfoMap = new()
    {
        { SceneType.BootstrapScene, new SceneInfo("BootstrapScene") },
        { SceneType.MainMenuScene,  new SceneInfo("MainMenuScene") },
        { SceneType.GameScene,      new SceneInfo("GameScene") },
    };

    public static SceneInfo GetSceneInfo(this SceneType sceneType)
    {
        if (SceneInfoMap.TryGetValue(sceneType, out var info))
            return info;
        throw new ArgumentOutOfRangeException(nameof(sceneType), sceneType, null);
    }

    public static string GetLoadedLevelSceneName()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != "GameScene" && s.name != "BootstrapScene")
                return s.name;
        }
        return null;
    }
}
