using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{

    [SerializeField]
    private float _yLimit = -50f;

    private Vector3 startPosition = Vector3.zero;

    private Rigidbody _rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Player"));

        if (hitColliders.Length > 0)
        {
            if(hitColliders[0].transform.GetComponent<ControlPlayer>() != null)
                hitColliders[0].transform.GetComponent<ControlPlayer>().catchTheBall();

            if (hitColliders[0].transform.GetComponent<AICharacterControler>() != null)
                hitColliders[0].transform.GetComponent<AICharacterControler>().catchTheBall();
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < _yLimit)
        {
            _rigidBody.position = startPosition;
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
            _rigidBody.rotation = Quaternion.identity;
        }
    }
}
