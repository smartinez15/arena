using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;

    private float speed = 10;
    private int damage = 1;
    private bool ricochet = false;

    private float deathTime;

    void Start()
    {
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 1)
        {
            string n = initialCollisions[0].name;

            OnHitObject(initialCollisions[0], transform.position);
        }
    }

    public void SetGunOptions(float nSpeed, int nDamage, float nLifeSpan, bool nRicochet)
    {
        speed = nSpeed;
        damage = nDamage;
        ricochet = nRicochet;
        deathTime = Time.time + nLifeSpan;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * moveDistance);
        CheckCollisions(moveDistance);
        if (Time.time > deathTime)
        {
            GameObject.Destroy(gameObject);
        }
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + .1f, collisionMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponent<Rigidbody>() == null && ricochet)
            {
                Vector3 reflect = Vector3.Reflect(ray.direction, hit.normal);
                float angle = 90 - Mathf.Atan2(reflect.z, reflect.x) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else
            {
                Destroy(gameObject);
                IDamageable dObject = hit.collider.GetComponent<IDamageable>();
                if (dObject != null)
                {
                    OnHitObject(hit.collider, hit.point);
                }
            }
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        Destroy(gameObject);
        IDamageable dObject = c.GetComponent<IDamageable>();
        if (dObject != null)
        {
            dObject.TakeHit(damage, hitPoint, transform.forward);
        }
    }
}
