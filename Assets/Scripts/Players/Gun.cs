using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectile bullet;
    public float fireRate = 500;
    public float muzzleVelocity = 5;

    private float nextShotTime;

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + fireRate / 1000;
            Projectile newBullet = Instantiate(bullet, muzzle.position, muzzle.rotation) as Projectile;
            newBullet.SetSpeed(muzzleVelocity);
        }
    }
}
