using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string Key;
    private TMP_Text label;

    private void Awake() => label = GetComponent<TMP_Text>();

    private void OnEnable()
    {
        NotificationQueue.Subscribe(OnMessage);
        Refresh();
    }

    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.LanguageChanged) Refresh();
    }

    private void Refresh() => label.text = LocalizationManager.Instance.GetLocalizedText(Key);
}
