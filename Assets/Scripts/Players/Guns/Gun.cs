using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }

    public FireMode fireMode;
    public float fireRate = 500;
    public float bulletSpeed = 5;
    public int damage = 1;
    public float bulletLifeSpan = 2;
    public bool bulletRicochet = false;

    public int burstCount = 5;

    public Transform[] muzzle;
    public Projectile bullet;

    public Transform shellEject;
    public Transform shell;

    float nextShotTime;
    MuzzleFlash muzzleFlash;
    bool triggerReleased;
    int shotsRemainingBurst;

    void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingBurst = burstCount;
    }

    void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingBurst == 0)
                {
                    return;
                }
                shotsRemainingBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleased)
                {
                    return;
                }
            }

            for (int i = 0; i < muzzle.Length; i++)
            {
                nextShotTime = Time.time + fireRate / 1000;
                Projectile newBullet = Instantiate(bullet, muzzle[i].position, muzzle[i].rotation) as Projectile;
                newBullet.SetGunOptions(bulletSpeed, damage, bulletLifeSpan, bulletRicochet);
            }
            Instantiate(shell, shellEject.position, shellEject.rotation);
            muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleased = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleased = true;
        shotsRemainingBurst = burstCount;
    }
}
