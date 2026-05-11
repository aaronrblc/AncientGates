using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FpsCameraController : MonoBehaviour
{
    [SerializeField] private float defaultFov = 70f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = defaultFov;
    }

    // TODO: shake, FOV lerp al sprintar, efectos de cámara
}
