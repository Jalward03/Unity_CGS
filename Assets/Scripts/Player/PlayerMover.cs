using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{
	public Camera cam;
	[SerializeField] private InputActionReference movement, look;
	private Rigidbody rb;
	[Range(1f, 5f)] public float mouseSensitivity;
	[Range(5f, 20f)] public float movementAcceleration;
	[Range(5f, 10f)] public float maxMovementSpeed;
	private Vector2 movementInput;
	private Vector2 lookInput;
	private float xRotation = 0;
	private Vector3 hitDirection;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		mouseSensitivity = mouseSensitivity * 4;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		PlayerLook();
		movementInput = movement.action.ReadValue<Vector2>();
		rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxMovementSpeed);

		if(movementInput.x == 0 && Math.Abs(rb.velocity.x) > 0) rb.velocity -= new Vector3(Time.deltaTime, 0, 0);
		if(movementInput.y == 0 && Math.Abs(rb.velocity.z) > 0) rb.velocity -= new Vector3(0, 0, Time.deltaTime);
	}

	private void FixedUpdate()
	{
		PlayerMovement();
	}

	private void PlayerMovement()
	{
		rb.AddForce((transform.forward * movementInput.y + transform.right * movementInput.x) * movementAcceleration, ForceMode.Acceleration);
	}

	private void PlayerLook()
	{
		Vector3 camForward = cam.transform.forward;
		camForward.y = 0;
		transform.forward = camForward;

		lookInput = look.action.ReadValue<Vector2>();

		float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
		float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

		xRotation -= mouseY;
		xRotation = Mathf.Clamp(xRotation, -80f, 80f);

		cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
		transform.Rotate(Vector3.up * mouseX);
		//Quaternion.Slerp(cam.transform.localRotation, Quaternion.Euler(Vector3.up * mouseX), Time.deltaTime);
	}
}