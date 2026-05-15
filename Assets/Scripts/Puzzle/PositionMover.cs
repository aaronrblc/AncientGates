using System.Collections;
using UnityEngine;

public class PositionMover : MonoBehaviour
{
    [SerializeField] private Vector3 offset = Vector3.right * 2f;
    [SerializeField] private float duration = 0.4f;

    private Vector3 _basePosition;
    private Vector3 _activePosition;
    private Coroutine _current;

    private void Awake()
    {
        _basePosition = transform.localPosition;
        _activePosition = _basePosition + offset;
    }

    public void Activate()   => Move(_activePosition);
    public void Deactivate() => Move(_basePosition);

    private void Move(Vector3 target)
    {
        if (_current != null) StopCoroutine(_current);
        _current = StartCoroutine(Slide(target));
    }

    private IEnumerator Slide(Vector3 target)
    {
        Vector3 start = transform.localPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localPosition = target;
    }
}
