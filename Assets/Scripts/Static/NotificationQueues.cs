using System;
using System.Collections.Generic;

public static class NotificationQueue
{
    private static readonly List<Action<Notification>> subscribers = new();

    public static void Subscribe(Action<Notification> subscriber)
    {
        if (!subscribers.Contains(subscriber))
            subscribers.Add(subscriber);
    }

    public static void Unsubscribe(Action<Notification> subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public static void SendMessage(Notification message)
    {
        var snapshot = new List<Action<Notification>>(subscribers);
        foreach (var s in snapshot)
            s.Invoke(message);
    }
}

public static class PlayerNotificationQueue
{
    private static readonly List<Action<Notification>> subscribers = new();

    public static void Subscribe(Action<Notification> subscriber)
    {
        if (!subscribers.Contains(subscriber))
            subscribers.Add(subscriber);
    }

    public static void Unsubscribe(Action<Notification> subscriber)
    {
        subscribers.Remove(subscriber);
    }

    public static void SendMessage(Notification message)
    {
        var snapshot = new List<Action<Notification>>(subscribers);
        foreach (var s in snapshot)
            s.Invoke(message);
    }
}
