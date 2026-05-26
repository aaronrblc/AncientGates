using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ModifierGroup : MonoBehaviour
{
    public enum ResolutionMode { Hide, Disable }

    [SerializeField] private ResolutionMode mode;
    [SerializeField] private List<InteractableTrigger> members;

    private readonly Dictionary<InteractableTrigger, UnityAction> _listeners = new();

    private void OnEnable()
    {
        foreach (var member in members)
        {
            if (member == null) continue;
            var captured = member;
            UnityAction action = () => OnMemberActivated(captured);
            _listeners[member] = action;
            member.OnInteracted.AddListener(action);
        }
        NotificationQueue.Subscribe(OnMessage);
    }

    private void OnDisable()
    {
        foreach (var member in members)
        {
            if (member == null) continue;
            if (_listeners.TryGetValue(member, out var action))
                member.OnInteracted.RemoveListener(action);
        }
        _listeners.Clear();
        NotificationQueue.Unsubscribe(OnMessage);
    }

    private void OnMemberActivated(InteractableTrigger chosen)
    {
        foreach (var member in members)
        {
            if (member == null || member == chosen) continue;
            if (mode == ResolutionMode.Hide)
                member.gameObject.SetActive(false);
            else
                member.MarkUsed();
        }
    }

    private void OnMessage(Notification n)
    {
        if (n.Type != NotificationType.LevelReset) return;
        foreach (var member in members)
        {
            if (member == null) continue;
            member.gameObject.SetActive(true);
            member.Reset();
        }
    }
}
