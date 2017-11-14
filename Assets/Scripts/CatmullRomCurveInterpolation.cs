using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CatmullRomCurveInterpolation : MonoBehaviour {

	// Used for the arc length lookup table
	public class Pair{
		public float x, y;
		public Pair(float x, float y){
			this.x = x;
			this.y = y;
		}
	}

	// Change this to adjust how many points are in the curve
	public int NumberOfPoints = 3;
	// Change this to see debug messages
	public bool DEBUG_MODE = false;
	Vector3[] controlPoints;

	// Used for distance interpolation
	List<Pair> arcLengthLookup = new List<Pair>();


	// Control constants
	const int MinX = -50;
	const int MinY = -50;
	const int MinZ = 0;

	const int MaxX = 50;
	const int MaxY = 50;
	const int MaxZ = 50;

	// How "fast" the plane will travel in the update loop each cycle
	public float distanceStep = 0.1f;
	// How accurate the table should be. Recommended 0.001f for 
	//	reasonable accuracy without a long processing period
	public float tableResolution = 0.001f;

	// Tracks how much time has passed in the update loop
	float passedTime = 0f;
	// Tracks how much distance has been traveled in update loop
	float distanceTraveled = 0f;
	// Used for CatmullRom interpolation
	float tao = 0.5f;
	// Used to track the total distance traveled by the catmullrom curve
	float totalDistance = 0f;
	float totalTime = 0f;

	/* Returns a point on a cubic Catmull-Rom/Blended Parabolas curve
	 * u is a scalar value from 0 to 1
	 * segmentNumber indicates which 4 points to use for interpolation
	 */
	Vector3 ComputePointOnCatmullRomCurve(float u, int segmentNumber)
	{
		//Don't extrapolate
		u = Mathf.Min (1, u);
		u = Mathf.Max (0, u);

		Vector3 point = new Vector3();
		Vector3 pointGradient = new Vector3 ();

		// Calculate the coefficients for our new position
		// P(t) = a * u^3 + b * u^2 + c * u + d
		Vector3 d = controlPoints [segmentNumber];
		Vector3 c = -tao * controlPoints [(segmentNumber + NumberOfPoints - 1) % NumberOfPoints]
		            + tao * controlPoints [(segmentNumber + 1) % NumberOfPoints];
		Vector3 b = 2 * tao * controlPoints [(segmentNumber + NumberOfPoints - 1) % NumberOfPoints]
		            + (tao - 3) * controlPoints [segmentNumber]
		            + (3 - (2 * tao)) * controlPoints [(segmentNumber + 1) % NumberOfPoints]
		            - tao * controlPoints [(segmentNumber + 2) % NumberOfPoints];
		Vector3 a = -tao * controlPoints [(segmentNumber + NumberOfPoints - 1) % NumberOfPoints]
		            + (2 - tao) * controlPoints [segmentNumber]
		            + (tao - 2) * controlPoints [(segmentNumber + 1) % NumberOfPoints]
		            + tao * controlPoints [(segmentNumber + 2) % NumberOfPoints];

		//Calculate the point
		point = a * Mathf.Pow (u, 3) + b * Mathf.Pow (u, 2) + c * u + d;
		//Calculate the gradient
		// P'(t) = 3*a*u^2 + 2 * b * u + c
		pointGradient = 3 * a * Mathf.Pow (u, 2) + 2 * b * u + c;

		//Make the object look at the direction it's moving in
		transform.LookAt (transform.position + pointGradient);

		return point;
	}
	float findUForDistance(float dist){
		float closestLower = 0, closestHigher = NumberOfPoints;
		int i = 0;

		//Find the closest distance below and above to find the u value to return
		for (i = 0; i < arcLengthLookup.Count; i++) {
			float tempDist = arcLengthLookup[i].y;
			if (dist > tempDist) {
				closestLower = tempDist;
			} else if (dist < tempDist) {
				closestHigher = tempDist;
				break;
			}
			else{
				return arcLengthLookup[i].x;
			}
		}
		//Lerp between the i-1th index and ith index by the distance between the closest
		//	two options
		return Mathf.Lerp (arcLengthLookup [i - 1].x, arcLengthLookup [i].x, 
			((dist - closestLower) / (closestHigher - closestLower)));
	}
	float findDistForTime(float t){
		return (-2 * Mathf.Pow (t, 3) + 3 * Mathf.Pow(t, 2)) * totalDistance;
	}
	void GenerateControlPointGeometry()
	{
		for(int i = 0; i < NumberOfPoints; i++)
		{
			GameObject tempcube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			tempcube.transform.localScale -= new Vector3(0.8f,0.8f,0.8f);
			tempcube.transform.position = controlPoints[i];
		}	
	}
	
	// Use this for initialization
	void Start () {

		controlPoints = new Vector3[NumberOfPoints];
		
		// set points randomly...
		controlPoints[0] = new Vector3(0,0,0);
		for(int i = 1; i < NumberOfPoints; i++)
		{
			controlPoints[i] = new Vector3(Random.Range(MinX,MaxX),Random.Range(MinY,MaxY),Random.Range(MinZ,MaxZ));
		}

		//Initialize the position to the first control point
		transform.position = controlPoints [0];

		//Create a lookup table for the u value and the lengths
		Vector3 temp = transform.position;

		bool finished = false;
		//Simulate going through the entire animation, very smilar to the Update loop
		//	except the transform isn't moved, just temp, and the table is being updated
		//	This way, the lookuptable can be used to find the U value
		arcLengthLookup.Add(new Pair(0, 0));
		while (!finished) {			
			totalTime += tableResolution;
			Vector3 newPos = ComputePointOnCatmullRomCurve(totalTime % 1.0f, (int)(totalTime) % NumberOfPoints);
			totalDistance += Vector3.Distance (temp, newPos);
			arcLengthLookup.Add (new Pair(totalTime, totalDistance));
			temp = newPos;
			if (totalTime > NumberOfPoints) {
				finished = true;
			}
		}
		GenerateControlPointGeometry();
	}
	
	// Update is called once per frame
	void Update () {
		passedTime += Time.deltaTime;

		//Use the following line to disable ease-in ease-out
		//distanceTraveled += distanceStepTemp;

		//Use the following line to use ease in ease out
		distanceTraveled = findDistForTime ((passedTime % totalTime) / totalTime);

		//Use the following 2 lines for distance based interpolation
		float u = findUForDistance (distanceTraveled % totalDistance);
		Vector3 temp = ComputePointOnCatmullRomCurve(u % 1.0f, (int)(u) % NumberOfPoints);

		//Use the following line for time based interpolation
		//Vector3 temp = ComputePointOnCatmullRomCurve(passedTime % 1.0f, (int)(passedTime) % NumberOfPoints);

		if (DEBUG_MODE) {
			print ("Traveled " + Vector3.Distance (temp, transform.position) + " in this update cycle.");
		}

		transform.position = temp;

		if (DEBUG_MODE) {
			GameObject g = GameObject.CreatePrimitive (PrimitiveType.Sphere);
			g.transform.position = transform.position;
		}

	}
}
