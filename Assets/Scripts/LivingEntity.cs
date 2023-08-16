using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour,IDamageable
{
    public float startingHealth;
    public float health;
    protected bool dead;

    public event Action onDeath; 

    protected virtual void Start()
    {
        health = startingHealth;
        onDeath += () =>
        {
            Cursor.visible = true;
            AudioManager.instance.PlaySound("Player Death", transform.position);
        };
    }
    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hirDirection)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    [ContextMenu("Self Destruct")]
    void Die()
    {
        dead = true;
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
