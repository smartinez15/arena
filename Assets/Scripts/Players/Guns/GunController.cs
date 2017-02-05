using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform holder;
    public Gun[] guns;
    private Gun equippedGun;

    public void EquipGun(Gun newGun)
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(newGun, holder.position, holder.rotation) as Gun;
        equippedGun.transform.parent = holder;
    }

    public void EquipGun(int gunIndex)
    {
        gunIndex = Mathf.Clamp(gunIndex, 0, guns.Length - 1);
        EquipGun(guns[gunIndex]);
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }

    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerReleased()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight
    {
        get
        {
            return holder.position.y;
        }
    }
}
