using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float viewAngle;



    public LayerMask obsticleMask, targetMask;
    [HideInInspector] public List<Transform> targets = new List<Transform>();
    private Transform mainTarget;

    public bool debugging;
    
    public void FindVisibletargets()
    {
        Debug.Log("Check");
        targets.Clear();
        //gather objects in range
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, radius, targetMask);

        //loop through the range of objects to sort them
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Debug.Log("Loop");
            //for each object, store a reference and get its direction from this object.
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            //check if the object is in the fov
            if(Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                //check if line of sight is brocken
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obsticleMask))
                {
                    targets.Add(target);
                }
                 
            }
            
        }
    }
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    private void OnDrawGizmos()
    {
        if (!debugging) return;
        Vector3 lhAngle = DirFromAngle(-viewAngle / 2, false);
        Vector3 rhAngle = DirFromAngle(viewAngle / 2, false);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + lhAngle * radius);
        Gizmos.DrawLine(transform.position, transform.position + rhAngle * radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        if(targets.Count > 0)
        {
            foreach(Transform item in targets)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(item.position, 1);
            }
        }
        
    }
}
