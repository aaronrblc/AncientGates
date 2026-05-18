using System;
using TMPro;
using UnityEngine;

public class LevelCompletePanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text statsText;

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void Start() => panel?.SetActive(false);

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.LevelCompleted) return;

        if (statsText != null)
        {
            var run = GameManager.Instance.RunState;
            if (run != null)
            {
                TimeSpan t = TimeSpan.FromSeconds(run.TimeElapsed);
                statsText.text = $"Nivel completado\n{t.Minutes:D2}:{t.Seconds:D2}";
            }
            else
            {
                statsText.text = "Nivel completado";
            }
        }

        panel?.SetActive(true);
        AppManager.Instance.PauseGame();
    }
}
