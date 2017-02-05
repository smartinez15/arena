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

    [Header("Options")]
    public FireMode fireMode;
    public float fireRate = 500;
    public float bulletSpeed = 5;
    public int damage = 1;
    public float bulletLifeSpan = 2;
    public bool bulletRicochet = false;
    public int burstCount = 5;
    public int magazineSize;
    public float reloadTime;

    [Header("Recoil")]
    public Vector2 kickBackLimits = new Vector3(0.05f, 0.2f);
    public Vector2 recoilAngleLimits = new Vector2(3, 10);
    public float recoilTime = .1f;

    [Header("Effects")]
    public Transform[] muzzle;
    public Projectile bullet;
    public Transform shellEject;
    public Transform shell;

    float nextShotTime;
    MuzzleFlash muzzleFlash;
    bool isReloading;
    bool triggerReleased;
    int shotsRemainingBurst;
    int shotsRemainingMag;

    Vector3 recoilVelocity;
    float recoilAngleVelocity;
    float recoilAngle;

    void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingBurst = burstCount;
        shotsRemainingMag = magazineSize;
    }

    void LateUpdate()
    {
        //Recoil animation
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilVelocity, recoilTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilAngleVelocity, recoilTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isReloading && shotsRemainingMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (!isReloading && Time.time > nextShotTime && shotsRemainingMag > 0)
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
                if (shotsRemainingMag == 0)
                {
                    break;
                }
                shotsRemainingMag--;
                nextShotTime = Time.time + fireRate / 1000;
                Projectile newBullet = Instantiate(bullet, muzzle[i].position, muzzle[i].rotation) as Projectile;
                newBullet.SetGunOptions(bulletSpeed, damage, bulletLifeSpan, bulletRicochet);
            }
            Instantiate(shell, shellEject.position, shellEject.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickBackLimits.x, kickBackLimits.y);
            recoilAngle += Random.Range(recoilAngleLimits.x, recoilAngleLimits.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
        }
    }

    public void Reload()
    {
        if (!isReloading && shotsRemainingMag != magazineSize)
        {
            StartCoroutine(AnimateReload());
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1f / reloadTime;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 60;
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        shotsRemainingMag = magazineSize;
    }

    public void Aim(Vector3 aimPoint)
    {
        if (!isReloading)
        {
            transform.LookAt(aimPoint);
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
