using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : Entity
{
    public int playerNumber = 1;
    public float moveSpeed = 5;
    private PlayerController controller;
    private GunController gun;
    private Camera playerCamera;
    private CameraCtrl camCtrl;

    protected override void Start()
    {
        base.Start();
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
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
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
            gun.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gun.OnTriggerReleased();
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
    }
}
