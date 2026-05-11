public class PlayerState
{
    public float MoveSpeed;
    public float SprintMultiplier;
    public float JumpForce;
    public float InteractRange;

    public PlayerState(PlayerConfig config)
    {
        MoveSpeed = config.MoveSpeed;
        SprintMultiplier = config.SprintMultiplier;
        JumpForce = config.JumpForce;
        InteractRange = config.InteractRange;
    }
}
