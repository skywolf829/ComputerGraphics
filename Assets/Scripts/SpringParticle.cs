using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringParticle : MonoBehaviour {
	public Vector3 velocity, acceleration, gravity, windForce;
	public float dampening = 0.1f, mass = 1;
	public bool staticParticle = false;

	public Dictionary<GameObject, float> springConstant =
		new Dictionary<GameObject, float>();
	public Dictionary<GameObject, float> restingDistance =
		new Dictionary<GameObject, float>();
	
	public Dictionary<GameObject, float> collisionPlanesBefore =
		new Dictionary<GameObject, float> ();
	public Dictionary<GameObject, float> collisionPlanesAfter =
		new Dictionary<GameObject, float> ();
	
	public List<GameObject> connectedParticles = 
		new List<GameObject>();


	void FixedUpdate(){
		if ((gameObject.GetComponent<Grabber> ().grabbing) || staticParticle)
			return;

		acceleration = Vector3.zero;
		UpdateSprings ();
		ApplyForce (gravity * mass);
		ApplyForce (windForce);

		collisionPlanesBefore = new Dictionary<GameObject, float> ();
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Collidable")){
			collisionPlanesBefore.Add (g, PointPlaneIntersection (g));
		}

		velocity += acceleration * Time.fixedDeltaTime;
		transform.position += velocity * Time.fixedDeltaTime;

		collisionPlanesAfter = new Dictionary<GameObject, float> ();
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("Collidable")){
			collisionPlanesAfter.Add (g, PointPlaneIntersection (g));
		}

		CheckCollisions ();
	}

	//Applies a force to the particle
	void ApplyForce(Vector3 force){
		acceleration += (force / mass);
	}


	void UpdateSprings(){
		List<Vector3> forces = new List<Vector3> ();
		foreach (GameObject g in connectedParticles) {
			float resting;
			float spring;
			restingDistance.TryGetValue (g, out resting);
			springConstant.TryGetValue (g, out spring);
			float dist = Vector3.Distance (gameObject.transform.position, 
				              g.transform.position);
			float delta = dist - resting;
			Vector3 dir = (gameObject.transform.position - g.transform.position);
			Vector3 force = dir.normalized
				* -delta * spring - dampening * velocity;
			ApplyForce (force);
		}
	}
	void CheckCollisions(){
		Vector3 suggestedVelocity = Vector3.zero;
		foreach (GameObject g in collisionPlanesBefore.Keys) {
			float valueBefore, valueAfter;
			collisionPlanesBefore.TryGetValue (g, out valueBefore);
			collisionPlanesAfter.TryGetValue (g, out valueAfter);

			if ((valueBefore > 0 && valueAfter <= 0) ||
				(valueAfter > 0 && valueBefore <= 0)) {
				float numerator = Vector3.Dot (velocity, gameObject.transform.up);
				float denominator = Vector3.Dot (gameObject.transform.up, gameObject.transform.up);
				Vector3 u = (numerator / denominator) * gameObject.transform.up;
				Vector3 w = velocity - u;
				suggestedVelocity += (w - u) * 0.9f;
				transform.position -= velocity * Time.fixedDeltaTime;
				velocity = suggestedVelocity / (float)collisionPlanesBefore.Count;
			}
		}

	}
	float PointPlaneIntersection(GameObject plane){
		float d = -Vector3.Dot (plane.transform.up, plane.transform.position);
		float e = Vector3.Dot (plane.transform.up, transform.position) + d;
		return e;
	}
	public bool IsConnectedTo(GameObject particle){
		return connectedParticles.Contains (particle);
	}

}
