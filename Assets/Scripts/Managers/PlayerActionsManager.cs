using UnityEngine;

public class PlayerActionsManager : Singleton<PlayerActionsManager>
{
    public InputActions InputActions { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InputActions = new InputActions();
        InputActions.Enable();
    }

    private void OnDestroy() => InputActions?.Disable();
    private void OnApplicationQuit() => InputActions?.Disable();
}
