using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class RotatePlatforms : MonoBehaviour
{
    [Header("General properties")]
    [SerializeField]
    private float maxSlideDistance;
    [SerializeField]
    private float minSlideDistance;
    
    [HideInInspector]
    public float NormalizedOffsetX;
    
    [Header("General properties")]
    [SerializeField]
    public float rotateSpeed = 3;
    [SerializeField]
    public float maxRotationAngle = 40;
    
    
    private Vector3 startTouchPosition;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Slide();
        Rotate();
    }
    private void Rotate()
    {
        transform.DORotate(NormalizedOffsetX * maxRotationAngle * Vector3.forward, rotateSpeed);
    }
     
    private void Slide()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
           
            var endTouchPosition = Input.mousePosition;
            var offsetX = endTouchPosition.x - startTouchPosition.x;
            if (offsetX > 0)
                offsetX = Mathf.Clamp(offsetX, 0, maxSlideDistance);
            else if (offsetX < 0)
                offsetX = Mathf.Clamp(offsetX, -maxSlideDistance, 0);
            NormalizedOffsetX = offsetX / maxSlideDistance;
        }
    }
}
