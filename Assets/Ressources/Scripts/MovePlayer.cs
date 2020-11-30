using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MovePlayer : MonoBehaviour
{

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

    [SerializeField, Range(0, 1)]
    private float _AirFriction = 1;

    [SerializeField, Range(0, 1)]
    private float _drag = 0.01f;


    private Rigidbody _rigidbody;
    private Animator _animator;
    private CapsuleCollider _capsuleCollider;
    private Vector3 _momentum = Vector3.zero;
    private Vector3 _horizontalMove = Vector3.zero;
    private Vector3 _groundCorrection = Vector3.zero;
    private bool _isOnGround = false;
    private FloorDetection _floorDetection;

    [SerializeField]
    public Transform _camera;
    private float _smoothAngleSpeed = 0;
    private Vector3 _forceToAdd = Vector3.zero;


    public bool isOnGround()
    {
        return _isOnGround;
    }

    void Awake()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

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

        Debug.Log("test");

        if (_floorDetection.detectGround)
        {
            Debug.Log("test1");
            if(_isOnGround)
            {
                if(_forceToAdd.y > 2f)
                    _isOnGround = false;
            }
            else
            {
                if(_floorDetection.floorDistance < 0.1f )
                    _isOnGround = true;
            }
        }
        else
        {
            Debug.Log("test2");
            _isOnGround = false;
        }

        _animator.SetBool("isOnGround", _isOnGround);
        // On calcule la ground Correction
        _groundCorrection = Vector3.zero;
        if (_isOnGround)
            _groundCorrection = (-_floorDetection.floorDistance / Time.fixedDeltaTime) * transform.up;
    }

    public void computeFromCameraMovement(Vector3 move, float speed)
    {
        if (move.magnitude >= 0.1f)
        {
            float targetAngle = Vector3.SignedAngle(Vector3.forward, move, Vector3.up) + _camera.eulerAngles.y;
            float angle = transform.eulerAngles.y;

            angle = Mathf.SmoothDampAngle(angle, targetAngle, ref _smoothAngleSpeed, 0.2f);

            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            _horizontalMove = moveDirection * move.magnitude * speed;

        }
        else
        {
            _horizontalMove = Vector3.zero;
        }
    }

    public void computeMovement(Vector3 move, float speed)
    {
        if (move.magnitude >= 0.1f)
        {
            _horizontalMove = move * speed;
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
            float frictionAttenuation = _maxMoveAcceleration * dynamicFriction;

            Vector3 horizontalMoveOnGround = Vector3.ProjectOnPlane(_horizontalMove, _floorDetection.hitNormal);

            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction au sol 
            // Permet d'avoir des sols glissant sur lequels on patine 
            _momentum = Vector3.MoveTowards(_momentum, horizontalMoveOnGround, frictionAttenuation * Time.fixedDeltaTime);
        }
        else //sinon (on est dans les airs)
        {
            // On ajoute la gravité
            _verticalMomentum += _gravity * Time.fixedDeltaTime * transform.up;

            // On ajoute de la friction dans l'air 
            float airFriction = _maxMoveAcceleration * _AirFriction;
            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction dans l'air (airControl)
            _horizontalMomentum = Vector3.MoveTowards(_horizontalMomentum, _horizontalMove, airFriction * Time.fixedDeltaTime);

            // On calcule le moment complet
            _momentum = _horizontalMomentum + _verticalMomentum;

            // On ajuste la vitess avec un drag ( cela permet de ne pas atteindre des vitesses extrême )
            _momentum *= Mathf.Max(0, 1 - _drag * Time.fixedDeltaTime);
        }
    }

    public void jump(float height)
    {
        //calcule de la vitesse de saut pour atteindre une certaine hauteur en fonction de la gravité
        float jumpVelocity = Mathf.Sqrt(2 * height * -_gravity);
        _forceToAdd = new Vector3(0,jumpVelocity,0);
    }


    public Vector3 getHorizontalMomentum()
    {
        return Vector3.ProjectOnPlane(_momentum, _floorDetection.hitNormal);
    }


    private void FixedUpdate()
    {

        // Calcule le momentum en fonction du mouvement précédent et retranche le groundCorrection  
        _momentum = _rigidbody.velocity - _groundCorrection;

        // Si je suis au sol retire le mouvement Vertical en cas de collision
        if (_isOnGround)
            _momentum = _momentum - Vector3.Project(_momentum, _floorDetection.hitNormal);




        handleGround();

        _momentum += _forceToAdd;
        _forceToAdd = Vector3.zero;

        computeMomentum();

        //applique le momentum et la groundCorection
        _rigidbody.velocity = _momentum + _groundCorrection;
    }

}