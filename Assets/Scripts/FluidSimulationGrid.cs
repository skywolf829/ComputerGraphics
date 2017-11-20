using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulationGrid : MonoBehaviour {


	class Grid{

		private float cellSize;
		private Vector3 volume;
		private Vector3 center;

		Dictionary<Vector3Int, float> scalarGrid;

		public Grid(float cellsize, Vector3 v, Vector3 origin){
			cellSize = cellsize;
			volume = v;
			center = origin;
			createGrid();
		}

		public void createGrid(){
			scalarGrid = new Dictionary<Vector3Int, float> ();
			for (float x = center.x-volume.x / 2.0f; x < center.x+volume.x / 2.0f; x += cellSize) {
				for (float y = center.y-volume.y / 2.0f; y < center.y+volume.y / 2.0f; y += cellSize) {
					for (float z = center.z-volume.z / 2.0f; z < center.y+volume.z / 2.0f; z += cellSize) {
						scalarGrid.Add (
							new Vector3Int ((int)(x / cellSize), 
								(int)(y / cellSize), (int)(z / cellSize)), 0);
					}
				}
			}
		}

		public float scalarAtPosition(Vector3 pos){
			if(pos.x < center.x - volume.x / 2.0f || pos.x > center.x + volume.x / 2.0f ||
				pos.y < center.y - volume.y / 2.0f || pos.y > center.y + volume.y / 2.0f ||
				pos.z < center.z - volume.z / 2.0f || pos.z > center.z + volume.z / 2.0f)
			return float.NaN;
			
			int x = (int)((pos.x - center.x + volume.x / 2.0f) / cellSize);
			int y = (int)((pos.y - center.y + volume.y / 2.0f) / cellSize);
			int z = (int)((pos.z - center.z + volume.z / 2.0f) / cellSize);

			float result;
			scalarGrid.TryGetValue (new Vector3Int (x, y, z), out result);

			return result;
		}

		public void Update(){

		}

	}



	public float cellSize;
	public Vector3 volume;
	public Vector3 center;

	Grid g;

	void Start () {
		g = new Grid (cellSize, volume, center);
	}

	public float GetScalarAtPosition(Vector3 pos){
		return g.scalarAtPosition (pos);
	}	

	void Update () {
		g.Update ();
	}
}
