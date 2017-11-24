using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation : MonoBehaviour {

	class Cell{
		public float pressure;
		public Vector3 velocity;
		public Vector3 tempVelocity;
		public Vector3Int index;

		public int layer;
		public string type;
		public Cell(Vector3Int index){			
			pressure = 0;
			velocity = Vector3.zero;
			this.index = index;
		}

	}

	class Grid{
        
		const float CFL_CONSTANT = 3;
		const float MIN_TIMESTEP = 0.008333f;
		const float MAX_TIMESTEP = 0.05f;

		private float cellSize;
		private Vector3 volume;
		private Vector3 center;

		Hashtable cells = new Hashtable();

		public Grid(float cellsize, Vector3 v, Vector3 origin){
			cellSize = cellsize;
			volume = v;
			center = origin;
		}
	
		Vector3 PressureGradientAt(Vector3 pos){
			Vector3 result = Vector3.zero;
			Cell c = GetCellAt (pos);
			if (c != null) {

				Cell aside = GetCellAt (pos - new Vector3 (cellSize, 0, 0));
				Cell below = GetCellAt (pos - new Vector3 (0, cellSize, 0));
				Cell behind = GetCellAt (pos - new Vector3 (0, 0, cellSize));

				if(aside != null)
					result.x = c.pressure - aside.pressure;

				if(below != null)
					result.y = c.pressure - below.pressure;

				if(behind != null)
					result.z = c.pressure - behind.pressure;
			}
			return result;
		}
        Vector3 VelocityGradientAt(Vector3 pos)
        {
            Vector3 result = Vector3.zero;
            Cell c = GetCellAt(pos);
            if (c != null)
            {

                Cell aside = GetCellAt(pos - new Vector3(cellSize, 0, 0));
                Cell below = GetCellAt(pos - new Vector3(0, cellSize, 0));
                Cell behind = GetCellAt(pos - new Vector3(0, 0, cellSize));
                result = c.velocity;
                //Debug.Log("Cell: " + c.index + " velocity is " + result);
                if (aside != null)
                {
                    result.x = c.velocity.x - aside.velocity.x;
                    //Debug.Log("Aside velocity is " + aside.velocity);

                }

                if (below != null)
                {
                    result.y = c.velocity.y - below.velocity.y;
                    //Debug.Log("Below velocity is " + below.velocity);
                }

                if (behind != null)
                {
                    result.z = c.velocity.z - behind.velocity.z;
                    //Debug.Log("Behind velocity is " + behind.velocity);
                }
            }
            //Debug.Log("End velocity gradient of " + c.index + ": " + result);
			return result;
		}

		float DivergenceAt(Vector3 pos){
			Vector3 result = VelocityGradientAt (pos);
			return result.x + result.y + result.z;
		}

		float PressureLaplacianAt(Vector3 pos){
			float result = 0;
			Cell c = GetCellAt (pos);
			if (c != null) {
				int num = 0;

				Cell left = GetCellAt (pos - new Vector3 (cellSize, 0, 0));
				Cell right = GetCellAt (pos + new Vector3 (cellSize, 0, 0));
				Cell below = GetCellAt (pos - new Vector3 (0, cellSize, 0));
				Cell above = GetCellAt (pos + new Vector3 (0, cellSize, 0));
				Cell behind = GetCellAt (pos - new Vector3 (0, 0, cellSize));
				Cell forward = GetCellAt (pos + new Vector3 (0, 0, cellSize));

				if (left != null) {
					result += left.pressure;
					num++;
				}
				if (right != null) {
					result += right.pressure;
					num++;
				}
				if (below != null) {
					result += below.pressure;
					num++;
				}
				if (above != null) {
					result += above.pressure;
					num++;
				}
				if (behind != null) {
					result += behind.pressure;
					num++;
				}
				if (forward != null) {
					result += forward.pressure;
					num++;
				}
				result += num * c.pressure;
			}
			return result;
		}

		Vector3 VelocityLaplacianAt(Vector3 pos){
			Vector3 result = Vector3.zero;
			Cell c = GetCellAt (pos);
			if (c != null) {
				Vector3 gradient = VelocityGradientAt (pos);

				int num = 0;

				Vector3 left = VelocityGradientAt (pos - new Vector3 (cellSize, 0, 0));
				Vector3 right = VelocityGradientAt (pos + new Vector3 (cellSize, 0, 0));
				Vector3 below = VelocityGradientAt (pos - new Vector3 (0, cellSize, 0));
				Vector3 above = VelocityGradientAt (pos + new Vector3 (0, cellSize, 0));
				Vector3 behind = VelocityGradientAt (pos - new Vector3 (0, 0, cellSize));
				Vector3 forward = VelocityGradientAt (pos + new Vector3 (0, 0, cellSize));

				if (left != null) {
					result.x += left.x;
					result.y += left.y;
					result.z += left.z;
					num++;
				}
				if (right != null) {
					result.x += right.x;
					result.y += right.y;
					result.z += right.z;
					num++;
				}
				if (below != null) {
					result.x += below.x;
					result.y += below.y;
					result.z += below.z;
					num++;
				}
				if (above != null) {
					result.x += above.x;
					result.y += above.y;
					result.z += above.z;
					num++;
				}
				if (behind != null) {
					result.x += behind.x;
					result.y += behind.y;
					result.z += behind.z;
					num++;
				}
				if (forward != null) {
					result.x += forward.x;
					result.y += forward.y;
					result.z += forward.z;
					num++;
				}
				result += num * gradient;
			}
			return result;
		}

		Cell TraceParticle(Vector3 pos, float timestep){
			Vector3 V = GetVelocity(pos);
			V = GetVelocity(new Vector3(pos.x+0.5f*timestep*V.x, 
                                        pos.y+0.5f*timestep*V.y, 
                                        pos.z+0.5f*timestep*V.z)); 
			Vector3 newPos = new Vector3 (pos.x + timestep * V.x, 
				                 pos.y + timestep * V.y,
				                 pos.z + timestep * V.z);
			return GetCellAt (newPos);
		}
        Vector3 GetVelocity(Vector3 pos){
            return GetInterpolatedValue(pos); 
		}
		Vector3 GetInterpolatedValue(Vector3 pos){
            Vector3 result = Vector3.zero;

            Cell c = GetCellAt(pos);
            Cell forward = GetCellAt(pos + new Vector3(0, 0,cellSize));
            Cell above = GetCellAt(pos + new Vector3(0, cellSize, 0));
            Cell right = GetCellAt(pos + new Vector3(cellSize, 0, 0));

            if(c != null){
                result = c.velocity;
                Vector3 cCellGlobalSpot = ToVector3(c.index) * cellSize;

                if (forward != null){
                    result.z = Mathf.Lerp(c.velocity.z, forward.velocity.z, 
                                          (pos.z - cCellGlobalSpot.z) / cellSize);
                }
                if (above != null)
                {
                    result.y = Mathf.Lerp(c.velocity.y, above.velocity.y, 
                                          (pos.y - cCellGlobalSpot.y) / cellSize);
                }
                if (right != null)
                {
                    result.x = Mathf.Lerp(c.velocity.x, right.velocity.x, 
                                          (pos.x - cCellGlobalSpot.x) / cellSize);
                }
            }

            return result;
		}
		float GetMaxVelocity(){
			float maxSpeed = 0;
			foreach (DictionaryEntry entry in cells)
			{
				float cellSpeed = ((Cell)(entry.Value)).velocity.magnitude;
				if(cellSpeed> maxSpeed) {
					maxSpeed = cellSpeed;
				}
			}
			return maxSpeed;
		}
		public float GetTimeStep(){
			return Mathf.Min(
				Mathf.Max(CFL_CONSTANT * cellSize / GetMaxVelocity (), MIN_TIMESTEP), 
				MAX_TIMESTEP
			);
		}
		Cell GetCellAt(Vector3 pos){
			Cell c = null;
			Vector3Int xyz = GetIndexFor(pos);
			if(cells.ContainsKey(hash(xyz))){
				c = (Cell)cells[hash(xyz)];
			}
			return c;
		}
		Vector3Int GetIndexFor(Vector3 pos){
			pos += Vector3.one * cellSize / 2.0f;
			return new Vector3Int (Mathf.FloorToInt(pos.x / cellSize),
				Mathf.FloorToInt(pos.y / cellSize), 
				Mathf.FloorToInt(pos.z / cellSize));
		}
		int hash(Vector3Int xyz){
			return 541 * xyz.x + 79 * xyz.y + 31 * xyz.z;
		}
		bool WithinBounds(Vector3Int pos){
			int minx = Mathf.FloorToInt((center.x - volume.x / 2.0f) / cellSize);
			int miny = Mathf.FloorToInt((center.y - volume.y / 2.0f) / cellSize);
			int minz = Mathf.FloorToInt((center.z - volume.z / 2.0f) / cellSize);
			int maxx = Mathf.FloorToInt((center.x + volume.x / 2.0f) / cellSize);
			int maxy = Mathf.FloorToInt((center.y + volume.y / 2.0f) / cellSize);
			int maxz = Mathf.FloorToInt((center.z + volume.z / 2.0f) / cellSize);
			return (pos.x < maxx && pos.x > minx) &&
			(pos.y < maxy && pos.y > miny) &&
			(pos.z < maxz && pos.z > minz);
		}
		void ApplyTempVelocities(){
			foreach (DictionaryEntry entry in cells) {
				Cell c = ((Cell)entry.Value);
				c.velocity = c.tempVelocity;
				c.tempVelocity = Vector3.zero;
			}
		}
		Vector3 ToVector3(Vector3Int input){
			return new Vector3 (input.x, input.y, input.z);
		}
		public void UpdateGrid(HashSet<GameObject> particles){

			// Reset all cells
			foreach (DictionaryEntry entry in cells) {
				Cell c = (Cell)(entry.Value);
				c.layer = -1;
			}

			// Update cells with fluid
			foreach (GameObject particle in particles) {
				Cell c = GetCellAt (particle.transform.position);
				Vector3Int xyz = GetIndexFor (particle.transform.position);
				if (c == null) {
					if (WithinBounds (xyz)) {
						c = new Cell (xyz);
						c.layer = 0;
						c.type = "fluid";
						cells.Add (hash(xyz), c);
					} 	
				} 
				else{
					if (c.type != "solid") {
						c.type = "fluid";
						c.layer = 0;
					}
				}
			}
			// Create buffer zone
			for (int i = 1; i <= Mathf.Max (2, CFL_CONSTANT); i++) {
				List<Cell> cellsToAdd = new List<Cell>();
				foreach (DictionaryEntry entry in cells) {
					Cell c = (Cell)(entry.Value);
					if (c.layer == i - 1) {
						List<Vector3Int> neighbors = new List<Vector3Int> ();
						neighbors.Add(c.index + new Vector3Int (0, 1, 0));
						neighbors.Add(c.index + new Vector3Int (0, -1, 0));
						neighbors.Add(c.index + new Vector3Int (0, 0, 1));
						neighbors.Add(c.index + new Vector3Int (0, 0, -1));
						neighbors.Add(c.index + new Vector3Int (1, 0, 0));
						neighbors.Add(c.index + new Vector3Int (-1, 0, 0));
						foreach (Vector3Int n in neighbors) {
							if (cells.ContainsKey (hash(n))) {
								Cell neighbor = (Cell)cells[hash(n)];
								if (neighbor.layer == -1 && neighbor.type != "solid") {
									neighbor.type = "air";
								}
								neighbor.layer = i;
							} else {
								Cell neighbor = new Cell (n);
								neighbor.layer = i;
								if (WithinBounds (n)) {
									neighbor.type = "air";
								} else {
									neighbor.type = "solid";
								}
								cellsToAdd.Add (neighbor);
							}
						}
					}
				}
				foreach (Cell k in cellsToAdd) {
					if (!cells.ContainsKey (hash (k.index))) {
						cells.Add (hash (k.index), k);
					}
				}
			}

			// Delete old cells
			List<int> cellsToRemove = new List<int>();
			foreach(DictionaryEntry entry in cells){
				Cell c = (Cell)entry.Value;
				if (c.layer == -1) {
					cellsToRemove.Add ((int)(entry.Key));
				}
			}
			foreach (int k in cellsToRemove) {
				cells.Remove (k);
			}
		}

		public void AdvanceVelocityField(float timestep, float gravity, float viscosity,
			float density, float atmPressure){
            if (cells.Count == 0) return;
			int numFluidCells = 0;

            // Convection via backwards particle trace
            //Debug.Log("Convection");
			foreach (DictionaryEntry entry in cells) {
				Cell c = (Cell)entry.Value;		
				Cell traceCell = TraceParticle (ToVector3 (c.index) * cellSize, -timestep);
				if (traceCell != null) {
					c.tempVelocity = traceCell.velocity;
				} else {
					c.tempVelocity = Vector3.zero;
				}
				if (c.type == "fluid") {
					numFluidCells++;
				}
			}
			ApplyTempVelocities ();

            // External forces
            //Debug.Log("External Forces");
			foreach (DictionaryEntry entry in cells) {
				Cell c = ((Cell)entry.Value);
				if (c.type == "fluid") {
					c.velocity += new Vector3 (0, timestep * gravity, 0);
				}
                else {
                    Cell above = GetCellAt(ToVector3(c.index) * cellSize +
                                           new Vector3(0, cellSize, 0));
                    if(above != null && above.type == "fluid"){
                        c.velocity += new Vector3(0, timestep * gravity, 0); 
                    }
                }
			}

            // Viscosity
            //Debug.Log("Viscosity");
			foreach (DictionaryEntry entry in cells) {
				Cell c = ((Cell)entry.Value);
                Vector3 l = VelocityLaplacianAt (ToVector3(c.index) * cellSize);
				c.tempVelocity = c.velocity + timestep * l * viscosity;
			}
			ApplyTempVelocities ();

            // Calculate pressure
            //Debug.Log("Pressure");
			Dictionary<Cell, int> cellToRow = new Dictionary<Cell, int>();

			alglib.sparsematrix a;
			double[] b = new double[numFluidCells];
			alglib.sparsecreate(numFluidCells, numFluidCells, out a);

			foreach (DictionaryEntry entry in cells) {
				Cell c = ((Cell)entry.Value);
				if (c.type == "fluid") {
					int num = 0, numAirCells = 0;
					int row = 0, leftRow = 0, rightRow = 0, 
					belowRow = 0, aboveRow = 0, behindRow = 0, 
					forwardRow = 0;
					bool added = false;

					Cell left = GetCellAt (ToVector3(c.index) * cellSize - new Vector3 (cellSize, 0, 0));
					Cell right = GetCellAt (ToVector3(c.index) * cellSize + new Vector3 (cellSize, 0, 0));
					Cell below = GetCellAt (ToVector3(c.index) * cellSize - new Vector3 (0, cellSize, 0));
					Cell above = GetCellAt (ToVector3(c.index) * cellSize + new Vector3 (0, cellSize, 0));
					Cell behind = GetCellAt (ToVector3(c.index) * cellSize - new Vector3 (0, 0, cellSize));
					Cell forward = GetCellAt (ToVector3(c.index) * cellSize + new Vector3 (0, 0, cellSize));

					if (left != null && left.type != "solid") {
						added = cellToRow.TryGetValue(left, out leftRow);
						if (!added && left.type == "fluid") {
							leftRow = cellToRow.Count;
							cellToRow.Add (left, leftRow);
						}
						if (left.type == "air")
							numAirCells++;
						num++;
					}
					if (right != null && right.type != "solid") {
						added = cellToRow.TryGetValue(right, out rightRow);
						if (!added && right.type == "fluid") {
							rightRow = cellToRow.Count;
							cellToRow.Add (right, rightRow);
						}
						if (right.type == "air")
							numAirCells++;
						num++;
					}
					if (below != null && below.type != "solid") {
						added = cellToRow.TryGetValue(below, out belowRow);
						if (!added && below.type == "fluid") {
							belowRow = cellToRow.Count;
							cellToRow.Add (below, belowRow);
						}
						if (below.type == "air")
							numAirCells++;
						num++;
					}
					if (above != null && above.type != "solid") {
						added = cellToRow.TryGetValue(above, out aboveRow);
						if (!added && above.type == "fluid") {
							aboveRow = cellToRow.Count;
							cellToRow.Add (above, aboveRow);
						}
						if (above.type == "air")
							numAirCells++;
						num++;
					}
					if (behind != null && behind.type != "solid") {
						added = cellToRow.TryGetValue(behind, out behindRow);
						if (!added && behind.type == "fluid") {
							behindRow = cellToRow.Count;
							cellToRow.Add (behind, behindRow);
						}
						if (behind.type == "air")
							numAirCells++;
						num++;
					}
					if (forward != null && forward.type != "solid") {
						added = cellToRow.TryGetValue(forward, out forwardRow);
						if (!added && forward.type == "fluid") {
							forwardRow = cellToRow.Count;
							cellToRow.Add (forward, forwardRow);
						}
						if (forward.type == "air")
							numAirCells++;
						num++;
					}


					added = cellToRow.TryGetValue(c, out row);
					if (!added) {
						row = cellToRow.Count;
						cellToRow.Add (c, row);
					}
					alglib.sparseset(a, row, row, -num);
					if (left != null && left.type == "fluid") {
						alglib.sparseset(a, row, leftRow, 1);
					}
					if (right != null && right.type == "fluid") {
						alglib.sparseset(a, row, rightRow, 1);
					}
					if (below != null && below.type == "fluid") {
						alglib.sparseset(a, row, belowRow, 1);
					}
					if (above != null && above.type == "fluid") {
						alglib.sparseset(a, row, aboveRow, 1);
					}
					if (behind != null && behind.type == "fluid") {
						alglib.sparseset(a, row, behindRow, 1);
					}
					if (forward != null && forward.type == "fluid") {
						alglib.sparseset(a, row, forwardRow, 1);
					}

					b [row] = ((density * cellSize) / timestep) *
					    DivergenceAt (ToVector3 (c.index) * cellSize) -
					    numAirCells * atmPressure;
                    //Debug.Log(b[row]);
				}
			}
			//Debug.Log (cellToRow.Count + " " + numFluidCells);
            alglib.sparseconverttocrs(a);
            alglib.linlsqrstate s;
            alglib.linlsqrreport rep;
            double[] x;
            alglib.linlsqrcreate(numFluidCells, numFluidCells, out s);
            alglib.linlsqrsolvesparse(s, a, b);
            alglib.linlsqrresults(s, out x, out rep);

			foreach(KeyValuePair<Cell, int> e in cellToRow){
				e.Key.pressure = (float)x[e.Value];
                //Debug.Log((float)x[e.Value]);
			}
			foreach (DictionaryEntry e in cells) {
				Cell c = (Cell)e.Value;
				if (c.type == "air")
					c.pressure = atmPressure;
			}

            // Apply pressure
            //Debug.Log("Applying pressure");
			foreach (DictionaryEntry e in cells) {
				Cell c = (Cell)e.Value;
				if (c.type == "fluid")
					c.velocity -= timestep / (density * cellSize) *
						PressureGradientAt (ToVector3 (c.index) * cellSize);
                else{
                    Cell above = GetCellAt(ToVector3(c.index) * cellSize +
                                           new Vector3(0, cellSize, 0));
                    Cell right = GetCellAt(ToVector3(c.index) * cellSize +
                                           new Vector3(cellSize, 0, 0));
                    Cell forward = GetCellAt(ToVector3(c.index) * cellSize +
                                           new Vector3(0, 0, cellSize));
                    if (above != null && above.type == "fluid")
                    {
                        c.velocity.y -= timestep / (density * cellSize) *
                        PressureGradientAt(ToVector3(c.index) * cellSize).y;
                    }
                    if (right != null && right.type == "fluid")
                    {
                        c.velocity.x -= timestep / (density * cellSize) *
                        PressureGradientAt(ToVector3(c.index) * cellSize).x;
                    }
                    if (forward != null && forward.type == "fluid")
                    {
                        c.velocity.z -= timestep / (density * cellSize) *
                        PressureGradientAt(ToVector3(c.index) * cellSize).z;
                    }

                }
			}

            // Extrapolate velocities into buffer
            //Debug.Log("Extrapolating velocities into buffer");
			foreach (DictionaryEntry e in cells) {
				Cell c = (Cell)e.Value;
				if (c.type == "fluid")
					c.layer = 0;
				else
					c.layer = -1;
			}
			for (int i = 1; i <= Mathf.Max (2, CFL_CONSTANT); i++) {
				foreach (DictionaryEntry e in cells) {
					Cell c = (Cell)e.Value;
					if (c.layer == -1) {
                        List<Cell> neighbors = new List<Cell>();
                        Cell left = GetCellAt(ToVector3(c.index) * cellSize - new Vector3(cellSize, 0, 0));
                        Cell right = GetCellAt(ToVector3(c.index) * cellSize + new Vector3(cellSize, 0, 0));
                        Cell below = GetCellAt(ToVector3(c.index) * cellSize - new Vector3(0, cellSize, 0));
                        Cell above = GetCellAt(ToVector3(c.index) * cellSize + new Vector3(0, cellSize, 0));
                        Cell behind = GetCellAt(ToVector3(c.index) * cellSize - new Vector3(0, 0, cellSize));
                        Cell forward = GetCellAt(ToVector3(c.index) * cellSize + new Vector3(0, 0, cellSize));
                        if (left != null && left.layer == i-1){
                            neighbors.Add(left);
                        }
                        if(right != null && right.layer == i-1){
                            neighbors.Add(right);
                        }
                        if(below != null && below.layer == i-1){
                            neighbors.Add(below);
                        }
                        if(above != null && above.layer == i-1){
                            neighbors.Add(below);
                        }
                        if(behind != null && behind.layer == i-1){
                            neighbors.Add(behind);
                        }
                        if (forward != null && forward.layer == i-1){
                            neighbors.Add(forward);
                        }
                        if (neighbors.Count > 0)
                        {
                            Vector3 newVelocity = Vector3.zero;
                            Vector3 numToDivide = Vector3.zero;
                            foreach (Cell n in neighbors)
                            {
                                if (below == null || below.type != "fluid")
                                {
                                    newVelocity.y += n.velocity.y;
                                    numToDivide.y++;
                                }
                                if (left == null || left.type != "fluid")
                                {
                                    newVelocity.x += n.velocity.x;
                                    numToDivide.x++;
                                }
                                if (behind == null || behind.type != "fluid")
                                {
                                    newVelocity.z += n.velocity.z;
                                    numToDivide.z++;
                                }
                            }
                            c.velocity = new Vector3(newVelocity.x / numToDivide.x,
                                                     newVelocity.y / numToDivide.y,
                                                     newVelocity.z / numToDivide.z);
                            c.layer = i;
                        }
					}
				}
			}

            // Set solid cell velocities
            //Debug.Log("Setting solid cell velocities");
			foreach (DictionaryEntry entry in cells) {
				Cell c = (Cell)entry.Value;
				if (c.type == "solid") {
					Cell left = GetCellAt (ToVector3(c.index) * cellSize - new Vector3 (cellSize, 0, 0));
					Cell right = GetCellAt (ToVector3(c.index) * cellSize+ new Vector3 (cellSize, 0, 0));
					Cell below = GetCellAt (ToVector3(c.index) * cellSize- new Vector3 (0, cellSize, 0));
					Cell above = GetCellAt (ToVector3(c.index) * cellSize+ new Vector3 (0, cellSize, 0));
					Cell behind = GetCellAt (ToVector3(c.index) * cellSize- new Vector3 (0, 0, cellSize));
					Cell forward = GetCellAt (ToVector3(c.index) * cellSize+ new Vector3 (0, 0, cellSize));


					if (left != null
					   && (left.type == "air" || left.type == "fluid")
					   && left.velocity.x > 0) {
						left.velocity.x = 0;
					}
					if (right != null
						&& (right.type == "air" || right.type == "fluid")
						&& right.velocity.x < 0) {
						right.velocity.x = 0;
					}
					if (below != null
						&& (below.type == "air" || below.type == "fluid")
						&& below.velocity.y > 0) {
						below.velocity.y = 0;
					}
					if (above != null
						&& (above.type == "air" || above.type == "fluid")
						&& above.velocity.y < 0) {
						above.velocity.y = 0;
					}
					if (behind != null
						&& (behind.type == "air" || behind.type == "fluid")
						&& behind.velocity.z > 0) {
						behind.velocity.z = 0;
					}
					if (forward != null
						&& (forward.type == "air" || forward.type == "fluid")
						&& forward.velocity.z < 0) {
						forward.velocity.z = 0;
					}
				}
			}


		}

		public void MoveParticles(HashSet<GameObject> particles, float timestep){
			foreach (GameObject particle in particles) {
				Vector3 v = GetVelocity (particle.transform.position);
				if (float.IsNaN (v.x) || float.IsNaN (v.y) || float.IsNaN (v.z)) {
					//Debug.Log ("Not good velocity");
				} else {
					//Debug.Log ("Moving at speed " + v);
					particle.transform.position += v * timestep;
				}
			}
		}
		public void SourceSpawn(Vector3 pos, Vector3 velocity){
			Cell c = GetCellAt (pos);
			if (c != null) {
				c.velocity = velocity;
			} else {
				c = new Cell (GetIndexFor (pos));
				c.type = "fluid";
			}

		}
		public void DrawGizmos() {
			foreach (DictionaryEntry entry in cells) {
				Cell c = (Cell)entry.Value;

				if (c.type == "air") {
					Gizmos.color = Color.white;
					Gizmos.DrawWireCube (ToVector3 (c.index) * cellSize, 
						new Vector3 (cellSize, cellSize, cellSize));
				} else if (c.type == "fluid") {
					Gizmos.color = Color.blue;
					Gizmos.DrawCube(ToVector3 (c.index) * cellSize, 
						new Vector3 (cellSize, cellSize, cellSize));
				} else if (c.type == "solid") {
					Gizmos.color = Color.clear;
					Gizmos.DrawWireCube(ToVector3 (c.index) * cellSize, 
						new Vector3 (cellSize, cellSize, cellSize));
				}

				Gizmos.color = Color.Lerp (Color.green, Color.red, c.velocity.magnitude / GetMaxVelocity());
				Gizmos.DrawRay (ToVector3 (c.index) * cellSize, c.velocity.normalized * cellSize / 2.0f);

			}
		}

	}



	public float cellSize;
	public Vector3 volume;
	public Vector3 center;
	public int numParticles;

	public Vector3 sourcePoint;
	public Vector3 sourceVelocity;

	Grid g;
	HashSet<GameObject> particles = new HashSet<GameObject>();

	void Start () {
		g = new Grid (cellSize, volume, center);
	}

    void FixedUpdate(){

        if (Input.GetKey(KeyCode.Space)) { 
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.localScale = Vector3.one * cellSize / 2.0f;
            p.transform.position = sourcePoint;
            particles.Add(p);
            g.SourceSpawn(sourcePoint, sourceVelocity);
        }
        FluidDynamicsAlgorithm();

	}
	void FluidDynamicsAlgorithm(){
		//while(true){
			float startTime = Time.realtimeSinceStartup;
			float timestep = g.GetTimeStep ();
			//timestep = Time.fixedDeltaTime;
			g.UpdateGrid (particles);
			g.AdvanceVelocityField (timestep, -2f, 0.01f, 1.0f, 1.0f);
			g.MoveParticles (particles, timestep);
			//Debug.Log ("Step took " + (Time.realtimeSinceStartup - startTime));
		//	yield return new WaitForFixedUpdate();
		//}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube (center, volume);
		if(g != null)
			g.DrawGizmos ();
	}
}
