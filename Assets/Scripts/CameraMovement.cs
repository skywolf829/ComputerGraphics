﻿using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour {

	public float speed = 2.0f;
	private float zoomSpeed = 2.0f;

	public float minX = -360.0f;
	public float maxX = 360.0f;
	
	public float minY = -45.0f;
	public float maxY = 45.0f;

	public float sensX = 100.0f;
	public float sensY = 100.0f;
	
	float rotationY = 0.0f;
	float rotationX = 0.0f;

	void Update () {

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		transform.Translate(0, scroll * zoomSpeed, scroll * zoomSpeed, Space.World);

		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
			transform.position += transform.right * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)){
			transform.position -= transform.right * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)){
			transform.position += transform.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)){
			transform.position -= transform.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.E)){
			transform.position += transform.up * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Q)){
			transform.position -= transform.up * speed * Time.deltaTime;
		}

		if (Input.GetMouseButton (0)) {
			rotationX += Input.GetAxis ("Mouse X") * sensX * Time.deltaTime;
			rotationY += Input.GetAxis ("Mouse Y") * sensY * Time.deltaTime;
			rotationY = Mathf.Clamp (rotationY, minY, maxY);
			transform.localEulerAngles = new Vector3 (-rotationY, rotationX, 0);
		}
	}
}
