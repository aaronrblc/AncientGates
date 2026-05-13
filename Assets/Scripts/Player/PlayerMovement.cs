using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController cc;
    private PlayerState state;
    private bool isPaused;

    private const float Gravity = -20f;
    private float verticalVelocity;

    public void Initialize(PlayerState playerState)
    {
        state = playerState;
        cc = GetComponent<CharacterController>();
    }

    private void OnEnable() => NotificationQueue.Subscribe(OnMessage);
    private void OnDisable() => NotificationQueue.Unsubscribe(OnMessage);

    private void OnMessage(Notification n)
    {
        if (n.Type == NotificationType.GamePaused) isPaused = true;
        else if (n.Type == NotificationType.GameResumed) isPaused = false;
    }

    private void Update()
    {
        if (isPaused || state == null) return;

        Vector2 input = PlayerActionsManager.Instance.InputActions.Player.Move.ReadValue<Vector2>();
        bool sprinting = PlayerActionsManager.Instance.InputActions.Player.Sprint.IsPressed();

        float speed = state.MoveSpeed * (sprinting ? state.SprintMultiplier : 1f);
        Vector3 move = (transform.forward * input.y + transform.right * input.x).normalized * speed;

        bool jumpPressed = PlayerActionsManager.Instance.InputActions.Player.Jump.WasPressedThisFrame();

        if (cc.isGrounded)
        {
            verticalVelocity = -2f;
            if (jumpPressed)
                verticalVelocity = state.JumpForce;
        }
        verticalVelocity += Gravity * Time.deltaTime;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }
}
