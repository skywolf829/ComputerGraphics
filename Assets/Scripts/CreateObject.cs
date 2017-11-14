using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateObject : MonoBehaviour
{
	// Types of objects I'm supporting
	public string[] geometryTypes = new string[]{"Sheet", "Cube", "Sheet in wind"};

	// Currently selected geometry
	public int selectedGeometry = 0;
	public float timeScale = 1f;

	//Information about size, number of particles, and their springiness
	public float sizeX = 3, sizeY = 3, sizeZ = 3;
	public float windX = 1, windY = 0, windZ = 0;
	public int particlesX = 5, particlesY = 5, particlesZ = 5;
	public float springConstantX = 50, 
	springConstantY = 50, springConstantZ = 50;
	public float dampening = 0.1f, mass = 1, gravity = -9.8f;

	// If a sheet, should we pin the corners?
	public bool pinCornerParticles = true;

	void Awake(){
		Time.timeScale = timeScale;
	}
	void Update(){
		// On mouse press, spawn a new object
		if (Input.GetKeyDown(KeyCode.Space)) {
			createObject ();
		}
	}

	/*
	 * The function to create an object. Will spawn a selected object
	 * in the scene (during runtime) where the click happened.
	 */ 
	public void createObject(){
		if (selectedGeometry == 0) {
			// Create a sheet
			createSheet();

		} else if (selectedGeometry == 1) {
			// Create a cube
			createCube();
		} else if (selectedGeometry == 2) {
			// Create a cube
			createSheetWithWind();
		}
	}
	private void createSheetWithWind(){
		// Hold all info about created gameobjects
		List<List<GameObject>> sheet = new List<List<GameObject>> ();

		// Create a parent object to hold the particles
		GameObject parent = new GameObject ("Sheet");

		// Iterate through all X and Y particles
		for (int i = 0; i < particlesX; i++) {
			sheet.Add (new List<GameObject> ());
			for (int j = 0; j < particlesZ; j++) {
				// Create the gameobject and child it to the parent
				//GameObject g = new GameObject();
				GameObject g = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				g.name = "PointParticle";
				g.transform.parent = parent.transform;
				g.transform.localScale = new Vector3 (0.15f, 0.15f, 0.15f);

				// Adjust the position needed for the size requested
				g.transform.position = new Vector3 (
					(i / (float)particlesX) * sizeX - sizeZ / 2.0f,
					4 + (j / (float)particlesZ) * sizeZ - sizeZ / 2.0f,
					0
				);
				g.AddComponent<SpringParticle> ();
				SpringParticle p = g.GetComponent<SpringParticle> ();
				p.dampening = dampening;
				p.mass = mass;
				p.gravity = new Vector3 (0, gravity, 0);
				p.windForce = new Vector3 (windX, windY, windZ);
				if(j == particlesZ - 1){
					p.staticParticle = true;
				}
				g.AddComponent<Grabber> ();
				sheet [i].Add(g);
			}
		}

		// Create the links between adjacent particles
		for (int i = 0; i < particlesX; i++) {
			for (int j = 0; j < particlesZ; j++) {
				SpringParticle p = sheet [i] [j].GetComponent<SpringParticle> ();
				if (i - 1 >= 0) {
					p.connectedParticles.Add (sheet [i - 1] [j]);
					p.restingDistance.Add (sheet[i-1][j], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j].transform.position
					));
					p.springConstant.Add (sheet[i-1][j], springConstantX);
				} 
				if (i + 1 < particlesX) {
					p.connectedParticles.Add (sheet [i + 1] [j]);
					p.restingDistance.Add (sheet[i+1][j], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j].transform.position
					));
					p.springConstant.Add (sheet[i+1][j], springConstantX);
				}
				if (j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i] [j - 1]);
					p.restingDistance.Add (sheet [i] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i] [j - 1], springConstantZ);
				} 
				if (j + 1 < particlesZ) {
					p.connectedParticles.Add (sheet [i] [j + 1]);
					p.restingDistance.Add (sheet [i] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i] [j + 1], springConstantZ);
				} 
				if (i - 1 >= 0 && j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i - 1] [j - 1]);
					p.restingDistance.Add (sheet [i - 1] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i - 1] [j - 1], springConstantZ);
				}
				if (i - 1 >= 0 && j + 1 < particlesZ) {
					p.connectedParticles.Add (sheet [i - 1] [j + 1]);
					p.restingDistance.Add (sheet [i - 1] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i - 1] [j + 1], springConstantZ);
				}
				if (i + 1 < particlesX && j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i + 1] [j - 1]);
					p.restingDistance.Add (sheet [i + 1] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i + 1] [j - 1], springConstantZ);
				}
				if (i + 1 < particlesX && j + 1 < particlesZ ) {
					p.connectedParticles.Add (sheet [i + 1] [j + 1]);
					p.restingDistance.Add (sheet [i + 1] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i + 1] [j + 1], springConstantZ);
				}
			}
		}
	}
	private void createCube(){
		List<List<List<GameObject>>> cube = new List<List<List<GameObject>>> ();
		GameObject parent = new GameObject ("Cube");
		for (int i = 0; i < particlesX; i++) {
			cube.Add (new List<List<GameObject>> ());
			for (int j = 0; j < particlesZ; j++) {
				cube [i].Add (new List<GameObject> ());
				for (int k = 0; k < particlesY; k++) {
					// Create the gameobject and child it to the parent
					//GameObject g = new GameObject();
					GameObject g = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					g.name = "PointParticle";
					g.transform.parent = parent.transform;
					g.transform.localScale = new Vector3 (0.15f, 0.15f, 0.15f);

					g.transform.position = new Vector3 (
						(i / (float)particlesX) * sizeX - sizeX / 2.0f,
						4 + (k / (float)particlesY) * sizeY - sizeY / 2.0f,
						(j / (float)particlesZ) * sizeZ - sizeZ / 2.0f
					);

					g.AddComponent<SpringParticle> ();
					SpringParticle p = g.GetComponent<SpringParticle> ();
					p.dampening = dampening;
					p.mass = mass;
					p.gravity = new Vector3 (0, gravity, 0);
					if(pinCornerParticles &&( 
						(i == 0 				&& j == 0 				&& k == 0) || 
						(i == 0 				&& j == particlesZ - 1 	&& k == 0) ||
						(i == particlesX - 1 	&& j == 0 				&& k == 0) || 
						(i == particlesX - 1 	&& j == particlesZ - 1 	&& k == 0) ||
						(i == 0 				&& j == 0 				&& k == particlesY - 1) || 
						(i == 0 				&& j == particlesZ - 1 	&& k == particlesY - 1) ||
						(i == particlesX - 1 	&& j == 0 				&& k == particlesY - 1) || 
						(i == particlesX - 1 	&& j == particlesZ - 1 	&& k == particlesY - 1)
					)){
						p.staticParticle = true;
					}
					g.AddComponent<Grabber> ();
					cube [i][j].Add(g);
				}
			}
		}
		for (int i = 0; i < particlesX; i++) {
			for (int j = 0; j < particlesZ; j++) {
				for (int k = 0; k < particlesY; k++) {
					SpringParticle p = cube [i] [j][k].GetComponent<SpringParticle> ();
					if (i - 1 >= 0) {
						p.connectedParticles.Add (cube [i - 1] [j][k]);
						p.restingDistance.Add (cube [i - 1] [j][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j][k].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j][k], springConstantX);
					} 
					if (i + 1 < particlesX) {
						p.connectedParticles.Add (cube [i + 1] [j][k]);
						p.restingDistance.Add (cube [i + 1] [j][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j][k].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j][k], springConstantX);
					}
					if (j - 1 >= 0) {
						p.connectedParticles.Add (cube [i] [j - 1][k]);
						p.restingDistance.Add (cube [i] [j - 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j - 1][k].transform.position
						));
						p.springConstant.Add (cube [i] [j - 1][k], springConstantZ);
					} 
					if (j + 1 < particlesZ) {
						p.connectedParticles.Add (cube [i] [j + 1][k]);
						p.restingDistance.Add (cube [i] [j + 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j + 1][k].transform.position
						));
						p.springConstant.Add (cube [i] [j + 1][k], springConstantZ);
					} 
					if (k - 1 >= 0) {
						p.connectedParticles.Add (cube [i] [j][k - 1]);
						p.restingDistance.Add (cube [i] [j][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j][k - 1].transform.position
						));
						p.springConstant.Add (cube [i] [j][k - 1], springConstantY);
					}
					if (k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i] [j][k + 1]);
						p.restingDistance.Add (cube [i] [j][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j][k + 1].transform.position
						));
						p.springConstant.Add (cube [i] [j][k + 1], springConstantY);
					}
					if (i - 1 >= 0 && j - 1 >= 0) {
						p.connectedParticles.Add (cube [i - 1] [j - 1][k]);
						p.restingDistance.Add (cube [i - 1] [j - 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j - 1][k].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j - 1][k], (springConstantZ + springConstantX) / 2.0f);
					}
					if (i - 1 >= 0 && j + 1 < particlesZ) {
						p.connectedParticles.Add (cube [i - 1] [j + 1][k]);
						p.restingDistance.Add (cube [i - 1] [j + 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j + 1][k].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j + 1][k], (springConstantZ + springConstantX) / 2.0f);
					}
					if (i + 1 < particlesX && j - 1 >= 0) {
						p.connectedParticles.Add (cube [i + 1] [j - 1][k]);
						p.restingDistance.Add (cube [i + 1] [j - 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j - 1][k].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j - 1][k], (springConstantZ + springConstantX) / 2.0f);
					}
					if (i + 1 < particlesX && j + 1 < particlesZ) {
						p.connectedParticles.Add (cube [i + 1] [j + 1][k]);
						p.restingDistance.Add (cube [i + 1] [j + 1][k], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j + 1][k].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j + 1][k], (springConstantZ + springConstantX) / 2.0f);
					}


					if (i - 1 >= 0 && k - 1 >= 0) {
						p.connectedParticles.Add (cube [i - 1] [j][k - 1]);
						p.restingDistance.Add (cube [i - 1] [j][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j][k - 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j][k - 1], (springConstantX + springConstantY) / 2.0f);
					}
					if (i - 1 >= 0 && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i - 1] [j][k + 1]);
						p.restingDistance.Add (cube [i - 1] [j][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j][k + 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j][k + 1], (springConstantX + springConstantZ) / 2.0f);
					}
					if (i + 1 < particlesX && k - 1 >= 0) {
						p.connectedParticles.Add (cube [i + 1] [j][k - 1]);
						p.restingDistance.Add (cube [i + 1] [j][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j][k - 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j][k - 1], (springConstantX + springConstantY) / 2.0f);
					}
					if (i + 1 < particlesX && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i + 1] [j][k + 1]);
						p.restingDistance.Add (cube [i + 1] [j][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j][k + 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j][k + 1], (springConstantX + springConstantY) / 2.0f);
					}


					if (k - 1 >= 0 && j - 1 >= 0) {
						p.connectedParticles.Add (cube [i ] [j - 1][k - 1]);
						p.restingDistance.Add (cube [i] [j - 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j - 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i] [j - 1][k - 1], (springConstantZ + springConstantY) / 2.0f);
					}
					if (k - 1 >= 0 && j + 1 < particlesZ) {
						p.connectedParticles.Add (cube [i] [j + 1][k - 1]);
						p.restingDistance.Add (cube [i] [j + 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j + 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i] [j + 1][k - 1], (springConstantZ + springConstantY) / 2.0f);
					}
					if (k + 1 < particlesX && j - 1 >= 0) {
						p.connectedParticles.Add (cube [i] [j - 1][k + 1]);
						p.restingDistance.Add (cube [i] [j - 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j - 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i] [j - 1][k + 1], (springConstantZ + springConstantY ) / 2.0f);
					}
					if (k + 1 < particlesX && j + 1 < particlesZ) {
						p.connectedParticles.Add (cube [i] [j + 1][k + 1]);
						p.restingDistance.Add (cube [i] [j + 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i] [j + 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i] [j + 1][k + 1], (springConstantZ + springConstantY ) / 2.0f);
					}

					//4 outer corners of cube
					if (i - 1 >= 0 && j - 1 >= 0 && k-1 >= 0) {
						p.connectedParticles.Add (cube [i - 1] [j - 1][k - 1]);
						p.restingDistance.Add (cube [i - 1] [j - 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j - 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j - 1][k - 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i - 1 >= 0 && j + 1 < particlesZ && k - 1 >= 0) {
						p.connectedParticles.Add (cube [i - 1] [j + 1][k - 1]);
						p.restingDistance.Add (cube [i - 1] [j + 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j + 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j + 1][k - 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i + 1 < particlesX && j - 1 >= 0 && k - 1 >= 0) {
						p.connectedParticles.Add (cube [i + 1] [j - 1][k - 1]);
						p.restingDistance.Add (cube [i + 1] [j - 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j - 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j - 1][k - 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i + 1 < particlesX && j + 1 < particlesZ && k - 1 >= 0) {
						p.connectedParticles.Add (cube [i + 1] [j + 1][k - 1]);
						p.restingDistance.Add (cube [i + 1] [j + 1][k - 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j + 1][k - 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j + 1][k - 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}

					// 4 outer corners of cube
					if (i - 1 >= 0 && j - 1 >= 0 && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i - 1] [j - 1][k + 1]);
						p.restingDistance.Add (cube [i - 1] [j - 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j - 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j - 1][k + 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i - 1 >= 0 && j + 1 < particlesZ && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i - 1] [j + 1][k + 1]);
						p.restingDistance.Add (cube [i - 1] [j + 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i - 1] [j + 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i - 1] [j + 1][k + 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i + 1 < particlesX && j - 1 >= 0 && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i + 1] [j - 1][k + 1]);
						p.restingDistance.Add (cube [i + 1] [j - 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j - 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j - 1][k + 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
					if (i + 1 < particlesX && j + 1 < particlesZ && k + 1 < particlesY) {
						p.connectedParticles.Add (cube [i + 1] [j + 1][k + 1]);
						p.restingDistance.Add (cube [i + 1] [j + 1][k + 1], Vector3.Distance (
							cube [i] [j][k].transform.position, cube [i + 1] [j + 1][k + 1].transform.position
						));
						p.springConstant.Add (cube [i + 1] [j + 1][k + 1], (springConstantZ + springConstantY +springConstantX) / 3.0f);
					}
				}
			}
		}
	}

	private void createSheet(){
		// Hold all info about created gameobjects
		List<List<GameObject>> sheet = new List<List<GameObject>> ();

		// Create a parent object to hold the particles
		GameObject parent = new GameObject ("Sheet");

		// Iterate through all X and Y particles
		for (int i = 0; i < particlesX; i++) {
			sheet.Add (new List<GameObject> ());
			for (int j = 0; j < particlesZ; j++) {
				// Create the gameobject and child it to the parent
				//GameObject g = new GameObject();
				GameObject g = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				g.name = "PointParticle";
				g.transform.parent = parent.transform;
				g.transform.localScale = new Vector3 (0.15f, 0.15f, 0.15f);

				// Adjust the position needed for the size requested
				g.transform.position = new Vector3 (
					(i / (float)particlesX) * sizeX - sizeX / 2.0f,
					4,
					(j / (float)particlesZ) * sizeZ - sizeZ / 2.0f
				);
				g.AddComponent<SpringParticle> ();
				SpringParticle p = g.GetComponent<SpringParticle> ();
				p.dampening = dampening;
				p.mass = mass;
				p.gravity = new Vector3 (0, gravity, 0);
				if(pinCornerParticles && 
						(  (i == 0					&& j == 0) 
						|| (i == 0 					&& j == particlesZ - 1) 
						|| (i == particlesX - 1 	&& j == 0) 
						|| (i == particlesX - 1 	&& j == particlesZ - 1)
					)){
					p.staticParticle = true;
				}
				g.AddComponent<Grabber> ();
				sheet [i].Add(g);
			}
		}

		// Create the links between adjacent particles
		for (int i = 0; i < particlesX; i++) {
			for (int j = 0; j < particlesZ; j++) {
				SpringParticle p = sheet [i] [j].GetComponent<SpringParticle> ();
				if (i - 1 >= 0) {
					p.connectedParticles.Add (sheet [i - 1] [j]);
					p.restingDistance.Add (sheet[i-1][j], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j].transform.position
					));
					p.springConstant.Add (sheet[i-1][j], springConstantX);
				} 
				if (i + 1 < particlesX) {
					p.connectedParticles.Add (sheet [i + 1] [j]);
					p.restingDistance.Add (sheet[i+1][j], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j].transform.position
					));
					p.springConstant.Add (sheet[i+1][j], springConstantX);
				}
				if (j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i] [j - 1]);
					p.restingDistance.Add (sheet [i] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i] [j - 1], springConstantZ);
				} 
				if (j + 1 < particlesZ) {
					p.connectedParticles.Add (sheet [i] [j + 1]);
					p.restingDistance.Add (sheet [i] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i] [j + 1], springConstantZ);
				} 
				if (i - 1 >= 0 && j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i - 1] [j - 1]);
					p.restingDistance.Add (sheet [i - 1] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i - 1] [j - 1], springConstantZ);
				}
				if (i - 1 >= 0 && j + 1 < particlesZ) {
					p.connectedParticles.Add (sheet [i - 1] [j + 1]);
					p.restingDistance.Add (sheet [i - 1] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i - 1][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i - 1] [j + 1], springConstantZ);
				}
				if (i + 1 < particlesX && j - 1 >= 0) {
					p.connectedParticles.Add (sheet [i + 1] [j - 1]);
					p.restingDistance.Add (sheet [i + 1] [j - 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j - 1].transform.position
					));
					p.springConstant.Add (sheet [i + 1] [j - 1], springConstantZ);
				}
				if (i + 1 < particlesX && j + 1 < particlesZ ) {
					p.connectedParticles.Add (sheet [i + 1] [j + 1]);
					p.restingDistance.Add (sheet [i + 1] [j + 1], Vector3.Distance (
						sheet[i][j].transform.position, sheet[i + 1][j + 1].transform.position
					));
					p.springConstant.Add (sheet [i + 1] [j + 1], springConstantZ);
				}
			}
		}


	}
}


