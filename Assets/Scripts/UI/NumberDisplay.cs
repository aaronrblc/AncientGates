using System.Globalization;
using TMPro;
using UnityEngine;

public class NumberDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        NotificationQueue.Subscribe(OnMessage);
        if (PuzzleNumberManager.Instance != null)
            label.text = PuzzleNumberManager.Instance.CurrentValue.ToString(CultureInfo.InvariantCulture);
    }

    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.NumberChanged) return;
        label.text = n.Content;
    }
}
