using System.Collections.Generic;

public class SceneInfo
{
    public string SceneName { get; }
    public List<string> Dependencies { get; }

    public SceneInfo(string sceneName, params string[] dependencies)
    {
        SceneName = sceneName;
        Dependencies = new List<string>(dependencies);
    }
}
