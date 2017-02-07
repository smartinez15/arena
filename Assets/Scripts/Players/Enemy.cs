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
    public Material deathFxMaterial;
    public static event System.Action onDeathStatic;

    State state = State.Idle;
    NavMeshAgent pathFinder;
    Transform target;
    Entity targetEntity;
    Material skinMaterial;
    Color skinOriginalColor;

    float attackDistance = 0.5f;
    float timeBwettenAttacks = 1;
    int attackDamage = 1;
    float nextAttackTime;
    float collisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

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
        skinOriginalColor = skinColor;
        deathFxMaterial.color = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
    }

    public override void TakeHit(int damage, Vector3 hitpoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health && !dead)
        {
            if (onDeathStatic != null)
            {
                onDeathStatic();
            }
            AudioManager.instance.PlaySound("EnemyDeath", transform.position);
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
                    AudioManager.instance.PlaySound("EnemyAttack", transform.position);
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

        skinMaterial.color = skinOriginalColor;
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
