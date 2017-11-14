using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * A custom inspector script used to give GenerateMesh.cs a GUI to create the mesh
 */
[CustomEditor(typeof(GenerateMesh))]
public class GenerateMeshEditor : Editor {

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();
		GenerateMesh script = (GenerateMesh)target;
		if (GUILayout.Button ("Generate mesh")) {
			script.Generate ();
		}
	}
}
