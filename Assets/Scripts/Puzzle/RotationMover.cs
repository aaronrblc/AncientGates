using System.Collections;
using UnityEngine;

public class RotationMover : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 rotationDelta = new(0f, 0f, 45f);
    [SerializeField] private float duration = 0.3f;

    private Quaternion _baseRotation;
    private Quaternion _activeRotation;
    private Coroutine _current;

    private void Awake()
    {
        if (target == null) target = transform;
        _baseRotation = target.localRotation;
        _activeRotation = _baseRotation * Quaternion.Euler(rotationDelta);
    }

    public void Activate()   => Move(_activeRotation);
    public void Deactivate() => Move(_baseRotation);

    private void Move(Quaternion target)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(Rotate(target));
    }

    private IEnumerator Rotate(Quaternion to)
    {
        Quaternion start = target.localRotation;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.localRotation = Quaternion.Slerp(start, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        target.localRotation = to;
    }
}
