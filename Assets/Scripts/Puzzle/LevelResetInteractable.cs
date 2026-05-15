using UnityEngine;

public class LevelResetInteractable : MonoBehaviour
{
    public void ResetLevel()
    {
        GameManager.Instance.ResetLevel();
    }
}
