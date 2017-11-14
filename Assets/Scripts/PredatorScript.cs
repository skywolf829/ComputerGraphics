using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorScript : MonoBehaviour {

	private float sightAngle = 30;

	private float collisionRadius = 1.3f;
	private float sightRadius = 5;

	private float maxSpeed = 15;

	GameObject sightObject;

	void Start(){
		GetComponent<Rigidbody2D> ().velocity = 
			new Vector2 (Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
	}
	void Update () {
		Vector2 vel = GetComponent<Rigidbody2D> ().velocity;

		vel += ChaseClosestPrey();

		float angle = Vector2.Angle (Vector2.right, vel);
		if (vel.y < 0)
			angle *= -1;
		transform.eulerAngles = 
			new Vector3 (0, 0, angle - 90);


		vel = Vector2.Lerp(vel, maxSpeed * vel.normalized, 0.1f);

		GetComponent<Rigidbody2D> ().velocity = vel;
	}
	Vector2 ToVector2(Vector3 input){
		return new Vector2 (input.x, input.y);
	}
	Vector3 ToVector3(Vector2 input){
		return new Vector3 (input.x, input.y, 0);
	}
	Vector2 ChaseClosestPrey(){
		Vector3 pos = transform.position;
		List<GameObject> preyInSight = new List<GameObject> ();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Prey")) {
			if (WithinRadius (g) && InSight (g)) {
				preyInSight.Add (g);	
				pos = g.transform.position;
			}
		}
		foreach (GameObject g in preyInSight) {
			if(Vector3.Distance(transform.position, g.transform.position) < 
				Vector3.Distance(transform.position, pos)){
				pos = g.transform.position;
			}
		}

		return ToVector2((pos - transform.position) / 5.0f );
	}
	bool WithinCollisionRange(GameObject g){
		return Vector3.Distance (g.transform.position, transform.position) < collisionRadius;
	}
	bool WithinRadius(GameObject g){
		return Vector3.Distance (g.transform.position, transform.position) < sightRadius;
	}
	bool InSight(GameObject g){
		return Vector3.Angle(transform.position + transform.up, g.transform.position) < sightAngle;
	}
}
