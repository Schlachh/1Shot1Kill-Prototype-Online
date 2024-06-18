using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public int playerNumber;
    [HideInInspector] public Rigidbody rb;
    public float timeToDestroy;
    [HideInInspector] public ProjectileGun gun;
    public float damageAmout;
    bool canDamage;
    public GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        explosion.SetActive(false);
    }
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        StartProjectile();
    }
    private void OnDisable()
    {
        //StopProjectile();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && canDamage == true)
        {
            if (other.GetComponent<PlayerHealth>().playerNum == playerNumber) return;
            
            Explode();
            StopProjectile();

        }
        
    }
    private void StopProjectile()
    {
        canDamage = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        GetComponent<TrailRenderer>().emitting = false;
        StopCoroutine(TimeTillDestruction(damageAmout));
        GetComponent<Renderer>().enabled = false;
        
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.SetActive(true);
        explosion.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DisableObject());
    }
    IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(1);
        //GetComponent<Renderer>().enabled = true;
        explosion.SetActive(false);
        gameObject.SetActive(false);
        yield return null;
    }
    private void StartProjectile()
    {
        canDamage = true;
        rb.constraints = RigidbodyConstraints.None;
        explosion.SetActive(false);
        GetComponent<TrailRenderer>().Clear();
        GetComponent<Renderer>().enabled = true;
        GetComponent<TrailRenderer>().emitting = true;
        if(gameObject.activeInHierarchy) StopAllCoroutines();
        StartCoroutine(TimeTillDestruction(damageAmout));
    }
    IEnumerator TimeTillDestruction(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopProjectile();
        yield return null;
    }
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5);

        for (int i = 0; i < colliders.Length; i++)
        {
            DealDamage(colliders[i].gameObject);
        }
    }
    void DealDamage(GameObject target)
    {
        if (!canDamage) return;
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
}
