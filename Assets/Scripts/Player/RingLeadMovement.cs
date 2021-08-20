using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ScriptableObjects.Player;
using Player;
using System.Linq;

public class RingLeadMovement : MonoBehaviour
{
    [SerializeField] private PlayerData _data;
    [SerializeField] private RotatePlatforms _rotatePlatforms;

    [SerializeField] private float _startMoveInterval;

    [SerializeField] private CameraFollow _cameraFollow;

    private PlayerInitializer _playerInitializer;
    
    private Rigidbody _rigidbody;
    private Collider _collider;
    private bool _onGround;
    private int _offset;
    
    private AnimationCurve _animationCurve => _data._animationCurve;
    private float _time => _data._time;
    private float _height => _data._height;
    private float _gravity => _data._gravity;
    private int _distance => _data._distance;

    public event Action Grounded;
    public event Action LeadSwitching;
    public event Action<Collider> Addendum;
    public event Action<Collider> Removal;
    public event Action<Collision> Final;

    private static bool DelaysReversed;

    public void Init()
    {
        _playerInitializer = GetComponentInParent<PlayerInitializer>();
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _rigidbody.useGravity = true;
        _offset = _data._offset;
    }

    private void Launch()
    {
        _playerInitializer.FirstTimeLaunchedRing = true;

        Physics.gravity = Vector3.up * _gravity;
        _rigidbody.useGravity = true;
        
        if (_rotatePlatforms.NormalizedOffsetX > 0)
            _offset = -1;
        else if (_rotatePlatforms.NormalizedOffsetX < 0)
            _offset = 1;
        else if (_rotatePlatforms.NormalizedOffsetX == 0)
            _offset = 1;
        _rigidbody.velocity = CalculateLaunchData().initialVelocity;

        if (transform.CompareTag("StartPart") == true && DelaysReversed == false)
        {
            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                _playerInitializer.SlinkyParts[i].StartMoveRingToHead();
            }
        }
        else if (transform.CompareTag("StartPart") == false)
        {
            List<float> reversedDelays = new List<float>();

            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                reversedDelays.Add(_playerInitializer.SlinkyParts[i].DelayBeforeMove);
            }

            reversedDelays.Reverse();

            for (int i = _playerInitializer.SlinkyParts.Count - 1; i >= 0; i--)
            {
                _playerInitializer.SlinkyParts[i].DelayBeforeMove = reversedDelays[i];
            }

            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                _playerInitializer.SlinkyParts[i].StartMoveRingToHead();
            }

            DelaysReversed = true;
        }
        else if (transform.CompareTag("StartPart") == true && DelaysReversed == true)
        {
            List<float> directDelays = new List<float>();

            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                directDelays.Add(_playerInitializer.SlinkyParts[i].DelayBeforeMove);
            }

            directDelays.Reverse();

            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                _playerInitializer.SlinkyParts[i].DelayBeforeMove = directDelays[i];
            }

            for (int i = 0; i < _playerInitializer.SlinkyParts.Count; i++)
            {
                _playerInitializer.SlinkyParts[i].StartMoveRingToHead();
            }

            DelaysReversed = false;
        }
    }

    private LaunchData CalculateLaunchData()
    {
        float displacementY = transform.position.y - _rigidbody.position.y;
        Vector3 displacementXZ =
            new Vector3(transform.position.x + (_distance*_offset)  - _rigidbody.position.x, 0, transform.position.z - _rigidbody.position.z);
        float time = Mathf.Sqrt(-2 * _height / _gravity) + Mathf.Sqrt(2 * (displacementY - _height) / _gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity * _height);
        Vector3 velocityXZ = displacementXZ / time;

        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(_gravity), time);
    }
    
    private readonly struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Finish"))
        {
            Final?.Invoke(coll);
        }
        
        if (!coll.gameObject.CompareTag("Ground") || _onGround) return;

        _onGround = true;
        var rot = coll.gameObject.transform.rotation;
        var rotationZ = rot.eulerAngles.z - transform.rotation.eulerAngles.z;
        transform.DOLocalRotateQuaternion(
            coll.gameObject.transform.rotation * Quaternion.Euler(0, 0, rotationZ), 0.1f);

        StartCoroutine(ReverseFollowSystemWithDelay(_startMoveInterval));
        Grounded?.Invoke();
    }

    public void StartJump()
    {
        StartCoroutine(Jump(_startMoveInterval));
    }
    
    private IEnumerator Jump(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetLeadStatus(true);
        StartCoroutine(MoveAndRotate());
        Launch();
    }

    private IEnumerator ReverseFollowSystemWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LeadSwitching?.Invoke();
        SetLeadStatus(false);
        _onGround = false;
    }

    private IEnumerator MoveAndRotate() 
    {
        var elapsed = 0f;
        var eulers = transform.eulerAngles;
            
        if (_rotatePlatforms.NormalizedOffsetX > 0)
            _offset = -1;
        else if (_rotatePlatforms.NormalizedOffsetX < 0)
            _offset = 1;
        else if (_rotatePlatforms.NormalizedOffsetX == 0)
            _offset = 1;

        while (elapsed < 1f)
        {
            elapsed += Time.fixedDeltaTime / _time;
            Quaternion rot = Quaternion.Euler(eulers.x, eulers.y,
                eulers.z + (_animationCurve.Evaluate(elapsed) * _offset));

            _rigidbody.rotation = Quaternion.Lerp(_rigidbody.rotation, rot, elapsed);
            yield return new WaitForFixedUpdate();
        }
    }

    private void SetLeadStatus(bool isHead)
    {
        _rigidbody.isKinematic = !isHead;
        _collider.enabled = isHead;

        if (isHead)
        {
            GetComponent<FollowSystem>()._target = null;
            _cameraFollow.YFollowTarget = transform;
        }
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Ring"))
        {
            Addendum?.Invoke(coll);
        }
        
        else if (coll.CompareTag("Obstacle"))
        {
            if (coll.GetComponent<Obstacle>().isActive)
            {
                Removal?.Invoke(coll);
                coll.GetComponent<Obstacle>().isActive = false;
            }
        }
    }
    
}