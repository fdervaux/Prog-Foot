using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    [SerializeField, Range(0, 10)]
    private float _moveSpeed = 3f;

    [SerializeField, Range(0, 100)]
    private float _maxMoveAcceleration = 50;

    [SerializeField]
    private FloorSensor _floorSensor;

    [SerializeField]
    private float _step = 0.3f;

    [SerializeField]
    private float _height = 2.0f;

    [SerializeField]
    private float _radius = 0.5f;

    [SerializeField]
    private float _gravity = -9.81f;

    [SerializeField]
    private float _jumpHeight = 1.5f;

    [SerializeField, Range(0, 1)]
    private float _AirFriction = 1;

    [SerializeField, Range(0, 1)]
    private float _drag = 0.01f;


    private PlayerInput _playerInput;
    private Vector2 _deltaMove = Vector2.zero;
    private Vector2 _lastDeltaMove = Vector2.zero;
    private Vector2 _deltaMoveSmoothDampVelocity = Vector2.zero;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private CapsuleCollider _capsuleCollider;
    private bool _perfomedJump = false;
    private Vector3 _groundVelocity = Vector3.zero;
    private Vector3 _momentum = Vector3.zero;
    private Vector3 _horizontalMove = Vector3.zero;
    private Vector3 _groundCorrection = Vector3.zero;
    private bool _isOnGround = false;
    private FloorDetection _floorDetection;
    private float _slopeAngle = 0; //rad

    private InputAction _move;

    [SerializeField]
    private Transform _camera;

    public void OnJump()
    {
        Debug.Log("jump");
        _perfomedJump = true;
       
    }

    public void OnTackle()
    {
        Debug.Log("tackle");
        _animator.SetTrigger("tackle");
    }



    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _move = _playerInput.actions.FindAction("move");

        calibrateSensor();
        calibrateCollider();
    }

    public void calibrateSensor()
    {
        _floorSensor.init(transform);
        //add 0.01 to avoid sensor on ground 
        _floorSensor.SetOffset(new Vector3(0, _step + 0.01f, 0));
        _floorSensor.SetCastLength(_step * 3);
    }

    public void calibrateCollider()
    {
        _capsuleCollider.height = _height - _step;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.height / 2 + _step, 0);
        _capsuleCollider.radius = _radius;
    }


    private void handleGround()
    {
        // On genere les infos du sol
        _floorSensor.Cast();
        _floorDetection = _floorSensor.GetFloorDetection();

        _isOnGround = false;

        if (_floorDetection.detectGround)
        {
            // On calcule la velocité vertical en fonction de la normal au sol
            float YVelocity = Vector3.Project(_momentum, _floorDetection.hitNormal).magnitude;

            // on determine si on est au sol en fonction de la celocité et de _floorDetection
            _isOnGround = YVelocity < 6f || _floorDetection.floorDistance < 0;

            // On calcule l'angle d u sol
            _slopeAngle = Vector3.Angle(_floorDetection.hitNormal, transform.up) * Mathf.Deg2Rad;
        }
        _animator.SetBool("isOnGround", _isOnGround);
        // On calcule la ground Correction
        _groundCorrection = Vector3.zero;
        if (_isOnGround)
            _groundCorrection = (-_floorDetection.floorDistance / Time.fixedDeltaTime) * transform.up;
    }

    private void computeMovement()
    {
        Vector2 inputMove = _move.ReadValue<Vector2>();
        Vector3 move = new Vector3(inputMove.x, 0f, inputMove.y);


        if(move.magnitude >= 0.1f)
        {
            float angle = Vector3.SignedAngle(Vector3.forward, move, Vector3.up) + _camera.eulerAngles.y ;

            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            _horizontalMove = moveDirection * move.magnitude;

        }
        else
        {
            _horizontalMove = Vector3.zero;
        }


    }

    private void computeMomentum()
    {
        // Cette fonction permet de gerer l'élan du joueur (momentum)

        Vector3 _verticalMomentum = Vector3.zero;
        Vector3 _horizontalMomentum = Vector3.zero;

        // On divise l'élan en un elan verticale et un élan horizontale
        if (_isOnGround)
            _verticalMomentum = Vector3.Project(_momentum, _floorDetection.hitNormal);
        else
            _verticalMomentum = Vector3.Project(_momentum, transform.up);
            
        _horizontalMomentum = _momentum - _verticalMomentum;

        // Si je suis au sol
        if (_isOnGround)
        {

            // On reset l'élan verticale
            _verticalMomentum = Vector3.zero;
            // On définit le nouveau élan
            _momentum = _horizontalMomentum;

            // On calcule la friction 
            float dynamicFriction = _floorDetection.collider.material.dynamicFriction;
            float frictionAttenuation = _maxMoveAcceleration * dynamicFriction * Mathf.Cos(_slopeAngle);

            Vector3 horizontalMoveOnGround = Vector3.ProjectOnPlane(_horizontalMove, _floorDetection.hitNormal);

            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction au sol 
            // Permet d'avoir des sols glissant sur lequels on patine 
            _momentum = Vector3.MoveTowards(_momentum, horizontalMoveOnGround * _moveSpeed, frictionAttenuation * Time.fixedDeltaTime);
        }
        else //sinon (on est dans les airs)
        {
            // On ajoute la gravité
            _verticalMomentum += _gravity * Time.fixedDeltaTime * transform.up;

            // On ajoute de la friction dans l'air 
            float airFriction = _maxMoveAcceleration * _AirFriction;
            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction dans l'air (airControl)
            _horizontalMomentum = Vector3.MoveTowards(_horizontalMomentum, _horizontalMove * _moveSpeed, airFriction * Time.fixedDeltaTime);

            // On calcule le moment complet
            _momentum = _horizontalMomentum + _verticalMomentum;

            // On ajuste la vitess avec un drag ( cela permet de ne pas atteindre des vitesses extrême )
            _momentum *= Mathf.Max(0, 1 - _drag * Time.fixedDeltaTime);
        }
    }

    private void tryJump()
    {
        // Cette fonction permet de gerer le saut

        // Si il y a l'input du saut et que je suis au sol et que je ne suis pas en train de glisser, je fais le saut
        if (_perfomedJump && _isOnGround)
        {
            _animator.SetTrigger("jump");

            //calcule de la vitesse de saut pour atteindre une certaine hauteur en fonction de la gravité
            float jumpVelocity = Mathf.Max(0, _groundVelocity.y) + Mathf.Sqrt(2 * _jumpHeight * -_gravity);

            // si mon moment vertical est inférieur j'ajoute le reste de vitesst pour atteindre la vitesse de saut maximal
            if (Vector3.Dot(_momentum, transform.up) < jumpVelocity)
            {
                _momentum = _momentum - Vector3.Project(_momentum, transform.up);
                _momentum += transform.up * jumpVelocity;
            }

            // si je saute je ne suis plus au sol
            _isOnGround = false;
        }
    }

    private void FixedUpdate()
    {
        

        // Calcule le momentum en fonction du mouvement précédent et retranche le groundCorrection  
        _momentum = _rigidbody.velocity - _groundCorrection;

        // Si je suis au sol retire le mouvement Vertical en cas de collision
        if (_isOnGround)
            _momentum = _momentum - Vector3.Project(_momentum, _floorDetection.hitNormal);

        handleGround();
        computeMovement();
        computeMomentum();
        tryJump();

        _animator.SetFloat("speed", _momentum.magnitude / _moveSpeed);

        //applique le momentum et la groundCorection
        _rigidbody.velocity = _momentum + _groundCorrection;


        // ne pas oublier de mettre _perfomedJump à false
        _perfomedJump = false;
    }

}