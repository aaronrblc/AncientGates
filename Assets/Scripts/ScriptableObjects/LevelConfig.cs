using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "SO/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string Name;
    public string DisplayNameKey;
    public float ParTime;
    public int ParMoves;
    public int puzzleInitialValue;
}
