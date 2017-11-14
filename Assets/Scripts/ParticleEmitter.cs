using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour {

	private List<GameObject> particles;

	public Color startColor, endColor;
	public Vector3 startPos, gravity, startVel,
		startPosVariance, velocityVariance;
	public float drag, maxLife, spawnRate;
	public int numParticles;
	public Sprite sprite;
	public GameObject container;

	void Start () {
		particles = new List<GameObject> ();
		if (!sprite) {
			Debug.Log ("No sprite chosen. Disabling object");
			gameObject.SetActive (false);
		}
		StartCoroutine (CreateParticles ());

	}
	
	void Update () {
		
		for (int i = 0; i < particles.Count; i++) {
			GameObject p = particles [i];
			if (particles [i].GetComponent<Particle> ().age > maxLife) {
				particles.Remove (p);
				Destroy (p);
				i--;
			}
		}

	}
	IEnumerator CreateParticles(){
		while (true) {
			if(particles.Count < numParticles)
				CreateParticle (); 				
			yield return new WaitForSecondsRealtime (spawnRate);
		}
		yield return null;
	}
	void CreateParticle(){
		GameObject p = new GameObject ();

		Particle script = p.AddComponent<Particle> ();
		script.acceleration = Vector3.zero;
		script.velocity = ControlledRandomize (startVel, 
			velocityVariance);
		script.age = 0;
		script.drag = ControlledRandomize (drag, 0.03f);
		script.gravity = gravity;
		script.maxAge = ControlledRandomize(maxLife, 1);
		script.startColor = startColor;
		script.endColor = endColor;

		script.appearance = sprite;

		p.transform.position = container.transform.position + 
			ControlledRandomize(startPos, startPosVariance);

		if (container) {
			p.transform.parent = container.transform;
		}
		particles.Add (p);
	}

	public static float ControlledRandomize(float center, float bounds){
		return center + Random.Range (bounds * -1, bounds);
	}
	public static Vector3 ControlledRandomize(Vector3 center, Vector3 bounds){
		return new Vector3 (center.x + Random.Range (bounds.x * -1, bounds.x),
			center.y + Random.Range (bounds.y * -1, bounds.y),
			center.z + Random.Range (bounds.z * -1, bounds.z));
	}
}
