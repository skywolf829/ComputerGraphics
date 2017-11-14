using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateMesh : MonoBehaviour {
	// A public material used to texture the created object
	public Material mat;
	public string location;
	List<Vector3> verts;
	List<Vector3> vertexNormalValues;
	Vector3[] vertexNorms;
	List<int> faces;

	bool vertexNormsCreated = false;

	private void getVertsAndFaces(){
		StreamReader f = new StreamReader (location);
		string line;
		verts = new List<Vector3> ();
		vertexNormalValues = new List<Vector3> ();
		faces = new List<int> ();
		while ((line = f.ReadLine()) != null) {
			if (line.StartsWith ("v ")) {
				line = line.Substring (2, line.Length - 2);
				int s1 = line.IndexOf (" ");
				int s2 = line.LastIndexOf (" ");
				float v1 = float.Parse (line.Substring (0, s1));
				float v2 = float.Parse (line.Substring (s1 + 1, s2 - s1 - 1));
				float v3 = float.Parse (line.Substring (s2 + 1));
				Vector3 v = new Vector3 (v1, v2, v3);
				verts.Add (v);
			} else if (line.StartsWith ("vn ")) {
				line = line.Substring (3, line.Length - 3);
				int s1 = line.IndexOf (" ");
				int s2 = line.LastIndexOf (" ");
				float v1 = float.Parse (line.Substring (0, s1));
				float v2 = float.Parse (line.Substring (s1 + 1, s2 - s1 - 1));
				float v3 = float.Parse (line.Substring (s2 + 1));
				Vector3 v = new Vector3 (v1, v2, v3);
				vertexNormalValues.Add (v);
			}
			else if (line.StartsWith ("f ")) {
				if (!vertexNormsCreated) {
					vertexNorms = new Vector3[verts.Count];
				}
				line = line.Substring (2, line.Length - 2);
				int v1 = int.Parse (line.Substring (0, line.IndexOf ("//"))) - 1;
				Vector3 v1n = vertexNormalValues[int.Parse (line.Substring (line.IndexOf ("//") + 2,
					line.IndexOf (" ") - line.IndexOf ("//") - 2)) - 1];

				line = line.Substring (line.IndexOf (" ") + 1);
				int v2 = int.Parse (line.Substring (0, line.IndexOf ("//"))) - 1;
				//print (int.Parse (line.Substring (line.IndexOf ("//") + 2, line.IndexOf (" ") - line.IndexOf ("//") - 2)));
				//print (vertexNormalValues.Count);
				Vector3 v2n = vertexNormalValues[int.Parse (line.Substring (line.IndexOf ("//") + 2, 
					line.IndexOf (" ") - line.IndexOf ("//") - 2)) - 1];
				
				line = line.Substring (line.IndexOf (" ") + 1);
				int v3 = int.Parse (line.Substring (0, line.IndexOf ("//"))) - 1;
				Vector3 v3n = vertexNormalValues[int.Parse (line.Substring (line.IndexOf ("//") + 2, 
					line.Length - line.IndexOf ("//") - 2)) - 1];
				
				faces.Add (v1);
				faces.Add (v2);
				faces.Add (v3);

				vertexNorms [v1] = v1n;
				vertexNorms [v2] = v2n;
				vertexNorms [v3] = v3n;
			}
		}
	}
	public void Generate(){
		getVertsAndFaces ();
		// Create a standard mesh
		Mesh mesh = new Mesh();

		mesh.vertices = verts.ToArray ();	
		mesh.triangles = faces.ToArray ();
		mesh.normals = vertexNorms;
		AssetDatabase.CreateAsset( mesh, location + "Mesh" );			
		AssetDatabase.SaveAssets();

		// Create a gameobject in the scene called Generated object
		GameObject go = new GameObject ("Generated object");
		// Add a MeshFilter with the created mesh 
		go.AddComponent<MeshFilter> ();
		go.GetComponent<MeshFilter> ().mesh = mesh;

		// Add a mesh renderer so it is visible in the scene
		go.AddComponent<MeshRenderer> ();
		// If a material was picked, assign the meshrenderer's material
		if (mat) {
			go.GetComponent<MeshRenderer> ().material = mat;
		}
		//So the object can interact with the terrain, add a meshcollider and rigidbody
		go.AddComponent<CapsuleCollider> ();
		go.AddComponent<Rigidbody> ();
		//Freeze rotation in the x/z plane so that the object only rotates on the Y axis
		go.GetComponent<Rigidbody>().constraints = 
			RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
	}
}
