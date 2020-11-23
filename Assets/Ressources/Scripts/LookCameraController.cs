
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineFreeLook))]
public class LookCameraController : MonoBehaviour
{

    [Range(0f, 10f)] public float LookSpeed = 1f;
    public bool InvertY = false;
    private CinemachineFreeLook _freeLookComponent;
    private InputAction look;

    [SerializeField]
    private PlayerInput _playerInput;

    // Start is called before the first frame update
    void Start()
    {
        _freeLookComponent = GetComponent<CinemachineFreeLook>();
        look = _playerInput.actions.FindAction("look");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 lookMovement = look.ReadValue<Vector2>();

        if( lookMovement.magnitude > 0.1f)
        {
            _freeLookComponent.m_YAxisRecentering.CancelRecentering();
        }

        lookMovement.y = InvertY ? -lookMovement.y : lookMovement.y;

        // This is because X axis is only contains between -180 and 180 instead of 0 and 1 like the Y axis
        lookMovement.x = lookMovement.x * 180f;

        //Ajust axis values using look speed and Time.deltaTime so the look doesn't go faster if there is more FPS
        _freeLookComponent.m_XAxis.Value += lookMovement.x * LookSpeed * Time.deltaTime;
        _freeLookComponent.m_YAxis.Value += lookMovement.y * LookSpeed * Time.deltaTime;
    }
}
