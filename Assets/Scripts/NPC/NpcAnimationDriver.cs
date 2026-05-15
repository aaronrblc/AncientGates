using UnityEngine;

public class NpcAnimationDriver : MonoBehaviour
{
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float runThreshold  = 3.0f;

    private Animator _animator;
    private Vector3  _previousPosition;
    private float    _smoothedSpeed;

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
    }

    private void Start()
    {
        UpdateAnimator(0f);
        _animator.Play("Idle_Standing", 0, 0f);
    }

    private void Update()
    {
        float rawSpeed = Vector3.Distance(transform.position, _previousPosition) / Time.deltaTime;
        _previousPosition = transform.position;
        _smoothedSpeed = Mathf.Lerp(_smoothedSpeed, rawSpeed, Time.deltaTime * 5f);
        UpdateAnimator(_smoothedSpeed);
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
