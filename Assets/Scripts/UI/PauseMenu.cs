using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void OnResumeClicked() => AppManager.Instance.ResumeGame();
    public void OnQuitClicked() => SceneLoader.ReturnToMainMenu();
}
