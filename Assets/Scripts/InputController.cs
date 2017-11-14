using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour {

	public GameObject container;
	public GameObject prey, predator;

	void Update () {
		if (Input.GetMouseButtonDown (0))
			CreateFriendly ();
		else if (Input.GetMouseButtonDown (1))
			CreateEnemy ();
	}

	void CreateFriendly(){
		if (!prey)
			return;
		GameObject g = GameObject.Instantiate<GameObject> (prey);
		g.name = "Prey";
		Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit rch;
		if (Physics.Raycast(r, out rch)) {
			Vector3 point = new Vector3 (rch.point.x, rch.point.y, 0);
			g.transform.position = point;
		}

		if (container)
			g.transform.parent = container.transform;
	}
	void CreateEnemy(){
		if (!predator)
			return;
		GameObject g = GameObject.Instantiate<GameObject> (predator);
		g.name = "Predator";
		Ray r = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit rch;
		if (Physics.Raycast(r, out rch)) {
			Vector3 point = new Vector3 (rch.point.x, rch.point.y, 0);
			g.transform.position = point;
		}

		if (container)
			g.transform.parent = container.transform;
	}
}
