public class Notification
{
    public NotificationType Type { get; private set; }
    public string Content { get; private set; }
    public string Sender { get; private set; }

    public Notification(NotificationType type, string content, string sender)
    {
        Type = type;
        Content = content;
        Sender = sender;
    }
}
