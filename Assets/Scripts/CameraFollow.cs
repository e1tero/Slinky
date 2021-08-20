using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private Vector3 _offset;

    [SerializeField] private Transform _yFollowTarget;
    public Transform YFollowTarget { get { return _yFollowTarget; } set { _yFollowTarget = value; } }
    [SerializeField] private float _distanceAfterFollowY;
   
    void FixedUpdate()
    {
        Vector3 desiredPosition = _target.position + _offset;

        if (transform.position.y - _yFollowTarget.position.y < _distanceAfterFollowY)
        {
            desiredPosition.y = transform.position.y;
        }
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);
        transform.position = smoothedPosition;
    }
}    

