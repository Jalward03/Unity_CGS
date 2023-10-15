using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
	
	public Camera cam;
	[SerializeField] private InputActionReference movement, look;
	private Rigidbody rb;
	[Range(1f, 30f)] public float mouseSensitivity;
	[Range(1f, 10f)] public float movementAcceleration;
	[Range(1f, 5f)] public float maxMovementSpeed;
	private Vector2 movementInput;
	private Vector2 lookInput;
	private float xRotation = 0;
	private Vector3 hitDirection;

	[HideInInspector] public bool canMove = true;
	
	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		mouseSensitivity = mouseSensitivity * 4;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if(canMove) 
			PlayerLook();
		// Gets data from input
		movementInput = movement.action.ReadValue<Vector2>();
		
		// Caps speed
		rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxMovementSpeed);

	}

	private void FixedUpdate()
	{
		if(canMove) 
			PlayerMovement();
	}

	private void PlayerMovement()
	{
		// Adds force in direction calculated by keyboard input
		rb.AddForce((transform.forward * movementInput.y + transform.right * movementInput.x) * movementAcceleration, ForceMode.Impulse);
	}

	private void PlayerLook()
	{
		// Assigns camera rotation to mouse input and clamps Y rotation at -80- and 80
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
	}
}