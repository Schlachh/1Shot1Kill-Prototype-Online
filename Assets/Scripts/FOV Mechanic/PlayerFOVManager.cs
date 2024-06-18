using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFOVManager : MonoBehaviour
{
    private FieldOfView fov;
    private CCMovement moveScript;

    public Transform target;
    public float checkFrequency;
    private void Start()
    {
        fov = GetComponent<FieldOfView>();
        moveScript = GetComponentInParent<CCMovement>();
        StartCoroutine(FindTargetsWithDelay(checkFrequency));
    }
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            fov.FindVisibletargets();
            SelectTarget();
        }
    }
    void SelectTarget()
    {
        for (int i = 0; i < fov.targets.Count; i++)
        {
            if(fov.targets[i].tag == "Player" && fov.targets[i] != transform.root)
            {
                target = fov.targets[i];
                moveScript.target = target;
                moveScript.shouldLook = true;
                return;
            }
        }
        moveScript.target = null;
        moveScript.shouldLook = false;
        return;
    }
}
