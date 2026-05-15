using UnityEngine;

public class WaypointPatrol : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 1.4f;
    [SerializeField] private float arriveDistance = 0.2f;
    [SerializeField] private float waitAtEachPoint = 0f;

    private Transform _target;
    private float _waitTimer;

    private void Start()
    {
        _target = pointB;
    }

    private void Update()
    {
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude <= arriveDistance)
        {
            _target = _target == pointA ? pointB : pointA;
            _waitTimer = waitAtEachPoint;
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            360f * Time.deltaTime
        );
    }
}
