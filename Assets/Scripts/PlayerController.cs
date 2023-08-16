using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Vector3 velocity;
    private Rigidbody mRigidbody;

    private void Start()
    {
        mRigidbody = GetComponent<Rigidbody>();
    }

    public void LookAt(Vector3 lookPoint)
    {
        var heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void Move(Vector3 velocity)
    {
        this.velocity = velocity;
    }
    private void FixedUpdate()
    {
        mRigidbody.MovePosition(mRigidbody.transform.position + velocity * Time.fixedDeltaTime);
    }
}
