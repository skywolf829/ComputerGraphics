using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour {

	float minDistance = 5f;
	float maxForce = 500;
	float minDistancePredator = 3f;
	float maxForcePredator = 3000;
	float largestScale;

	void Update () {
		largestScale = Mathf.Max (transform.localScale.x, transform.localScale.y) / 2.0f;

		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Predator")) {
			float d = DistanceTo (g);
			if (d < minDistancePredator) {
				Vector3 f = ((minDistancePredator - d) / minDistancePredator) * maxForcePredator *
				            (g.transform.position - ClosestPoint (g.transform.position));
				g.GetComponent<Rigidbody2D> ().AddForce (f);
			}
		}
		foreach (GameObject g in GameObject.FindGameObjectsWithTag("Prey")) {
			float d = DistanceTo (g);
			if (d < minDistance) {
				Vector3 f = ((minDistance - d) / minDistance) * maxForce *
				            (g.transform.position - ClosestPoint (g.transform.position));
				g.GetComponent<Rigidbody2D> ().AddForce (f);
			}
		}
	}

	float DistanceTo(GameObject g){
		Vector3 p = ClosestPoint (g.transform.position);
		return Vector3.Distance (p, g.transform.position);
	}
	Vector3 ClosestPoint(Vector3 pos){
		Vector3 toPoint = pos - transform.position;
		Vector3 proj = Vector3.Project (toPoint, transform.right);
		if (Vector3.Magnitude (proj) > largestScale) {
			proj = proj.normalized * largestScale;
		}
		return transform.position + proj;
	}
}
