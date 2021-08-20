using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Extensions;
using UnityEngine;
using System.Linq;

namespace Player
{
    public class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _ringCollectEffect;
        [SerializeField] private ParticleSystem _finalFireworkEffect;
        [SerializeField] private GameObject _finalUI;
        [SerializeField] private FollowSystem _ringPrefab;
        [SerializeField] private PlayerView _view;
        [SerializeField] private List<FollowSystem> _parts;
        public List<FollowSystem> SlinkyParts => _parts;
        [SerializeField] private RingLeadMovement _head;
        [SerializeField] private RingLeadMovement _tail;

        [SerializeField] private bool _isHead;

        [SerializeField] private float _ringsDelayDelta;

        private bool _firstTimeLaunchedRing;
        public bool FirstTimeLaunchedRing { get { return _firstTimeLaunchedRing; } set { _firstTimeLaunchedRing = value; } }

        private void Awake()
        {
            _firstTimeLaunchedRing = false;

            List<FollowSystem> followSystemsReversed = new List<FollowSystem>();

            for (int i = 0; i < _parts.Count; i++)
            {
                followSystemsReversed.Add(_parts[i]);
            }

            followSystemsReversed.Reverse();

            for (int i = 0; i < followSystemsReversed.Count; i++)
            {
                followSystemsReversed[i].DelayBeforeMove = i * _ringsDelayDelta;
            }

            _firstTimeLaunchedRing = false;

            _view.Init(_parts);
            
            _head.Init();
            _tail.Init();
            
            EventSignature(_head, _tail);
        }

        private void EventSignature(RingLeadMovement head, RingLeadMovement tail)
        {
            head.Grounded += tail.StartJump;
            tail.Grounded += head.StartJump;
            
            head.LeadSwitching += ReverseFollowSystem;
            tail.LeadSwitching += ReverseFollowSystem;
            
            head.Addendum += Add;
            tail.Addendum += Add;
            
            head.Removal += Remove;
            tail.Removal += Remove;

            head.Final += Final;
            tail.Final += Final;
        }
        
        private void ReverseFollowSystem()
        {
            if (_isHead)
            {
                for (int i = 0; i < _parts.Count; i++)
                {
                    var index = (i + 1) % _parts.Count;
                    _parts[i]._target = _parts[index].transform;
                }
            }
            else
            {
                for (int i = _parts.Count - 1; i >= 0; i--)
                {
                    var index = i - 1;
                    index = index.ToCircleIndex(_parts.Count);
                    _parts[i]._target = _parts[index].transform;
                }
            }

            _isHead = !_isHead;
        }

        private void Final(Collision collision)
        {
            Instantiate(_finalFireworkEffect, collision.transform.position, Quaternion.identity);
            _finalUI.SetActive(true);
        }
        private void Add(Collider collider)
        {
            FollowSystem part = null;

            if (_isHead)
            {
                Instantiate(_ringCollectEffect, collider.gameObject.transform.position + new Vector3(0,1,0), Quaternion.identity);
                part = Instantiate(_ringPrefab, collider.transform.position, Quaternion.identity, transform);
                _parts.Add(part);
                FollowSystem lastRing = _parts[_parts.Count - 2];
                FollowSystem firstRing = _parts[0];

                Transform targetForAddedPart = null;

                if (lastRing._target == null)
                {
                    targetForAddedPart = firstRing._target;
                }
                else
                {
                    targetForAddedPart = lastRing._target;
                }

                part._target = targetForAddedPart;
                _parts[_parts.Count - 2] = part;
                _parts[_parts.Count - 1] = lastRing;
                _parts[_parts.Count - 1]._target = part.transform;
            }

            else
            {
                Instantiate(_ringCollectEffect, collider.gameObject.transform.position + new Vector3(0,1,0), Quaternion.identity);
                part = Instantiate(_ringPrefab, collider.transform.position, Quaternion.identity, transform);
                _parts.Insert(0,part);
               
                part._target = _parts[1]._target;
                _parts[1]._target = part.transform;
                var lastPart = _parts[1];
                _parts[0] = lastPart;
                _parts[1] = part;
            }
            
            Destroy(collider.gameObject);
            _view.Init(_parts);

            part.StartMoveRingToHead();
        }

        private void Remove(Collider collider)
        {
            for (int i = 0; i < _parts.Count - 1; i++)
            {
                var currentColor = _parts[i].GetComponentInChildren<MeshRenderer>().material.color;
                _parts[i].GetComponentInChildren<MeshRenderer>().material.DOColor(Color.white, 0.3f);
                StartCoroutine(NormalizeColorMaterial(_parts[i].gameObject,currentColor));
            }

            if (_isHead)
            {
                _parts[_parts.Count - 1]._target = _parts[_parts.Count - 2]._target;
                _parts[_parts.Count - 2]._target = null;
                _parts[_parts.Count - 2].gameObject.AddComponent<Rigidbody>()
                    .AddExplosionForce(200f, collider.transform.position - Vector3.up, 100f);
                _parts.Remove(_parts[_parts.Count - 2]);
            }
            
            else if (!_isHead)
            {
                _parts[0]._target = _parts[1]._target;
                _parts[1]._target = null;
                _parts[1].gameObject.AddComponent<Rigidbody>()
                    .AddExplosionForce(200f, collider.transform.position - Vector3.up, 100f);
                _parts.Remove(_parts[1]);
            }
        }
        
        private IEnumerator NormalizeColorMaterial(GameObject part, Color currentColor)
        {
            yield return new WaitForSeconds(0.3f);
            part.GetComponentInChildren<MeshRenderer>().material.DOColor(currentColor, 0.3f);
        }
    }
}