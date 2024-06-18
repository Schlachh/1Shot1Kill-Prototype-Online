using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastGun : MonoBehaviour, IShootable
{
    [Header("Damage Settings")]
    public int playerNumber;
    public float damageAmout;
    public float fireRate;
    private float lastShotTime;

    [Header("Accuracy Settings")]//accuracy settings
    public float range = 10f;
    public float spread = 0.8f;
    public int numberOfrays = 12;

    [SerializeField] Transform barrelEnd;

    [Header("Effects and Components")] //effects settings
    public float effectTime = 0.5f;
    public ParticleSystem muzzleFlash, hitFlash;
    LineRenderer line;


    Ray ray;
    RaycastHit hit;

    [Space]
    public bool debugging = false;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Shoot()
    {
        if (Time.time - lastShotTime < fireRate) return;
        lastShotTime = Time.time;

        //calculate Ray positions
        Vector3 leftSide = barrelEnd.position + (barrelEnd.right * -spread);
        Vector3 rightSide = barrelEnd.position + (barrelEnd.right * spread);
        ray.direction = barrelEnd.forward;

        //raycast out several times until we hit a target
        for (int i = 1; i < numberOfrays; i++)
        {
            //use a lerp to determin the position of the current ray
            ray.origin = Vector3.Lerp(leftSide, rightSide, (float)i / numberOfrays);

            //make the raycast
            if (Physics.Raycast(ray, out hit, range))
            {
                if (hit.collider.tag == "Player" || hit.collider.tag == "Enemy") //if we have hit a player or enemy, DealDamage and quit the loop. Otherwise keep raycasting
                {
                    DealDamage(hit.collider.gameObject);
                    DrawBullet(hit.point);

                    hitFlash.transform.position = hit.point;
                    hitFlash.transform.rotation = Quaternion.LookRotation(hit.point - barrelEnd.position);
                    hitFlash.Play();
                    return;
                }
            }
        }
        //if the loop did not hit a target, raycast from the centre to see if an environment object is hit
        ray.origin = barrelEnd.position;
        if (Physics.Raycast(ray, out hit, range))
        {
            DrawBullet(hit.point);

            hitFlash.transform.position = hit.point;
            hitFlash.transform.rotation = Quaternion.LookRotation(hit.point - barrelEnd.position);
            hitFlash.Play();
            return;
        }
        else //if no environment is hit, then draw a line straight ahead to show a missed bullet
        {
            DrawBullet(barrelEnd.position + barrelEnd.forward * range);
        }
    }

    void DealDamage(GameObject target)
    {
        //attempt to get the player health script on the target of your attack
        PlayerHealth script = target.GetComponent<PlayerHealth>();
        if (script != null) //if the player health script was found, deal damage to the player
        {
            script.lastDamagedBy = playerNumber;
            script.TakeDamage(damageAmout);
            script.StopCoroutine(script.ResetDamagedBy());
            script.StartCoroutine(script.ResetDamagedBy());
        }
        else //if no playerhealth was found, check if the regular health script was found and try to damage that
        {
            Health hScript = target.GetComponent<Health>();
            if (hScript == null) return; //if no script was found for that either, then cancel
            script.TakeDamage(damageAmout);
        }
    }

    void DeactivateEffects()
    {
        line.enabled = false;
    }

    void DrawBullet(Vector3 end)
    {
        line.enabled = true;
        line.SetPosition(0, barrelEnd.position);
        line.SetPosition(1, end);
        muzzleFlash.transform.position = barrelEnd.position;
        muzzleFlash.Play();
        Invoke("DeactivateEffects", effectTime);
    }

    private void OnDrawGizmos()
    {
        if (debugging == false) return;
        //draw out the raycast zone of the gun
        Gizmos.DrawRay(barrelEnd.position, barrelEnd.forward);
        Vector3 leftSide = barrelEnd.position + (barrelEnd.right * -spread);
        Vector3 rightSide = barrelEnd.position + (barrelEnd.right * spread);

        Gizmos.DrawSphere(leftSide, 0.1f);
        Gizmos.DrawSphere(rightSide, 0.1f);
        for (int i = 1; i < numberOfrays; i++)
        {
            Vector3 rayPos = Vector3.Lerp(leftSide, rightSide, (float)i/numberOfrays);
            Gizmos.DrawRay(rayPos, barrelEnd.forward);
        }
    }
}
