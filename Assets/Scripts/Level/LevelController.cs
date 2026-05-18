using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static Vector3? OverridePosition;
    public static Quaternion? OverrideRotation;

    [SerializeField] private Transform startingPoint;

    private void Start()
    {
        if (OverridePosition.HasValue)
        {
            PlacePlayerAt(OverridePosition.Value, OverrideRotation ?? Quaternion.identity);
            OverridePosition = null;
            OverrideRotation = null;
        }
        else
        {
            PlacePlayer();
        }
    }

    private void PlacePlayerAt(Vector3 position, Quaternion rotation)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var cc = player.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.SetPositionAndRotation(position, rotation);
        if (cc != null) cc.enabled = true;
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
