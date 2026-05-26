using System.Collections;
using UnityEngine;

public class ScaleAnimator : MonoBehaviour
{
    [SerializeField] private Transform scaleArm;
    [SerializeField] private Transform leftWeight;
    [SerializeField] private Transform rightWeight;
    [SerializeField] private float tiltAngle = 30f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Quaternion _armStart;
    private Quaternion _armTarget;
    private Quaternion _leftWorldRot;
    private Quaternion _rightWorldRot;

    private void Awake()
    {
        _armStart = scaleArm.localRotation;
        _armTarget = _armStart * Quaternion.Euler(0, 0, -tiltAngle);
        _leftWorldRot = leftWeight.rotation;
        _rightWorldRot = rightWeight.rotation;
    }

    public void Activate() => StartCoroutine(TiltRoutine());

    private IEnumerator TiltRoutine()
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / duration, 1f);
            scaleArm.localRotation = Quaternion.Lerp(_armStart, _armTarget, curve.Evaluate(t));
            yield return null;
        }
    }

    // LateUpdate se ejecuta después de que el brazo ya rotó:
    // forzar la rotación mundial de los pesos anula la rotación heredada del brazo.
    private void LateUpdate()
    {
        leftWeight.rotation = _leftWorldRot;
        rightWeight.rotation = _rightWorldRot;
    }
}
