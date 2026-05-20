using UnityEngine;

public class NpcController : MonoBehaviour
{
    public enum NpcState { Patrol, Chase }

    [Header("Detección")]
    [SerializeField] private float detectionDistance = 6f;
    [SerializeField] private float detectionEyeHeight = 1.6f;

    [Header("Persecución")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float losePlayerDistance = 10f;

    public NpcState State { get; private set; } = NpcState.Patrol;

    private WaypointPatrol _patrol;
    private Transform      _npcTransform;
    private Transform      _playerTransform;

    private void Awake()
    {
        _patrol       = GetComponentInChildren<WaypointPatrol>();
        _npcTransform = _patrol.transform;

        var player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Update()
    {
        if (_playerTransform == null) return;

        switch (State)
        {
            case NpcState.Patrol: UpdatePatrol(); break;
            case NpcState.Chase:  UpdateChase();  break;
        }
    }

    private void UpdatePatrol()
    {
        if (!HasLineOfSight()) return;

        State = NpcState.Chase;
        _patrol.enabled = false;
        NotificationQueue.SendMessage(new(NotificationType.NpcPlayerDetected, gameObject.name, "NpcController"));
    }

    private void UpdateChase()
    {
        float dist = Vector3.Distance(_npcTransform.position, _playerTransform.position);

        if (dist > losePlayerDistance)
        {
            State = NpcState.Patrol;
            _patrol.enabled = true;
            NotificationQueue.SendMessage(new(NotificationType.NpcPlayerLost, gameObject.name, "NpcController"));
            return;
        }

        Vector3 dir = _playerTransform.position - _npcTransform.position;
        dir.y = 0f;
        _npcTransform.position += dir.normalized * chaseSpeed * Time.deltaTime;
        _npcTransform.rotation = Quaternion.RotateTowards(
            _npcTransform.rotation,
            Quaternion.LookRotation(dir),
            360f * Time.deltaTime
        );
    }

    private bool HasLineOfSight()
    {
        Vector3 eye       = _npcTransform.position + Vector3.up * detectionEyeHeight;
        Vector3 playerEye = _playerTransform.position + Vector3.up * detectionEyeHeight;
        Vector3 toPlayer  = playerEye - eye;

        return toPlayer.magnitude <= detectionDistance
            && !Physics.Raycast(eye, toPlayer.normalized, toPlayer.magnitude);
    }
}
