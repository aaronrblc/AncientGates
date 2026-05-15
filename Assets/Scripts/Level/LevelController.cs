using UnityEngine;

public class LevelController : MonoBehaviour
{
    [SerializeField] private Transform startingPoint;

    private void Start()
    {
        PlacePlayer();
    }

    private void PlacePlayer()
    {
        if (startingPoint == null) return;
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var cc = player.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(
            startingPoint.position,
            Quaternion.Euler(0f, startingPoint.eulerAngles.y, 0f)
        );

        if (cc != null) cc.enabled = true;
    }
}
