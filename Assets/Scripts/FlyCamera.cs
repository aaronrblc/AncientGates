using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float fastMultiplier = 3f;
    public float sensitivity = 2f;

    float rotX, rotY;

    void Start()
    {
        rotX = transform.eulerAngles.x;
        rotY = transform.eulerAngles.y;
    }

    void Update()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;

        if (mouse == null || kb == null) return;

        if (mouse.rightButton.isPressed)
        {
            Cursor.lockState = CursorLockMode.Locked;

            Vector2 delta = mouse.delta.ReadValue() * sensitivity * 0.1f;
            rotY += delta.x;
            rotX -= delta.y;
            rotX = Mathf.Clamp(rotX, -89f, 89f);

            transform.rotation = Quaternion.Euler(rotX, rotY, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        float speed = moveSpeed * (kb.leftShiftKey.isPressed ? fastMultiplier : 1f);

        Vector3 dir = Vector3.zero;
        if (kb.wKey.isPressed) dir += transform.forward;
        if (kb.sKey.isPressed) dir -= transform.forward;
        if (kb.dKey.isPressed) dir += transform.right;
        if (kb.aKey.isPressed) dir -= transform.right;
        if (kb.eKey.isPressed) dir += transform.up;
        if (kb.qKey.isPressed) dir -= transform.up;

        transform.position += dir * speed * Time.deltaTime;
    }
}
