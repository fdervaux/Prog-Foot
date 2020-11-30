using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterControler : MonoBehaviour
{
    private NavMeshAgent _navemeshAgent;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private float smoothAngleVelocity = 0;

    private MovePlayer _movePlayer;

    private bool haveBall = false;

    [SerializeField]
    private float _speed = 10;

    [SerializeField]
    private Transform _ball;

    [SerializeField]
    private Transform _goal;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private Animator _AiAnimator;

    [SerializeField]
    private float distanceMinToThrowBall = 4;

    [SerializeField]
    private float distanceMinToTackled = 2;

    [SerializeField]
    private GameObject _localBall;

    [SerializeField]
    private float dropBallDuration = 1;

    private float startTimeDropBall = 0;

    private Transform _target = null;


    public void setTargetToBall()
    {
        _target = _ball;
    }

    public void setTargetToGoal()
    {
        _target = _goal;
    }

    public void setTargetToNull()
    {
        _target = null;
    }

    public void catchTheBall()
    {
        if (Time.time > startTimeDropBall + dropBallDuration)
        {
            _ball.gameObject.SetActive(false);
            _localBall.SetActive(true);
            _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 1.0f);
        }
    }

    public void startThrowBall()
    {
        _animator.SetTrigger("throw");
    }

    public void throwBall()
    {
        _localBall.SetActive(false);
        _ball.transform.position = _localBall.transform.position;
        _ball.gameObject.SetActive(true);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        Vector3 shootDirection = (_goal.position - transform.position).normalized;
        _ball.GetComponent<Rigidbody>().AddForce(shootDirection * 25 + transform.up * 5f, ForceMode.VelocityChange);
        _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 0.0f);
        startTimeDropBall = Time.time;
    }

    public void EndAction()
    {
        _AiAnimator.SetTrigger("endAction");
    }


    // Start is called before the first frame update
    void Start()
    {
        _navemeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _movePlayer = GetComponent<MovePlayer>();
        _navemeshAgent.updatePosition = false;
        _navemeshAgent.updateRotation = false;
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 displacement = Vector3.zero;
        if(_target != null)
        {
            _navemeshAgent.isStopped = false;

            if(_navemeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                _navemeshAgent.destination = _target.position;

                Vector3 direction = (_navemeshAgent.steeringTarget - _rigidbody.position).normalized;

                if (_navemeshAgent.remainingDistance > 0.1f)
                {
                    displacement = Vector3.ProjectOnPlane(direction, transform.up).normalized;
                }

                _navemeshAgent.nextPosition = _rigidbody.position + _rigidbody.velocity * Time.fixedDeltaTime;

                //rotation
                Quaternion lookSteering = Quaternion.LookRotation(_navemeshAgent.steeringTarget - transform.position, transform.up);
                float targetAngle = lookSteering.eulerAngles.y;
                float currentAngle = transform.rotation.eulerAngles.y;

                currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref smoothAngleVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            }
        } 
        else
        {
            _navemeshAgent.isStopped = true;
            displacement = Vector3.zero;
        }

        _movePlayer.computeMovement(displacement, _speed);

        Vector3 _horizontalMove = Vector3.ProjectOnPlane(_rigidbody.velocity, transform.up);

        _animator.SetFloat("speed", _horizontalMove.magnitude);

        Vector3 distanceToPlayer = _player.position - transform.position;
        Vector3 distanceToGoal = _goal.position - transform.position;

        _AiAnimator.SetBool("isNearToGoal", distanceToGoal.magnitude < distanceMinToThrowBall);
        _AiAnimator.SetBool("characterIsNear", distanceToPlayer.magnitude < distanceMinToThrowBall);
        _AiAnimator.SetBool("ballIsFree", _ball.gameObject.activeSelf);
        _AiAnimator.SetBool("haveCatchBall", _localBall.gameObject.activeSelf);

    }
}
