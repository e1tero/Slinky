using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SlinkyMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float time;
    public int offset;
    public AnimationCurve animationCurve;
    private bool onGround;
    public RotatePlatforms rotatePlatforms;
    public List<FollowSystem> parts;
    public FollowSystem ringPrefab;
    [SerializeField] private float distance, height;
    [SerializeField] private GameObject ringCollectEffect;
    private bool flag;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //flag = true;
    }
    
    
    IEnumerator MoveAndRotate()
    {
        yield return new WaitForSeconds(0.5f);
        var elapsed = 0f;
        //Vector3 currentPosition = transform.position;
        //Vector3 targetPosition = new Vector3(transform.position.x + (distance * offset), transform.position.y,
            //transform.position.z);
        var eulers = transform.eulerAngles;
            
        while (elapsed < 1f)
        {
            elapsed += Time.fixedDeltaTime / time;
            Quaternion rot = Quaternion.Euler(eulers.x, eulers.y,
                eulers.z + (animationCurve.Evaluate(elapsed) * offset));

            transform.rotation = Quaternion.Lerp(transform.rotation, rot, elapsed);
            //transform.position = (Vector3Extensions.Parabola(currentPosition, targetPosition, height, elapsed));
            yield return new WaitForFixedUpdate();
        }

       
        onGround = false;
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

            StartCoroutine(MoveAndRotate());
            ReverseFollowSystem();
        }

    }

    private void ReverseFollowSystem()
    {
        var copyParts = new List<FollowSystem>(parts);

        for (int i = 0; i < parts.Count; i++)
        {
            parts[i]._target = copyParts[(i + 1) % parts.Count].transform;
        }

        var newHead = parts[parts.Count - 1];
        newHead._target = null;
        newHead.GetComponent<BallLauncher>().enabled = true;
        var lastHead = parts[0];
        lastHead.GetComponent<Rigidbody>().isKinematic = true;
        lastHead.GetComponent<BallLauncher>().enabled = false;
        lastHead.GetComponent<BoxCollider>().enabled = false;
        
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
