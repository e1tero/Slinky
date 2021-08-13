using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSystem : MonoBehaviour
{
    [SerializeField] public Transform _target;
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private Vector3 _offset;

    void FixedUpdate()
    {
        if (_target == null) return;
        Vector3 desiredPosition = _target.position + _offset; 
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed);
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, _target.rotation, _smoothSpeed);
        
        transform.position = smoothedPosition;
        transform.rotation = smoothedRotation;

    }
}
