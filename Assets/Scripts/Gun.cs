using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        Burst,
        Single
    }
    public FireMode fireMode;
    
    public Transform[] projectileSpawn;
    public Projectile projectile;
    private MuzzleFlash muzzleFlash;
    public int burstCount;
    public int projectilesPerMag;
    public float reloadTime = .01f;

    public AudioClip shootClip;
    public AudioClip reloadClip;

    public Shell shell;
    public Transform shellEjection;
    
    public float msBetweenShoot = 100;
    public float muzzleVelocity = 35;

    private float nextShootTime;

    private bool triggerReleaseSinceLastShot;
    private int shotsRemainingInBurst;

    private Vector3 recoilSmoothDampVelocity;
    private float Velocity;
    private float recoilAngle;
    public Vector2 kickMinMax;

    [SerializeField] private int projectilesRemainingInMag;
    private bool isRelading;
    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = projectilesPerMag;
    }

    private void LateUpdate()
    {
        transform.localPosition =
            Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, .1f);
        //recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref Velocity, .1f);
        //transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;

        if (!isRelading && projectilesRemainingInMag <= 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        if (!isRelading && Time.time > nextShootTime)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }

                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleaseSinceLastShot) return;
            }
            
            nextShootTime = Time.time + msBetweenShoot * 0.001f;
            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if (projectilesRemainingInMag <= 0)
                {
                    break;
                }
                projectilesRemainingInMag--;
                Projectile newProjectile = Instantiate(projectile);
                newProjectile.transform.position = projectileSpawn[i].position;
                newProjectile.transform.rotation = projectileSpawn[i].rotation;
                newProjectile.SetSpeed(muzzleVelocity);
            }

            Instantiate(shell.gameObject, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            AudioManager.instance.PlaySound(shootClip, transform.position);
            //recoilAngle += 20;
            //recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
        }
    }

    public void Reload()
    {
        if (!isRelading)
        {
            AudioManager.instance.PlaySound(reloadClip, transform.position);
            StartCoroutine(nameof(AnimateReload));
        }
    }

    IEnumerator AnimateReload()
    {
        isRelading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1 / reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadTime;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }

        isRelading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }
    public void OnTriggerHold()
    {
        Shoot();
        triggerReleaseSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleaseSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }

    public void Aim(Vector3 position)
    {
        if(!isRelading)
            transform.LookAt(position);
    }
}
