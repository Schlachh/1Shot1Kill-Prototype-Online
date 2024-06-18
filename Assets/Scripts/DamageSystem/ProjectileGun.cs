using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGun : MonoBehaviour, IShootable
{
    [Header("Damage Settings")]
    public int playerNumber;
    public float fireRate = 0.25f;
    private float lastShotTime;

    [Header("Projectile Settings")]//accuracy settings
    public Projectile projectile;
    public float projectileForce = 0.8f;
    public int ammoNumber;
    private List<Projectile> bulletPool = new List<Projectile>();

    [SerializeField] Transform barrelEnd;

    [Header("Effects and Components")] //effects settings
    public float effectTime = 0.5f;
    public ParticleSystem muzzleFlash;
    LineRenderer line;

    [Space]
    public bool debugging = false;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        lastShotTime = -fireRate;
        Debug.Log("ProjectileGun initialized. LastShotTime set to: " + lastShotTime);
        //SetUpPool(); 
    }
    public void Shoot()
    {
        Debug.Log("Attempting to shoot...");

        Debug.Log($"Time.time: {Time.time}, lastShotTime: {lastShotTime}, fireRate: {fireRate}");

        Debug.Log("Shooting!");

        muzzleFlash.transform.position = barrelEnd.position;
        muzzleFlash.transform.rotation = barrelEnd.rotation;
        muzzleFlash.Play();

        Debug.Log($"barrelEnd Position: {barrelEnd.position}, Rotation: {barrelEnd.rotation}");

        //Projectile bullet = GetBulletFromPool();
        Projectile bullet = Instantiate(projectile, barrelEnd.position, barrelEnd.rotation);
        if (bullet == null)
        {
            Debug.Log("Bullet instantiation failed.");
            return;
        }

        Debug.Log("Bullet instantiated.");

        bullet.transform.position = barrelEnd.position;
        bullet.transform.rotation = barrelEnd.rotation;

        bullet.gameObject.SetActive(true);

        bullet.rb.AddForce((barrelEnd.forward * projectileForce) + Vector3.up * 0.2f, ForceMode.Impulse);
        lastShotTime = Time.time;

        Debug.Log("Bullet fired.");
    }

    void SetUpPool()
    {
        for (int i = 0; i < ammoNumber; i++)
        {
            Projectile spawnedAmmo = Instantiate(projectile, transform.position, transform.rotation);
            spawnedAmmo.playerNumber = playerNumber;
            spawnedAmmo.gun = this;
            bulletPool.Add(spawnedAmmo);
            spawnedAmmo.gameObject.SetActive(false);
        }
    }

    Projectile GetBulletFromPool()
    {
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].gameObject.activeInHierarchy)
            {
                return bulletPool[i];
            }
        }
        return null;
    }
    void OnDrawGizmos()
    {
        if (barrelEnd != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(barrelEnd.position, 0.1f);
            Gizmos.DrawLine(barrelEnd.position, barrelEnd.position + barrelEnd.forward * 2);
        }
    }

}
