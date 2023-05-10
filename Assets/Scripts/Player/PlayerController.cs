using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	public Camera cam;
	[SerializeField] private InputActionReference movement, look;
	private CharacterController cc;

	[Range(1f, 5f)] public float mouseSensitivity;
	[Range(5f, 20f)] public float movementSpeed;
	private float xRotation = 0f;
	private Vector2 movementInput;
	private Vector2 lookInput;
	private Vector3 velocity = new Vector3();
	private Vector3 hitDirection;

	public bool isGrounded;

	private void Awake()
	{
		cc = GetComponent<CharacterController>();
		mouseSensitivity = mouseSensitivity * 4;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		PlayerLook();
		movementInput = movement.action.ReadValue<Vector2>();
	}

	private void FixedUpdate()
	{
		PlayerMovement();
	}

	private void PlayerMovement()
	{
		Vector3 delta;

		Vector3 camForward = cam.transform.forward;
		camForward.y = 0;
		camForward.Normalize();

		Vector3 camRight = cam.transform.right;
		delta = (movementInput.x * camRight + movementInput.y * camForward) * movementSpeed;

		if(isGrounded || movementInput.x != 0 || movementInput.y != 0)
		{
			velocity.x = delta.x;
			velocity.z = delta.z;
		}

		if(isGrounded && velocity.y < 0)
			velocity.y = 0;

		velocity += Physics.gravity * Time.fixedDeltaTime;

		if(!isGrounded)
			hitDirection = Vector3.zero;

		if(movementInput.x == 0 && movementInput.y == 0)
		{
			Vector3 horizontalHitDirection = hitDirection;
			horizontalHitDirection.y = 0;
			float displacement = horizontalHitDirection.magnitude;
			if(displacement > 0)
				velocity -= 0.2f * horizontalHitDirection / displacement;
		}

		cc.Move(velocity * Time.fixedDeltaTime);

		isGrounded = cc.isGrounded;
	}


	private void PlayerLook()
	{
		lookInput = look.action.ReadValue<Vector2>();

		float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
		float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -80f, 80f);

		cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
		transform.Rotate(Vector3.up * mouseX);
	}
}