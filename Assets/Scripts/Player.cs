using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed;//移动速度
    
    private PlayerController playerController;
    private GunController gunController;
    private Camera viewCamera;

    public Crosshairs crosshair;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1);
    }

    protected override void Start()
    {
        base.Start();
        
    }

    private void Update()
    {
        PlayerTrigger();
        PlayerMovement();
        PlayerRotation();
        
        if (transform.position.y < -10)
        {
            TakeDamage(health);
        }
    }

    void PlayerMovement()
    {
        var moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        var moveVelocity = moveInput * moveSpeed;
        playerController.Move(moveVelocity);
    }

    void PlayerRotation()
    {
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        crosshair.DetectTargets(ray);
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            playerController.LookAt(point);
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude >
                1)
            {
                crosshair.transform.position = point;
                gunController.Aim(new Vector3(point.x, 1,point.z));
            }
        }
    }

    void PlayerTrigger()
    {
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }

        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }
}
