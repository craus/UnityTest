﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MoveWithRigidBody : MonoBehaviour
{
    public float speed = 6.0F;
    public float rotateSpeed = 6.0F;

    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;

    void Update()
    {
        if (TimeManager.Paused)
        {
            return;
        }
        transform.Rotate(0, Input.GetAxis("Mouse X") * rotateSpeed, 0, Space.World);
        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}