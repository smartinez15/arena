using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public float fireRate = 500;
    public float bulletSpeed = 5;
    public int damage = 1;
    public float bulletLifeSpan = 2;
    public bool bulletRicochet = false;

    public Transform muzzle;
    public Projectile bullet;

    public Transform shellEject;
    public Transform shell;

    private float nextShotTime;

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + fireRate / 1000;
            Projectile newBullet = Instantiate(bullet, muzzle.position, muzzle.rotation) as Projectile;
            newBullet.SetGunOptions(bulletSpeed, damage, bulletLifeSpan, bulletRicochet);

            Instantiate(shell, shellEject.position, shellEject.rotation);
        }
    }
}
