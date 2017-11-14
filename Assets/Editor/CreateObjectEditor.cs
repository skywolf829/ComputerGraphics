using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreateObject))]
public class CreateObjectEditor: Editor {

	CreateObject script;

	public override void OnInspectorGUI(){
		script = (CreateObject)target;
		script.timeScale = EditorGUILayout.FloatField ("Timescale", script.timeScale);
		script.selectedGeometry = EditorGUILayout.Popup 
			("Select geometry type", script.selectedGeometry, script.geometryTypes);

		EditorGUILayout.Separator ();

		script.sizeX = Mathf.Clamp(EditorGUILayout.FloatField 
			("Size of object in X direction", script.sizeX),
			1, 50);
		if (script.selectedGeometry == 1) {
			script.sizeY = Mathf.Clamp (EditorGUILayout.FloatField 
				("Size of object in Y direction", script.sizeY),
				1, 50);
		}
		script.sizeZ = Mathf.Clamp(EditorGUILayout.FloatField 
			("Size of object in Z direction", script.sizeZ),
			1, 50);

		EditorGUILayout.Separator ();

		script.particlesX = Mathf.Clamp(EditorGUILayout.IntField 
			("Number of particles in X direction", script.particlesX),
			0, 100);
		if (script.selectedGeometry == 1) {
			script.particlesY = Mathf.Clamp (EditorGUILayout.IntField 
				("Number of particles in Y direction", script.particlesY),
				0, 100);
		}
		script.particlesZ = Mathf.Clamp(EditorGUILayout.IntField 
			("Number of particles in Z direction", script.particlesZ),
			0, 100);

		EditorGUILayout.Separator ();

		script.springConstantX = Mathf.Clamp(EditorGUILayout.FloatField 
			("Spring constant in X direction", script.springConstantX),
			0, 5000);
		if (script.selectedGeometry == 1) {
			script.springConstantY = Mathf.Clamp (EditorGUILayout.FloatField 
				("Spring constant in Y direction", script.springConstantY),
				0, 5000);
		}
		script.springConstantZ = Mathf.Clamp(EditorGUILayout.FloatField 
			("Spring constant in Z direction", script.springConstantZ),
			0, 5000);
		
		if (script.selectedGeometry == 2) {
			script.windX = Mathf.Clamp (EditorGUILayout.FloatField 
				("Wind force in X direction", script.windX),
				0, 500);
			script.windY = Mathf.Clamp (EditorGUILayout.FloatField 
				("Wind force in Y direction", script.windY),
				0, 500);
			script.windZ = Mathf.Clamp (EditorGUILayout.FloatField 
				("Wind force in Z direction", script.windZ),
				0, 500);
		}

		EditorGUILayout.Space ();

		script.dampening = Mathf.Clamp(EditorGUILayout.FloatField
			("Dampening", script.dampening), 0, 10);
		script.mass = Mathf.Clamp(EditorGUILayout.FloatField
			("Mass", script.mass), 0.1f, 100);
		script.gravity = Mathf.Clamp (EditorGUILayout.FloatField
			("Gravity", script.gravity), -20, 20);
		

		script.pinCornerParticles = EditorGUILayout.Toggle (
			"Pin corner particles", script.pinCornerParticles);
		

		EditorGUILayout.Space ();

	}

}
