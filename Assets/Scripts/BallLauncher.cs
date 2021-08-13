using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BallLauncher : MonoBehaviour
{

    public Rigidbody ball;
    public Transform target;
    private Rigidbody rb;
    
    public float h = 25;
    public float gravity = -18;

    public bool debugPath;
    public RotatePlatforms rotatePlatforms;
    public int offset;
    public int distance;

    
    [SerializeField] private float time;
    public AnimationCurve animationCurve;
    private bool onGround;
    public List<FollowSystem> parts;
    public FollowSystem ringPrefab;
    [SerializeField] private float height;
    [SerializeField] private GameObject ringCollectEffect;
    private bool flag;
    [SerializeField] private FollowSystem _newHead;
    public FollowSystem lastHead;
    
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    void Update()
    {
        
    }
    void Launch()
    {
        Physics.gravity = Vector3.up * gravity;
        rb.useGravity = true;
        rb.velocity = CalculateLaunchData().initialVelocity;
    }

    LaunchData CalculateLaunchData()
    {
        float displacementY = transform.position.y - rb.position.y;
        Vector3 displacementXZ =
            new Vector3(transform.position.x + (distance*offset)  - rb.position.x, 0, transform.position.z - rb.position.z);
        float time = Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / time;

        return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravity), time);
    }

    void DrawPath()
    {
        LaunchData launchData = CalculateLaunchData();
        Vector3 previousDrawPoint = rb.position;

        int resolution = 30;
        for (int i = 1; i <= resolution; i++)
        {
            float simulationTime = i / (float) resolution * launchData.timeToTarget;
            Vector3 displacement = launchData.initialVelocity * simulationTime +
                                   Vector3.up * gravity * simulationTime * simulationTime / 2f;
            Vector3 drawPoint = rb.position + displacement;
            Debug.DrawLine(previousDrawPoint, drawPoint, Color.green);
            previousDrawPoint = drawPoint;
        }
    }

    struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.tag == "Ground")
        {
            var rot = coll.gameObject.GetComponent<Transform>().rotation;
            var rotationZ = rot.eulerAngles.z - transform.rotation.eulerAngles.z;
            transform.DOLocalRotateQuaternion(
                coll.gameObject.transform.rotation * Quaternion.Euler(0, 0, rotationZ), 0.1f);

            if (rotatePlatforms.NormalizedOffsetX > 0)
                offset = -1;
            else if (rotatePlatforms.NormalizedOffsetX < 0)
                offset = 1;
            else if (rotatePlatforms.NormalizedOffsetX == 0)
                offset = 1;
            
            Debug.Log(this.gameObject.name + " Перед корутиной");
            StartCoroutine(_newHead.GetComponent<BallLauncher>().StartJump());
            //StartCoroutine(StartJump());
        }
    }

    IEnumerator StartJump()
    {
        yield return new WaitForSeconds(0.7f);
        ReverseFollowSystem();
        StartCoroutine(MoveAndRotate());
        Launch();
    }
    IEnumerator MoveAndRotate() 
    {
        //yield return new WaitForSeconds(0.5f);
        var elapsed = 0f;
        var eulers = transform.eulerAngles;
            
        while (elapsed < 1f)
        {
            elapsed += Time.fixedDeltaTime / time;
            Quaternion rot = Quaternion.Euler(eulers.x, eulers.y,
                eulers.z + (animationCurve.Evaluate(elapsed) * offset));

            transform.rotation = Quaternion.Lerp(transform.rotation, rot, elapsed);
            yield return new WaitForFixedUpdate();
        }
        onGround = false;
    }
    
    
    private void ReverseFollowSystem()
    {
        var copyParts = new List<FollowSystem>(parts);

        for (int i = 0; i < parts.Count; i++)
        {
            parts[i]._target = copyParts[(i + 1) % parts.Count].transform;
        }

        _newHead = parts[parts.Count - 1];
        _newHead._target = null;
        _newHead.GetComponent<BoxCollider>().enabled = true;
        _newHead.GetComponent<Rigidbody>().isKinematic = false;
        _newHead.GetComponent<BallLauncher>().enabled = true;

        lastHead = parts[0];
        lastHead.GetComponent<Rigidbody>().isKinematic = true;
        lastHead.GetComponent<BoxCollider>().enabled = false;
        lastHead.GetComponent<BallLauncher>().enabled = false;
        
        parts.Reverse();
        
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.tag == "Ring")
        { 
            Instantiate(ringCollectEffect, coll.gameObject.transform.position, Quaternion.identity);
            Destroy(coll.gameObject);
            var part = Instantiate(ringPrefab, transform.position, Quaternion.identity);
            part._target = parts[parts.Count - 1].gameObject.transform;
            parts.Add(part);
        }
        
        else if (coll.tag == "Obstacle")
        {
            for (int i = 0; i < parts.Count - 1; i++)
            {
                var currentColor = parts[i].GetComponentInChildren<MeshRenderer>().material.color;
                parts[i].GetComponentInChildren<MeshRenderer>().material.DOColor(Color.white, 0.3f);
                StartCoroutine(NormalizeColorMaterial(parts[i].gameObject,currentColor));
            }
            parts[parts.Count - 1]._target = null;
            parts[parts.Count - 1].gameObject.AddComponent<Rigidbody>().AddExplosionForce(200f,coll.transform.position - Vector3.up, 100f);
            parts.Remove(parts[parts.Count - 1]);
        }
    }

    IEnumerator NormalizeColorMaterial(GameObject part, Color currentColor)
    {
        yield return new WaitForSeconds(0.3f);
        part.GetComponentInChildren<MeshRenderer>().material.DOColor(currentColor, 0.3f);
    }

}