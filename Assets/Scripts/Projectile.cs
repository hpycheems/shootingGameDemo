using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    public Color trailColor;
    public float speed = 10;
    private float damage = 1;

    private float lifeTime = 3;
    private float skinWith = .1f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        Collider[] colliders = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if(colliders.Length > 0)
        {
            OnHitObject(colliders[0], transform.position);
        }
        GetComponent<TrailRenderer>().material.SetColor("_MainColor", trailColor);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    private void Update()
    {   
        float moveDistance = speed * Time.deltaTime;
        CheckCollisions(moveDistance);
        transform.position += transform.forward * moveDistance;
    }

    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, moveDistance + skinWith, collisionMask))
        {
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageable = c.GetComponent<IDamageable>();
        if(damageable != null)
            damageable.TakeHit(damage, hitPoint, transform.forward);
        GameObject.Destroy(gameObject);
    }
}
