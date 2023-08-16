using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    public enum State
    {
        Idle,
        Chasing,
        Attacking
    }
    public NavMeshAgent pathFinder;
    private Transform target;
    private Material skinMateiral;
    private LivingEntity targetEntity;
    public ParticleSystem deathEffect;
    public static event Action onDeathStatic;
    
    private State currentStet;
    private Color originalColo;

    private float attackDistanceThreshold = .9f;
    private float timeBetweenAttacks = 1;
    private float damage = 1;

    private float nextAttackTime;
    private float myCollisionRadius;
    private float targetCollisionRadius;

    private bool hasTarget;

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();
            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }

        onDeath += () =>
        {
            AudioManager.instance.PlaySound("Enemy Death", Vector3.zero);
        };
    }

    protected override void Start()
    {
        base.Start();
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            currentStet = State.Chasing;
            targetEntity.onDeath += OnTargetDeath;
            StartCoroutine(nameof(UpdatePath));
        }
    }

    public void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, 
        float enemyHealth, Color skinColor)
    {
        pathFinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;
        skinMateiral = GetComponentInChildren<Renderer>().material;

        Material color = deathEffect.GetComponent<Renderer>().sharedMaterial;
        color.color = new Color(skinColor.r, skinColor.g, skinColor.b, 1);

        skinMateiral.color = skinColor;
        originalColo = skinMateiral.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (damage >= health)
        {
            if (onDeathStatic != null)
            {
                onDeathStatic();
            }
            AudioManager.instance.PlaySound("Impact", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.LookRotation(hitDirection)), 2);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentStet = State.Idle;
    }
    private void Update()
    {
        if (Time.time > nextAttackTime && hasTarget)
        {
            float sqrDstToTarget = (transform.position - target.position).sqrMagnitude;
            if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
            {
                nextAttackTime = Time.time + timeBetweenAttacks;
                AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                StartCoroutine(nameof(Attack));
            }
        }
    }

    IEnumerator Attack()
    {
        currentStet = State.Attacking;//切换状态
        pathFinder.enabled = false;//停止寻路

        Vector3 originalPosition = transform.position;//记录当前位置
        Vector3 dirToTarget = (target.position - transform.position).normalized;//攻击方向
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);//攻击位置

        float attackSpeed = 3;
        float percent = 0;
        
        skinMateiral.color = Color.red;
        bool hasAppliedDamage = false;
        while (percent <= 1)
        {
            if (percent >= 0.5 && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;//一个函数从0上升在下降到0
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        skinMateiral.color = originalColo;

        currentStet = State.Chasing;
        pathFinder.enabled = true;
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;
        while (hasTarget)
        {
            if (currentStet == State.Chasing)
            {
                Vector3 dirToTarget = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius);
                if(!dead)
                    pathFinder.SetDestination(targetPosition);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
