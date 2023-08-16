using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public Projectile projectile;
    public Transform muzzle;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            SceneManager.LoadScene(0);
        }
    }

    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.Space))
    //    {
    //        Projectile newProjectile = Instantiate(projectile, projectileSpawn.position, transform.rotation);
    //    }
    //}
}
