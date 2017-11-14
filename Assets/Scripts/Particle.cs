using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {
	public Vector3 velocity, acceleration, gravity;
	public float age, maxAge, mass, drag;
	public Color startColor, endColor;
	public Sprite appearance;


	public Dictionary<GameObject, float> collisionPlanesBefore =
		new Dictionary<GameObject, float> ();
	public Dictionary<GameObject, float> collisionPlanesAfter =
		new Dictionary<GameObject, float> ();


	void Start(){
		SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer> ();
		sr.sprite = appearance;
	}

	void FixedUpdate(){
		acceleration = gravity;

		GetComponent<SpriteRenderer> ().color =  
			Color.Lerp (startColor, endColor, age / maxAge);
		GetComponent<SpriteRenderer> ().color = new Color (
			GetComponent<SpriteRenderer> ().color.r,
			GetComponent<SpriteRenderer> ().color.g,
			GetComponent<SpriteRenderer> ().color.b,
			1);		


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

		age += Time.fixedDeltaTime;
	}

	void ApplyForce(Vector3 force){
		acceleration += (force / mass);
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

}
