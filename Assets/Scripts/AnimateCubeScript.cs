using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCubeScript : MonoBehaviour {
	float movementScalar = 15;

	void Update () {
		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			transform.position -= transform.right * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			transform.position += transform.right * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.E)) {
			transform.position -= transform.forward * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.Q)) {
			transform.position += transform.forward * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
			transform.position += transform.up * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
			transform.position -= transform.up * Time.deltaTime * movementScalar;
		} 
		if (Input.GetKey (KeyCode.R)) {
			transform.position = Vector3.zero;
		}
	}
}
