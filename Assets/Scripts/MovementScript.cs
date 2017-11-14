using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour {
	bool moving = false;
	Vector3 startingPos;

	void Start(){
		startingPos = transform.position;
	}
	void Update () {
		Vector3 dir = Vector3.zero;
		moving = false;
		if (Input.GetKey (KeyCode.W)) {
			transform.position += Vector3.forward * Time.deltaTime * 10;
			dir += Vector3.forward;
			moving = true;
		}
		if (Input.GetKey (KeyCode.S)) {
			transform.position += Vector3.forward * -Time.deltaTime * 10;
			dir -= Vector3.forward;
			moving = true;
		}
		if (Input.GetKey (KeyCode.A)) {
			transform.position += Vector3.right * -Time.deltaTime * 10;
			dir -= Vector3.right;
			moving = true;
		}
		if (Input.GetKey (KeyCode.D)) {
			transform.position += Vector3.right * Time.deltaTime * 10;
			dir += Vector3.right;
			moving = true;
		}
		if(Input.GetKey(KeyCode.R)){
			transform.position = startingPos;
			gameObject.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}
		transform.position = new Vector3 (transform.position.x, 
			Terrain.activeTerrain.SampleHeight (transform.position + dir) + 5,
			transform.position.z);
		transform.LookAt (transform.position + dir);
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);

	}
}
