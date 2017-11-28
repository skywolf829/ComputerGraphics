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

        float cellSize;
        Grid g;
        public Cell(Grid g, float size, Vector3Int index){
            this.g = g;
			pressure = 0;
            cellSize = size;
			velocity = Vector3.zero;
			this.index = index;
		}
        public bool hasFluidNeighbor(){
            foreach (Cell c in neighbors())
            {
                if (c.type == "fluid")
                    return true;
            }
            return false;
        }
        public List<Cell> neighbors(){
            List<Cell> n = new List<Cell>();
            if(above() != null){
                n.Add(above());
            }
            if(below() != null){
                n.Add(below());
            }
            if (left() != null){
                n.Add(left());
            }
            if(right() != null){
                n.Add(right());
            }
            if(behind() != null){
                n.Add(behind());
            }
            if (forward() != null){
                n.Add(forward());
            }
            return n;
        }
        public Cell above(){
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(0, cellSize, 0));
        }
        public Cell below()
        {
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(0, -cellSize, 0));
        }
        public Cell left()
        {
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(-cellSize, 0, 0));
        }
        public Cell right()
        {
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(cellSize, 0, 0));
        }
        public Cell forward()
        {
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(0, 0, cellSize));
        }
        public Cell behind()
        {
            Vector3 pos = new Vector3(index.x, index.y, index.z);
            return g.GetCellAt(pos * cellSize + new Vector3(0, 0, -cellSize));
        }

	}

	class Grid{
        
		const float CFL_CONSTANT = 2;
		const float MIN_TIMESTEP = 1 / 1000f;
        const float MAX_TIMESTEP = 1 / 60f;

		private float cellSize;
		private Vector3 volume;
		private Vector3 center;

		Hashtable cells = new Hashtable();
        List<Vector3[]> sourceCells = new List<Vector3[]>();

		public Grid(float cellsize, Vector3 v, Vector3 origin){
			cellSize = cellsize;
			volume = v;
			center = origin;
		}
	
		Vector3 PressureGradientAt(Vector3 pos){
			Vector3 result = Vector3.zero;
            Vector3 num = Vector3.zero;
			Cell c = GetCellAt (pos);
			if (c != null) {
                if (c.left() != null)
                {
                    result.x += c.pressure - c.left().pressure;
                    num.x += 1;
                }
                if (c.below() != null)
                {
                    result.y += c.pressure - c.below().pressure;
                    num.y += 1;
                }

                if (c.behind() != null)
                {
                    result.z += c.pressure - c.behind().pressure;
                    num.z += 1;
                }
                if (c.right() != null)
                {
                    result.x += -c.pressure + c.right().pressure;
                    num.x += 1;
                }
                if (c.above() != null)
                {
                    result.y += -c.pressure + c.above().pressure;
                    num.y += 1;
                }

                if (c.forward() != null)
                {
                    result.z += -c.pressure + c.forward().pressure;
                    num.z += 1;
                }
			}
            if (num.x > 0.01f) result.x /= num.x;
            if (num.y > 0.01f) result.y /= num.y;
            if (num.z > 0.01f) result.z /= num.z;
			return result;
		}
        Vector3 VelocityGradientAt(Vector3 pos)
        {
            Vector3 result = Vector3.zero;
            Vector3 num = Vector3.zero;
            Cell c = GetCellAt(pos);
            if (c != null)
            {
                result = c.velocity;
                //Debug.Log("Cell: " + c.index + " velocity is " + result);
                if (c.left() != null)
                {
                    result.x += c.velocity.x - c.left().velocity.x;
                    num.x += 1;
                }
                if(c.right() != null){
                    result.x += -c.velocity.x + c.right().velocity.x;
                    num.x += 1;
                }

                if (c.below() != null)
                {
                    result.y += c.velocity.y - c.below().velocity.y;
                    num.y += 1;
                }
                if (c.above() != null)
                {
                    result.y += -c.velocity.y + c.above().velocity.y;
                    num.y += 1;
                }
                if (c.behind() != null)
                {
                    result.z += c.velocity.z - c.behind().velocity.z;
                    num.z += 1;
                }
                if (c.forward() != null)
                {
                    result.z += -c.velocity.z + c.forward().velocity.z;
                    num.z += 1;
                }
            }
            if (num.x > 0.01f) result.x /= num.x;
            if (num.y > 0.01f) result.y /= num.y;
            if (num.z > 0.01f) result.z /= num.z;
            return result;
		}
        Vector3 ModifiedVelocityGadientAt(Vector3 pos){
            Vector3 result = Vector3.zero;
            Vector3 num = Vector3.zero;
            Cell c = GetCellAt(pos);
            if (c != null)
            {
                Vector3 cVel = c.velocity;

                if ((c.left() != null && c.left().type == "solid") ||
                    (c.right() != null && c.right().type == "solid")){
                    cVel.x = 0;
                }
                if ((c.below() != null && c.below().type == "solid") ||
                    (c.above() != null && c.above().type == "solid")){
                    cVel.y = 0;
                }
                if((c.behind() != null && c.behind().type == "solid") ||
                   (c.forward() != null && c.forward().type == "solid")){
                    cVel.z = 0;
                }

                if (c.right() != null && c.right().type != "solid")
                {
                    result.x += -cVel.x + c.right().velocity.x;
                    num.x += 1;
                }
                if(c.left() != null && c.left().type != "solid"){
                    result.x += cVel.x - c.left().velocity.x;
                    num.x += 1;
                }
                if (c.above() != null && c.above().type != "solid")
                {
                    result.y += -cVel.y + c.above().velocity.y;
                    num.y += 1;
                }
                if(c.below() != null && c.below().type != "solid"){
                    result.y += cVel.y + c.below().velocity.y;
                    num.y += 1;
                }

                if (c.forward() != null && c.forward().type != "solid")
                {
                    result.z += -cVel.z + c.forward().velocity.z;
                    num.z += 1;
                }
                if(c.behind() != null && c.behind().type != "solid"){
                    result.z += cVel.z - c.behind().velocity.z;
                    num.z += 1;
                }
            }
            if (num.x > 0.01f) result.x /= num.x;
            if (num.y > 0.01f) result.y /= num.y;
            if (num.z > 0.01f) result.z /= num.z;
            return result;
        }
        float ModifiedDivergenceAt(Vector3 pos){
            Vector3 result = ModifiedVelocityGadientAt(pos);
            return result.x + result.y + result.z;
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

                if (c.left() != null) {
                    result += c.left().pressure;
					num++;
				}
                if (c.right() != null) {
                    result += c.right().pressure;
					num++;
				}
                if (c.below() != null) {
                    result += c.below().pressure;
					num++;
				}
                if (c.above() != null) {
                    result += c.above().pressure;
					num++;
				}
                if (c.behind() != null) {
                    result += c.behind().pressure;
					num++;
				}
                if (c.forward() != null) {
                    result += c.forward().pressure;
					num++;
				}
				result -= num * c.pressure;
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

                if (c.left() != null) {
					result.x += left.x;
					result.y += left.y;
					result.z += left.z;
					num++;
				}
                if (c.right() != null) {
					result.x += right.x;
					result.y += right.y;
					result.z += right.z;
					num++;
				}
                if (c.below() != null) {
					result.x += below.x;
					result.y += below.y;
					result.z += below.z;
					num++;
				}
                if (c.above() != null) {
					result.x += above.x;
					result.y += above.y;
					result.z += above.z;
					num++;
				}
                if (c.behind() != null) {
					result.x += behind.x;
					result.y += behind.y;
					result.z += behind.z;
					num++;
				}
                if (c.forward() != null) {
					result.x += forward.x;
					result.y += forward.y;
					result.z += forward.z;
					num++;
				}
				result -= num * gradient;
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
            if(c != null){
                result = c.velocity;
                Vector3 cCellGlobalSpot = ToVector3(c.index) * cellSize;

                if (pos.x > cCellGlobalSpot.x && c.right() != null)
                {
                    result.x = Mathf.Lerp(c.velocity.x, c.right().velocity.x,
                                          (pos.x - cCellGlobalSpot.x) / cellSize);   
                }
                else if (pos.x < cCellGlobalSpot.x && c.left() != null){
                    result.x = Mathf.Lerp(c.velocity.x, c.left().velocity.x,
                                          (-pos.x + cCellGlobalSpot.x) / cellSize);  
                }
                if (pos.z > cCellGlobalSpot.z && c.forward() != null){
                    result.z = Mathf.Lerp(c.velocity.z, c.forward().velocity.z, 
                                          (pos.z - cCellGlobalSpot.z) / cellSize);
                }
                else if (pos.z < cCellGlobalSpot.z && c.behind() != null)
                {
                    result.z = Mathf.Lerp(c.velocity.z, c.behind().velocity.z,
                                          (-pos.z + cCellGlobalSpot.z) / cellSize);
                }
                if (pos.y > cCellGlobalSpot.y && c.above() != null)
                {
                    result.y = Mathf.Lerp(c.velocity.y, c.above().velocity.y, 
                                          (pos.y - cCellGlobalSpot.y) / cellSize);
                }
                else if (pos.y < cCellGlobalSpot.y && c.below() != null)
                {
                    result.y = Mathf.Lerp(c.velocity.y, c.below().velocity.y,
                                          (-pos.y + cCellGlobalSpot.y) / cellSize);
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
            //Debug.Log(cellSize / GetMaxVelocity());
			return Mathf.Min(
				Mathf.Max(cellSize / GetMaxVelocity (), MIN_TIMESTEP), 
				MAX_TIMESTEP
			);
		}
		public Cell GetCellAt(Vector3 pos){
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
        void CollisionCheckAndResponse(Cell c, Vector3 velocity, Vector3 normal, Vector3 pos, float timestep, 
                                       float dampener){
            float d, eBefore, eAfter, numerator, denominator;
            Vector3 suggestedVelocity;
            d = -Vector3.Dot(normal,pos);
            eBefore = Vector3.Dot(normal, ToVector3(c.index) * cellSize) + d;
            eAfter = Vector3.Dot(normal, ToVector3(c.index) * cellSize + velocity * timestep) + d;
            Debug.Log("eBefore " + eBefore + " eAfter: " + eAfter);
            if ((eBefore >= 0 && eAfter <= 0) ||
            (eAfter >= 0 && eBefore <= 0))
            {
                Debug.Log("Collision");
                numerator = Vector3.Dot(velocity, normal);
                denominator = Vector3.Dot(normal, normal);
                Vector3 u = (numerator / denominator) * normal;
                Vector3 w = velocity - u;
                suggestedVelocity = (w - u) * dampener;
                c.velocity = suggestedVelocity;
            }
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
        public void UpdateGrid(HashSet<GameObject> particles)
        {

            // Reset all cells
            foreach (DictionaryEntry entry in cells)
            {
                Cell c = (Cell)(entry.Value);
                c.layer = -1;
            }

            // Update cells with fluid
            foreach (GameObject particle in particles)
            {
                Cell c = GetCellAt(particle.transform.position);
                Vector3Int xyz = GetIndexFor(particle.transform.position);
                if (c == null)
                {
                    if (WithinBounds(xyz))
                    {
                        c = new Cell(this, cellSize, xyz);
                        c.layer = 0;
                        c.type = "fluid";
                        cells.Add(hash(xyz), c);
                    }
                }
                else
                {
                    if (c.type != "solid")
                    {
                        c.type = "fluid";
                        c.layer = 0;
                    }
                }
            }
            // Create buffer zone
            for (int i = 1; i <= Mathf.Max(2, CFL_CONSTANT); i++)
            {
                List<Cell> cellsToAdd = new List<Cell>();
                foreach (DictionaryEntry entry in cells)
                {
                    Cell c = (Cell)(entry.Value);
                    if (c.layer == i - 1)
                    {
                        List<Vector3Int> neighbors = new List<Vector3Int>();
                        neighbors.Add(c.index + new Vector3Int(0, 1, 0));
                        neighbors.Add(c.index + new Vector3Int(0, -1, 0));
                        neighbors.Add(c.index + new Vector3Int(0, 0, 1));
                        neighbors.Add(c.index + new Vector3Int(0, 0, -1));
                        neighbors.Add(c.index + new Vector3Int(1, 0, 0));
                        neighbors.Add(c.index + new Vector3Int(-1, 0, 0));
                        foreach (Vector3Int n in neighbors)
                        {
                            if (cells.ContainsKey(hash(n)))
                            {
                                Cell neighbor = (Cell)cells[hash(n)];
                                if (neighbor.layer == -1 && neighbor.type != "solid")
                                {
                                    neighbor.type = "air";
                                }
                                neighbor.layer = i;
                            }
                            else
                            {
                                Cell neighbor = new Cell(this, cellSize, n);
                                neighbor.layer = i;
                                if (WithinBounds(n))
                                {
                                    neighbor.type = "air";
                                }
                                else
                                {
                                    neighbor.type = "solid";
                                }
                                cellsToAdd.Add(neighbor);
                            }
                        }
                    }
                }
                foreach (Cell k in cellsToAdd)
                {
                    if (!cells.ContainsKey(hash(k.index)))
                    {
                        cells.Add(hash(k.index), k);
                    }
                }
            }

            // Delete old cells
            List<int> cellsToRemove = new List<int>();
            foreach (DictionaryEntry entry in cells)
            {
                Cell c = (Cell)entry.Value;
                if (c.layer == -1)
                {
                    cellsToRemove.Add((int)(entry.Key));
                }
            }
            foreach (int k in cellsToRemove)
            {
                cells.Remove(k);
            }
        }

		public void AdvanceVelocityField(float timestep, float gravity, float viscosity,
			float density, float atmPressure){
            if (cells.Count == 0) return;
			int numFluidCells = 0;

            foreach (DictionaryEntry entry in cells)
            {
                Cell c = (Cell)entry.Value;
                if (c.type == "fluid") numFluidCells++;
            }
            // Convection via backwards particle trace
            //Debug.Log("Convection");

			foreach (DictionaryEntry entry in cells) {
				Cell c = (Cell)entry.Value;		
				Cell traceCell = TraceParticle (ToVector3 (c.index) * cellSize, -timestep);
				if (traceCell != null) {
					c.tempVelocity = traceCell.velocity;
				} else {
                    c.tempVelocity = c.velocity;
				}

			}
			ApplyTempVelocities ();

            // Add new source entries
            foreach (Vector3[] entry in sourceCells)
            {
                Cell c = GetCellAt(entry[0]);
                if (c != null)
                {
                    c.type = "fluid";
                    c.velocity = entry[1];
                    c.layer = 0;
                }
                else
                {
                    Debug.Log("Making new cell for source");
                    c = new Cell(this, cellSize, GetIndexFor(entry[0]));
                    c.type = "fluid";
                    c.velocity = entry[1];
                    c.layer = 0;
                    cells.Add(hash(c.index), c);

                }
            }
            sourceCells.Clear();

            // External forces
            //Debug.Log("External Forces");
			foreach (DictionaryEntry entry in cells) {
				Cell c = ((Cell)entry.Value);
				if (c.type == "fluid" ||
                    (c.above() != null && c.above().type == "fluid")) {
                    c.velocity += new Vector3(0, timestep * gravity, 0);
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
                    if (c.left() != null && c.left().type != "solid") {
                        added = cellToRow.TryGetValue(c.left(), out leftRow);
                        if (!added && c.left().type == "fluid") {
							leftRow = cellToRow.Count;
                            cellToRow.Add (c.left(), leftRow);
						}
                        if (c.left().type == "air")
							numAirCells++;
						num++;
					}
                    if (c.right() != null && c.right().type != "solid") {
                        added = cellToRow.TryGetValue(c.right(), out rightRow);
                        if (!added && c.right().type == "fluid") {
							rightRow = cellToRow.Count;
                            cellToRow.Add (c.right(), rightRow);
						}
                        if (c.right().type == "air")
							numAirCells++;
						num++;
					}
                    if (c.below() != null && c.below().type != "solid") {
                        added = cellToRow.TryGetValue(c.below(), out belowRow);
                        if (!added && c.below().type == "fluid") {
							belowRow = cellToRow.Count;
                            cellToRow.Add (c.below(), belowRow);
						}
                        if (c.below().type == "air")
							numAirCells++;
						num++;
					}
                    if (c.above() != null && c.above().type != "solid") {
                        added = cellToRow.TryGetValue(c.above(), out aboveRow);
                        if (!added && c.above().type == "fluid") {
							aboveRow = cellToRow.Count;
                            cellToRow.Add (c.above(), aboveRow);
						}
                        if (c.above().type == "air")
							numAirCells++;
						num++;
					}
                    if (c.behind() != null && c.behind().type != "solid") {
                        added = cellToRow.TryGetValue(c.behind(), out behindRow);
                        if (!added && c.behind().type == "fluid") {
							behindRow = cellToRow.Count;
                            cellToRow.Add (c.behind(), behindRow);
						}
                        if (c.behind().type == "air")
							numAirCells++;
						num++;
					}
                    if (c.forward() != null && c.forward().type != "solid") {
                        added = cellToRow.TryGetValue(c.forward(), out forwardRow);
                        if (!added && c.forward().type == "fluid") {
							forwardRow = cellToRow.Count;
                            cellToRow.Add (c.forward(), forwardRow);
						}
                        if (c.forward().type == "air")
							numAirCells++;
						num++;
					}


					added = cellToRow.TryGetValue(c, out row);
					if (!added) {
						row = cellToRow.Count;
						cellToRow.Add (c, row);
					}
					alglib.sparseset(a, row, row, -num);
                    if (c.left() != null && c.left().type == "fluid") {
						alglib.sparseset(a, row, leftRow, 1);
					}
                    if (c.right() != null && c.right().type == "fluid") {
						alglib.sparseset(a, row, rightRow, 1);
					}
                    if (c.below() != null && c.below().type == "fluid") {
						alglib.sparseset(a, row, belowRow, 1);
					}
                    if (c.above() != null && c.above().type == "fluid") {
						alglib.sparseset(a, row, aboveRow, 1);
					}
                    if (c.behind() != null && c.behind().type == "fluid") {
						alglib.sparseset(a, row, behindRow, 1);
					}
                    if (c.forward() != null && c.forward().type == "fluid") {
						alglib.sparseset(a, row, forwardRow, 1);
					}

					b [row] = ((density * cellSize) / timestep) *
                        ModifiedDivergenceAt (ToVector3 (c.index) * cellSize) -
					    numAirCells * atmPressure;
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
                if (c.type == "fluid"
                   || (c.type == "air" && c.hasFluidNeighbor())){
                    c.velocity -= (timestep / (density * cellSize)) *
                        PressureGradientAt(ToVector3(c.index) * cellSize);
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
            for (int i = 1; i <= Mathf.Max(2, CFL_CONSTANT); i++)
            {
                foreach (DictionaryEntry e in cells)
                {
                    Cell c = (Cell)e.Value;
                    List<Cell> neighbors = new List<Cell>();
					if (c.layer == -1) {
                        foreach (Cell n in c.neighbors())
                        {
                            if (n.layer == i - 1)
                                neighbors.Add(n);
                        }
                        if (neighbors.Count > 0)
                        {
                            Vector3 newVelocity = Vector3.zero;
                            Vector3 numToDivide = Vector3.zero;
                            foreach (Cell n in neighbors)
                            {
                                if (c.below() == null || c.below().type != "fluid")
                                {
                                    newVelocity.y += n.velocity.y;
                                    numToDivide.y += 1;
                                }
                                if (c.left() == null || c.left().type != "fluid")
                                {
                                    newVelocity.x += n.velocity.x;
                                    numToDivide.x += 1;
                                }
                                if (c.behind() == null || c.behind().type != "fluid")
                                {
                                    newVelocity.z += n.velocity.z;
                                    numToDivide.z += 1;
                                }
                                if(c.above() == null || c.above().type != "fluid")
                                {
                                    newVelocity.y += n.velocity.y;
                                    numToDivide.y += 1;
                                }
                                if (c.right() == null || c.right().type != "fluid")
                                {
                                    newVelocity.x += n.velocity.x;
                                    numToDivide.x += 1;
                                }
                                if (c.forward() == null || c.forward().type != "fluid")
                                {
                                    newVelocity.z += n.velocity.z;
                                    numToDivide.z += 1;
                                }
                            }
                            //Debug.Log("Before: " + c.velocity);
                            if (numToDivide.x > 0.01f){
                                c.velocity.x = newVelocity.x / numToDivide.x;
                            }
                            if (numToDivide.y > 0.01f)
                            {
                                c.velocity.y = newVelocity.y / numToDivide.y;
                            }
                            if (numToDivide.z > 0.01f)
                            {
                                c.velocity.z = newVelocity.z / numToDivide.z;
                            }
                            //Debug.Log("After: " + c.velocity);
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
                    c.velocity = Vector3.zero;


                    if (c.left() != null
                        && (c.left().type == "air" || c.left().type == "fluid")
                        && c.left().velocity.x > 0) {
                        c.left().velocity.x = -c.left().velocity.x * 0.5f;
					}
                    if (c.below() != null
                        && (c.below().type == "air" || c.below().type == "fluid")
                        && c.below().velocity.y > 0) {
                        c.below().velocity.y = -c.below().velocity.y * 0.5f;
                    }
                    if (c.behind() != null
                        && (c.behind().type == "air" || c.behind().type == "fluid")
                        && c.behind().velocity.z > 0) {
                        c.behind().velocity.z = -c.behind().velocity.z * 0.5f;
                    }

                    if (c.right() != null
                        && (c.right().type == "air" || c.right().type == "fluid")
                        && c.right().velocity.x < 0) {
                        c.right().velocity.x = -c.right().velocity.x * 0.5f;
					}
                    if (c.above() != null
                        && (c.above().type == "air" || c.above().type == "fluid")
                        && c.above().velocity.y < 0) {
                        c.above().velocity.y = -c.above().velocity.y * 0.5f;
					}
                    if (c.forward() != null
                        && (c.forward().type == "air" || c.forward().type == "fluid")
                        && c.forward().velocity.z < 0) {
                        c.forward().velocity.z = -c.forward().velocity.z * 0.5f;
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
            sourceCells.Add(new Vector3[] { pos, velocity });
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
					Gizmos.DrawWireCube(ToVector3 (c.index) * cellSize, 
						new Vector3 (cellSize, cellSize, cellSize));
				} else if (c.type == "solid") {
					Gizmos.color = Color.green;
					Gizmos.DrawWireCube(ToVector3 (c.index) * cellSize, 
						new Vector3 (cellSize, cellSize, cellSize));
				}

				Gizmos.color = Color.Lerp (Color.green, Color.red, c.velocity.magnitude / GetMaxVelocity());
             
                Gizmos.DrawRay(ToVector3(c.index) * cellSize, c.velocity / GetMaxVelocity() * cellSize / 2.0f);

			}
		}

	}



	public float cellSize;
	public Vector3 volume;
	public Vector3 center;
    //public int numParticles;
    public Material blue;

	public Vector3 sourcePoint;
	public Vector3 sourceVelocity;

    bool automatic = false;

	Grid g;
	HashSet<GameObject> particles = new HashSet<GameObject>();

	void Start () {
		g = new Grid (cellSize, volume, center);
	}

    void FixedUpdate(){

        if (Input.GetKeyDown(KeyCode.Space)) { 
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.transform.localScale = Vector3.one * cellSize / 2.0f;
            p.transform.position = sourcePoint;
            p.GetComponent<MeshRenderer>().material = blue;
            particles.Add(p);
            g.SourceSpawn(sourcePoint, sourceVelocity);
        }
        if(Input.GetKeyDown(KeyCode.C)){
            automatic = !automatic;
        }
        if(Input.GetKeyDown(KeyCode.Return) || automatic)
            FluidDynamicsAlgorithm();

	}

	void FluidDynamicsAlgorithm(){
		//while(true){
			float startTime = Time.realtimeSinceStartup;
			float timestep = g.GetTimeStep ();
			//timestep = Time.fixedDeltaTime;
			g.UpdateGrid (particles);
			g.AdvanceVelocityField (timestep, -5f, 0.01f, 1.0f, 1.0f);
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
