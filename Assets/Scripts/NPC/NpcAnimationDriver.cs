using UnityEngine;

public class NpcAnimationDriver : MonoBehaviour
{
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold  = 3.0f;

    [Header("Look At Player")]
    [SerializeField] private float lookAtDistance    = 5f;
    [SerializeField] private float lookAtSmoothing   = 5f;
    [SerializeField] private float lookBodyWeight    = 0.15f;
    [SerializeField] private float lookHeadWeight    = 0.8f;
    [SerializeField] private float lookClampWeight   = 0.5f;

    private Animator _animator;
    private Vector3  _previousPosition;
    private float    _smoothedSpeed;
    private Transform _playerTransform;
    private float    _currentLookWeight;

    private static readonly int MoveSpeed          = Animator.StringToHash("MoveSpeed");
    private static readonly int CurrentGait        = Animator.StringToHash("CurrentGait");
    private static readonly int IsGrounded         = Animator.StringToHash("IsGrounded");
    private static readonly int IsStopped          = Animator.StringToHash("IsStopped");
    private static readonly int IsWalking          = Animator.StringToHash("IsWalking");
    private static readonly int MovementInputHeld  = Animator.StringToHash("MovementInputHeld");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _previousPosition = transform.position;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    private void Start()
    {
        UpdateAnimator(0f);
        _animator.Play("Idle_Standing", 0, 0f);
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) _playerTransform = player.transform;
        }

        float rawSpeed = Vector3.Distance(transform.position, _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, rawSpeed, Time.deltaTime * 5f);
        UpdateAnimator(_smoothedSpeed);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_playerTransform == null) return;

        Vector3 eyePos    = transform.position + Vector3.up * 1.6f;
        Vector3 playerEye = _playerTransform.position + Vector3.up * 1.6f;
        Vector3 toPlayer  = playerEye - eyePos;

        bool canSee = toPlayer.magnitude <= lookAtDistance
                   && !Physics.Raycast(eyePos, toPlayer.normalized, toPlayer.magnitude);

        float targetWeight = canSee ? 1f : 0f;
        _currentLookWeight = Mathf.Lerp(_currentLookWeight, targetWeight, Time.deltaTime * lookAtSmoothing);

        _animator.SetLookAtPosition(playerEye);
        _animator.SetLookAtWeight(_currentLookWeight, lookBodyWeight, lookHeadWeight, 0f, lookClampWeight);
    }

    private void UpdateAnimator(float speed)
    {
        int gait;
        if      (speed < 0.05f)         gait = 0;
        else if (speed < walkThreshold) gait = 1;
        else if (speed < runThreshold)  gait = 2;
        else                            gait = 3;

        _animator.SetFloat(MoveSpeed,           speed);
        _animator.SetInteger(CurrentGait,       gait);
        _animator.SetBool(IsGrounded,           true);
        _animator.SetBool(IsStopped,            speed < 0.05f);
        _animator.SetBool(IsWalking,            gait == 1);
        _animator.SetBool(MovementInputHeld,    speed > 0.05f);
    }
}
