using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    [SerializeField] private Vector3 degreesPerSecond = new(0f, 90f, 0f);

    private void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
}
