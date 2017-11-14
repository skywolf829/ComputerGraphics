using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour {
	bool rotating = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha0)) {
			rotating = !rotating;
		}
		Rotate ();
	}
	void Rotate(){
		if (rotating) {
			transform.RotateAround (Vector3.zero, Vector3.up, Time.deltaTime * 25);
		}
	}
}
