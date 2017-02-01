using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : Entity
{

    public float moveSpeed = 5;
    private PlayerController controller;
    private GunController gun;
    private Camera playerCamera;

    protected override void Start()
    {
        base.Start();
        controller = GetComponent<PlayerController>();
        gun = GetComponent<GunController>();
        playerCamera = Camera.main;
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
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
        }

        //Weapon
        if (Input.GetMouseButton(0))
        {
            gun.Shoot();
        }
    }
}
