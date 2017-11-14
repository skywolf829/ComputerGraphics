using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyScript : MonoBehaviour {

	private float sightAngle = 180;

	private float collisionRadius = 1.3f;
	private float sightRadius = 5;

	private float maxSpeed = 10;

	GameObject sightObject;

	Vector3 startPos;

	void Start(){
		startPos = transform.position;
		GetComponent<Rigidbody2D> ().velocity = 
			new Vector2 (Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
	}
	void Update () {
		Vector2 vel = GetComponent<Rigidbody2D> ().velocity;
		vel +=
			AverageBoidCorrection () +
			MoveAwayFromBoids () +
			AverageBoidVelocity () +
			RunFromPredators() +
			MoveTowardGoal();
		
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
	Vector2 AverageBoidCorrection(){
		Vector3 averagePos = Vector3.zero;
		List<GameObject> flockingUnits = new List<GameObject> ();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Prey")) {
			if (!g.Equals (gameObject) && WithinRadius (g))
				flockingUnits.Add (g);			
		}
		foreach (GameObject g in flockingUnits) {
			averagePos += g.transform.position;
		}
		if (flockingUnits.Count > 0) {
			averagePos /= flockingUnits.Count;
			averagePos = (averagePos - transform.position) / 100f;
		}
		return ToVector2(averagePos);
	}

	Vector2 MoveAwayFromBoids(){
		Vector3 c = Vector3.zero;
		List<GameObject> flockingUnits = new List<GameObject> ();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Prey")) {
			if (!g.Equals (gameObject) && WithinCollisionRange(g))
				flockingUnits.Add (g);
		}
		foreach (GameObject g in flockingUnits) {
			c -= (g.transform.position - transform.position);
		}
		c /= 50.0f;
		return ToVector2(c);
	}
	Vector2 MoveTowardGoal(){
		Vector3 vel = Vector3.zero;
		GameObject closest= null;
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Goal")) {
			if (closest && Vector3.Distance(transform.position, closest.transform.position) >
				Vector3.Distance(transform.position, g.transform.position)) {
				closest = g;
			} else {
				closest = g;
			}
		}
		if(closest)
			vel += (closest.transform.position - transform.position).normalized / 15.0f;
		return ToVector2 (vel);
	}
	Vector2 RunFromPredators(){
		Vector3 vel = Vector3.zero;
		List<GameObject> nearbyPredators = new List<GameObject> ();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Predator")) {
			if (WithinRadius(g))
				nearbyPredators.Add (g);
		}
		foreach (GameObject g in nearbyPredators) {
			vel += transform.position - g.transform.position;
		}
		if (nearbyPredators.Count > 0) {
			vel /= nearbyPredators.Count;
			vel /= 20.0f;
		}
		return ToVector2 (vel);
	}
	Vector2 AverageBoidVelocity(){
		Vector3 vel = Vector3.zero;
		List<GameObject> flockingUnits = new List<GameObject> ();
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Prey")) {
			if (!g.Equals (gameObject) && WithinRadius(g))
				flockingUnits.Add (g);
		}
		foreach (GameObject g in flockingUnits) {
			vel += ToVector3(g.GetComponent<Rigidbody2D> ().velocity);
		}
		if (flockingUnits.Count > 0) {
			vel /= flockingUnits.Count;
			vel = (vel - ToVector3 (GetComponent<Rigidbody2D> ().velocity)) / 8f;
		}
		return ToVector2(vel);
	}
	Vector2 RandomSway(){
		return new Vector2 (Random.Range (-0.05f, 0.05f), Random.Range (-0.5f, 0.05f));
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

	void OnCollisionEnter2D(Collision2D c){
		if (c.gameObject.tag == "Predator") {
			transform.position = startPos;
			GetComponent<Rigidbody2D> ().velocity = 
				new Vector2 (Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
		}
	}
}
