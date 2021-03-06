﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    private Rigidbody body;
    private Vector3 velocity;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 pVelocity)
    {
        velocity = pVelocity;
    }

    public void LookAt(Vector3 point)
    {
        transform.LookAt(point);
    }

    void FixedUpdate()
    {
        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
    }
}
