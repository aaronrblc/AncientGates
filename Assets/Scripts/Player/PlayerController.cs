using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerLook look;
    private PlayerInteraction interaction;

    public void Initialize(PlayerState state)
    {
        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        interaction = GetComponent<PlayerInteraction>();

        movement.Initialize(state);
        interaction.Initialize(state);
    }

    private void OnEnable() => PlayerNotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => PlayerNotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        switch (n.Type)
        {
            // TODO: routing de notificaciones del jugador
            default: break;
        }
    }
}
