using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class ControlPlayer : MonoBehaviour
{

    private PlayerInput _playerInput;
    private InputAction _move;
    private MovePlayer _movePlayer;
    private Animator _animator;

    private Rigidbody _rigidbody;

    private bool _startJump = false;
    private bool _startTackle = false;
    private bool _startTrip = false;

    private bool _isTackle = false;
    private bool _isTrip = false;

    private Vector3 _tripDirection = Vector3.zero;
    private float _startTimeAction = 0f;

    [SerializeField, Range(0, 20)]
    private float _moveSpeed = 3f;

    [SerializeField, Range(0, 5)]
    private float tackleDuration = 1;

    [SerializeField, Range(0, 5)]
    private float tripDuration = 1;

    [SerializeField, Range(0, 20)]
    private float _tackleSpeed = 15;

    [SerializeField, Range(0, 20)]
    private float _tripSpeed = 15;

    [SerializeField]
    private float _jumpHeight = 1.5f;

    [SerializeField]
    private GameObject _vcam;

    [SerializeField]
    private GameObject _upCamera;

    private Vector3 _startPosition;
    private Quaternion _startRotation;

    [SerializeField]
    private float _yLimit = -20;

    [SerializeField]
    private GameObject _localBall;

    [SerializeField]
    private GameObject _ball;

    [SerializeField]
    private float dropBallDuration = 1;

    private float startTimeDropBall = 0;


    public void catchTheBall()
    {
        if (Time.time > startTimeDropBall + dropBallDuration)
        {
            Debug.Log("touch Ball");
            _ball.gameObject.SetActive(false);
            _localBall.SetActive(true);
            _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 1.0f);
        }
    }

    private void throwBall()
    {
        _localBall.SetActive(false);
        _ball.transform.position = _localBall.transform.position + transform.forward * 1;
        _ball.SetActive(true);
        _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        _ball.GetComponent<Rigidbody>().AddForce(
            _movePlayer._camera.transform.forward * 25 + _movePlayer._camera.transform.up * 5f, 
            ForceMode.VelocityChange);
        _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 0.0f);
        startTimeDropBall = Time.time;
    }

    public void OnJump()
    {
        Debug.Log("jump");
        _startJump = true;
    }

    public void OnTackle()
    {
        Debug.Log("tackle");
        _startTackle = true;
    }

    public void OnTrip()
    {
        Debug.Log("trip");
        _startTrip = true;

        _tripDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        _tripDirection.Normalize();
    }

    public void OnThrow()
    {
        _animator.SetTrigger("throw");
    }

    public bool tryTackle()
    {
        if (_startTackle && !_isTackle && _movePlayer.isOnGround())
        {
            _isTackle = true;
            _animator.SetTrigger("tackle");
            _startTimeAction = Time.time;
        }

        if (_isTackle && _movePlayer.isOnGround())
        {
            if (Time.time > _startTimeAction + tackleDuration)
            {
                _isTackle = false;
                return false;
            }

            _movePlayer.computeFromCameraMovement(Vector3.forward, _tackleSpeed);
            return true;
        }

        _isTackle = false;
        return false;
    }

    public bool tryTrip()
    {
        if (_startTrip && !_isTrip)
        {
            _isTrip = true;
            _animator.SetTrigger("trip");
            _startTimeAction = Time.time;

            if(_localBall.activeSelf)
            {
                _localBall.SetActive(false);
                _ball.transform.position = _localBall.transform.position + transform.forward * 1;
                _ball.SetActive(true);
                _ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
                _ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                _ball.GetComponent<Rigidbody>().AddForce(transform.up * 10f, ForceMode.VelocityChange);
                _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 0.0f);
                startTimeDropBall = Time.time;
            }
        }

        if (_isTrip)
        {
            if (Time.time > _startTimeAction + tripDuration)
            {
                _isTrip = false;
                return false;
            }

            Debug.Log(_tripDirection);
            _movePlayer.computeFromCameraMovement(_tripDirection, _tripSpeed);
            return true;
        }

        _isTrip = false;
        return false;
    }

    public bool tryJump()
    {
        if (_startJump  && _movePlayer.isOnGround())
        {
            _animator.SetTrigger("jump");
            _movePlayer.jump(_jumpHeight);

        }

        return false;
    }

    void Awake()
    {
        _movePlayer = GetComponent<MovePlayer>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _move = _playerInput.actions.FindAction("move");
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    

    // Fixed is called once per frame
    void FixedUpdate()
    {
        if (_upCamera.activeSelf)
        {
            _upCamera.SetActive(false);
            _vcam.SetActive(true);
        }

        Vector2 inputMove = _move.ReadValue<Vector2>();
        Vector3 move = new Vector3(inputMove.x, 0f, inputMove.y);

        if (transform.position.y < _yLimit)
        {
            Debug.Log("test");
            _rigidbody.position = _startPosition;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.rotation = _startRotation;
            _vcam.SetActive(false);
            CinemachineFreeLook freelook = _vcam.GetComponent<CinemachineFreeLook>();
            freelook.m_XAxis.Value = 0;
            freelook.m_YAxis.Value = 0.5f;
            _upCamera.SetActive(true);

            if (_localBall.activeSelf)
            {
                _localBall.SetActive(false);
                _ball.transform.position = _localBall.transform.position;
                _animator.SetLayerWeight(_animator.GetLayerIndex("catch ball"), 0.0f);
                _ball.SetActive(true);
            }
        }
        else
        {
            bool performedAction = tryTackle();
            if (!performedAction) performedAction = tryTrip();
            if (!performedAction) performedAction = tryJump();

            if (!performedAction)
            {
                _animator.SetFloat("speed", _movePlayer.getHorizontalMomentum().magnitude / _moveSpeed);
                _movePlayer.computeFromCameraMovement(move, _moveSpeed);
            }
        }

        _startJump = false;
        _startTackle = false;
        _startTrip = false;
    }
}
