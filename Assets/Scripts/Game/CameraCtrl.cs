using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{

    public int playerNumber = 1;            // The position that that camera will be following.
    public float smooth = 3f;        // The speed with which the camera will be following.
    public float rotationSmooth = 4f;

    private Transform target;
    private Vector3 offset;                     // The initial offset from the target.
    private Quaternion rot;
    private bool rotate;
    private bool allowRot;
    private bool playerAlive;

    void Start()
    {
        //Find the player
        target = GameObject.FindWithTag("Player" + playerNumber).transform;
        target.GetComponent<Entity>().OnDeath += OnPlayerDeath;
        playerAlive = true;

        // Calculate the initial offset.
        offset = transform.position - target.position;
        rotate = false;
        allowRot = true;

        rot = transform.rotation;
    }

    void OnPlayerDeath()
    {
        playerAlive = false;
    }

    public void Rotate(char dir)
    {
        if (dir == 'Q' && allowRot)
        {
            Vector3 eul = rot.eulerAngles;
            rot = Quaternion.Euler(eul.x, eul.y + 90f, eul.z);
            rotate = true;
            allowRot = false;
        }
        if (dir == 'E' && allowRot)
        {
            Vector3 eul = rot.eulerAngles;
            rot = Quaternion.Euler(eul.x, eul.y - 90f, eul.z);
            rotate = true;
            allowRot = false;
        }
    }

    void FixedUpdate()
    {
        if (playerAlive)
        {
            // Create a postion the camera is aiming for based on the offset from the target.
            Vector3 targetCamPos = target.position + offset;

            // Smoothly interpolate between the camera's current position and it's target position.
            transform.position = Vector3.Lerp(transform.position, targetCamPos, smooth * Time.deltaTime);

            if (rotate)
            {
                float delta = rot.eulerAngles.y;
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSmooth * Time.deltaTime);
                delta -= transform.localEulerAngles.y;
                if (Mathf.Abs(delta) < 5f)
                {
                    allowRot = true;
                }
                if (Mathf.Abs(delta) < 0.2f)
                {
                    rotate = false;
                    transform.localRotation = rot;
                }
            }
        }
    }
}
