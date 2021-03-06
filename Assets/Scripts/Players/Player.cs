﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : Entity
{
    public int playerNumber = 1;
    public float moveSpeed = 5;
    public Crosshairs crosshairs;

    private PlayerController controller;
    private GunController gun;
    private Camera playerCamera;
    private CameraCtrl camCtrl;

    protected override void Start()
    {
        base.Start();
    }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        gun = GetComponent<GunController>();
        camCtrl = GameObject.FindWithTag("Player" + playerNumber + "CamHolder").GetComponent<CameraCtrl>();
        Camera[] sceneCameras = Camera.allCameras;
        for (int i = 0; i < sceneCameras.Length; i++)
        {
            if (sceneCameras[i].CompareTag("Player" + playerNumber + "Camera"))
            {
                playerCamera = sceneCameras[i];
                break;
            }
        }
        //FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gun.EquipGun(waveNumber);
    }

    void Update()
    {
        //Movement
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        moveInput = playerCamera.transform.TransformDirection(moveInput);
        moveInput.y = 0;
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;

        controller.Move(moveVelocity);

        //Rotation
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gun.GunHeight);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTarget(ray);
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gun.Aim(point);
            }
        }

        //Weapon
        if (Input.GetMouseButton(0))
        {
            gun.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gun.OnTriggerReleased();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gun.Reload();
        }

        //Camera
        if (Input.GetKey(KeyCode.Q))
        {
            camCtrl.Rotate('Q');
        }
        if (Input.GetKey(KeyCode.E))
        {
            camCtrl.Rotate('E');
        }

        //Bounds
        if (transform.position.y < -20)
        {
            TakeDamage(health);
        }
    }

    public override void Die()
    {
        AudioManager.instance.PlaySound("PlayerDeath", transform.position);
        base.Die();
    }
}
