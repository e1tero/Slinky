using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class FollowSystem : MonoBehaviour
{
    [SerializeField] public Transform _target;
    [SerializeField] private float _smoothSpeed = 0.125f;
    [SerializeField] private float _delayBeforeMove;
    public float DelayBeforeMove { get { return _delayBeforeMove; } set { _delayBeforeMove = value; } }
    [SerializeField] private Vector3 _offset;

    private PlayerInitializer _playerInitalizer;

    private Vector3 _targetPosition;

    private Coroutine _moveToHeadCoroutine;

    private bool _ringStopped;

    private void Start()
    {
        _playerInitalizer = GetComponentInParent<PlayerInitializer>();
    }

    private void FixedUpdate()
    {
        // It's only for start rings falling
        if (_playerInitalizer.FirstTimeLaunchedRing == false)
        {
            Move();
        }
    }

    public void StartMoveRingToHead()
    {
        if (_moveToHeadCoroutine != null)
        {
            StopCoroutine(_moveToHeadCoroutine);
            _moveToHeadCoroutine = null;
        }

        _moveToHeadCoroutine = StartCoroutine(MoveRingToHead());
    }

    private void Move()
    {
        if (_target != null && transform.position != _targetPosition)
        {
            Vector3 _targetPosition = _target.position + _offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, _targetPosition, _smoothSpeed);
            Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, _target.rotation, _smoothSpeed);

            transform.position = smoothedPosition;
            transform.rotation = smoothedRotation;
        }
        else
        {
            _ringStopped = true; // It's used for MoveRingToHead coroutine as the end state
        }
    }

    private IEnumerator MoveRingToHead()
    {
        _ringStopped = false;

        yield return new WaitForSeconds(_delayBeforeMove);

        yield return new WaitForFixedUpdate();

        while (_ringStopped == false)
        {
            Move();

            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForFixedUpdate();
    }
}
