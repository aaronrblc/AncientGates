using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SO/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public int Id;
    public string PlayerName;
    public float MoveSpeed = 5f;
    public float SprintMultiplier = 1.5f;
    public float JumpForce = 5f;
    public float InteractRange = 2.5f;
}
