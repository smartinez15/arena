using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    public float speed = 10;
    public int damage = 1;
    public float lifeSpan = 3;

    private float deathTime;

    void Start()
    {
        deathTime = Time.time + lifeSpan;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
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

        if (Physics.Raycast(ray, out hit, moveDistance + .05f, collisionMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.GetComponent<Rigidbody>() == null)
            {
                Vector3 reflect = Vector3.Reflect(ray.direction, hit.normal);
                float angle = 90 - Mathf.Atan2(reflect.z, reflect.x) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, angle, 0);
            }
            else
            {
                IDamageable dObject = hit.collider.GetComponent<IDamageable>();
                if (dObject != null)
                {
                    dObject.TakeHit(damage, hit);
                    GameObject.Destroy(gameObject);
                }
            }
        }
    }
}
