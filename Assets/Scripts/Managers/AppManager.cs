using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : Singleton<AppManager>
{
    [Header("Config")]
    [SerializeField] private AppConfig appConfig;

    public bool IsTheGamePaused { get; private set; }
    private LanguageEnum language = LanguageEnum.English;

    protected override void Awake() => base.Awake();

    private void Start()
    {
        AssignDependencies();
        IsTheGamePaused = false;
        Time.timeScale = 1f;
        language = appConfig != null ? appConfig.Language : LanguageEnum.English;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    public void PauseGame()
    {
        IsTheGamePaused = true;
        Time.timeScale = 0f;
        NotificationQueue.SendMessage(new(NotificationType.GamePaused, "", "AppManager"));
    }

    public void ResumeGame()
    {
        IsTheGamePaused = false;
        Time.timeScale = 1f;
        NotificationQueue.SendMessage(new(NotificationType.GameResumed, "", "AppManager"));
    }

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            case NotificationType.GameLoaded: ResumeGame(); break;
            case NotificationType.GameOver: PauseGame(); break;
        }
    }

    private void AssignDependencies()
    {
        if (appConfig == null)
            appConfig = Resources.Load<AppConfig>("SO/AppConfig");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single) AssignDependencies();
    }

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
