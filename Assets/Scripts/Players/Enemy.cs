using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Entity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }

    public GameObject deathFX;

    private State state = State.Idle;
    private NavMeshAgent pathFinder;
    private Transform target;
    private Entity targetEntity;
    private Material skinMaterial;

    private Color originalColor;

    private float attackDistance = 0.5f;
    private float timeBwettenAttacks = 1;
    private int attackDamage = 1;
    private float nextAttackTime;
    private float collisionRadius;
    private float targetCollisionRadius;

    private bool hasTarget;

    void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player1") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player1").transform;
            targetEntity = target.GetComponent<Entity>();
            collisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (hasTarget)
        {
            state = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            StartCoroutine(UpdatePath());
        }
    }

    public void SetDifficulty(float moveSpeed, int hitPoints, int health, Color skinColor)
    {
        pathFinder.speed = moveSpeed;

        if (hasTarget)
        {
            attackDamage = targetEntity.startingHealth / hitPoints;
        }
        startingHealth = health;
        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
    }

    public override void TakeHit(int damage, Vector3 hitpoint, Vector3 hitDirection)
    {
        if (damage >= health)
        {
            Destroy(Instantiate(deathFX, hitpoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject, 4);
        }
        base.TakeHit(damage, hitpoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        state = State.Idle;
    }

    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextAttackTime)
            {
                float sqrDistToTarget = (target.position - transform.position).sqrMagnitude;
                if (sqrDistToTarget < Mathf.Pow(attackDistance + collisionRadius + targetCollisionRadius, 2))
                {
                    nextAttackTime = Time.time + timeBwettenAttacks;
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        state = State.Attacking;
        pathFinder.enabled = false;

        Vector3 originalPos = transform.position;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 attackPos = target.position - directionToTarget * (collisionRadius);

        float attackSpeed = 3;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        float percent = 0;
        while (percent <= 1)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(attackDamage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            transform.position = Vector3.Lerp(originalPos, attackPos, interpolation);

            yield return null;
        }

        skinMaterial.color = originalColor;
        pathFinder.enabled = true;
        state = State.Chasing;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;

        while (hasTarget)
        {
            if (state == State.Chasing)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                Vector3 targetPos = target.position - directionToTarget * (collisionRadius + targetCollisionRadius + attackDistance / 2);
                if (!dead)
                    pathFinder.SetDestination(targetPos);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
