using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Vertex{
	public string name;
	public int xPos, yPos;
	public Vertex parent;

	public Vertex(){
		xPos = 0; 
		yPos = 0;
		name = "v00";
		parent = null;
	}

	public Vertex(int x, int y){
		xPos = x;
		yPos = y;
		name = "v" + x + "" + y;
		parent = null;
	}
	public void setParent(Vertex a){

		parent = a;
	}
}

public class Edge{
	public Vertex v1, v2;
	public int weight;

	public Edge(Vertex a, Vertex b){
		v1 = a; 
		v2 = b;
		weight = 0;
	}
	public Edge(Vertex a, Vertex b, int w){
		v1 = a; 
		v2 = b;
		weight = w;
	}
	public void setWeight(int w){
		weight = w;
	}
	public bool contains(Vertex a){
		return v1.Equals (a) || v2.Equals (a);
	}
	public bool equals(Edge e1, Edge e2){
		return (e1.v1 == e2.v1 && e1.v1 == e2.v2)
		|| (e1.v1 == e2.v2 && e1.v2 == e2.v1);
	}
}

public class Grid {
	

	private int width, height;
	public Vertex[,] vertices;
	public HashSet<Vertex> vertexSet;
	public HashSet<Edge> edges;

	public Grid(){
		width = 2;
		height = 2;
		edges = new HashSet<Edge> ();
		vertexSet = new HashSet<Vertex> ();
		vertices = new Vertex[width, height];
	}

	public Grid(int w, int h){
		width = w;
		height = h;
		edges = new HashSet<Edge> ();
		vertexSet = new HashSet<Vertex> ();
		vertices = new Vertex[width, height];
	}
	public void instantiateVertices(){
		vertices = new Vertex[width, height];
		vertexSet = new HashSet<Vertex> ();
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				Vertex v = new Vertex (i, j);
				vertices [i, j] = v;
				vertexSet.Add (v);
			}
		}
	}
	public void randomizeEdgeWeights(){
		edges = new HashSet<Edge> ();
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				if (i - 1 >= 0) {
					Edge e = new Edge (vertices [i, j], vertices [i - 1, j], (int)(Random.value * 10));
					edges.Add (e);
				}
				if (j - 1 >= 0) {
					Edge e = new Edge (vertices [i, j], vertices [i, j - 1], (int)(Random.value * 10));
					edges.Add (e);
				}
			}
		}
	}
	public void setWidth(int w){
		width = w;
	}
	public void setHeight(int h){
		height = h;
	}

	public void addEdge(Vertex a, Vertex b){
		edges.Add (new Edge (a, b));
	}
	public void addEdge(Edge e){
		edges.Add (e);
	}
	public void addVertex(Vertex a){
		vertexSet.Add (a);
	}
	public void removeVertex(int i, int j){
		vertexSet.Remove (vertices [i, j]);
	}
	public void removeVertex(Vertex a){
		vertexSet.Remove (a);
	}
	public Grid Prims(Vertex start){
		Grid U = new Grid (width, height);
	
		foreach (Vertex v in vertexSet) {
			if(!start.Equals(v)) U.addVertex (v);
		}
		HashSet<Vertex> difference = new HashSet<Vertex> ();
		foreach (Vertex v in vertexSet) {
			difference.Add (v);
		}
		difference.ExceptWith(U.vertexSet);
		List<Edge> connectingEdges = new List<Edge> ();
		//check for edge from G.V - U to U
		foreach (Edge e in this.edges) {
			foreach (Vertex a in difference) {
				foreach (Vertex b in U.vertexSet) {
					if (e.contains (a) && e.contains (b)) {
						connectingEdges.Add (e);
					}
				}
			}
		}
		while (U.vertexSet.Count > 0 && connectingEdges.Count > 0) {
			//Find the minimum weight edge
			int minWeight = connectingEdges [0].weight;
			int minWeightIndex = 0;
			for (int i = 1; i < connectingEdges.Count; i++) {
				if (connectingEdges [i].weight < minWeight) {
					minWeight = connectingEdges [i].weight;
					minWeightIndex = i;
				}
			}
			//set the parent and remove it from U
			if (U.vertexSet.Contains (connectingEdges [minWeightIndex].v1)) {
				connectingEdges [minWeightIndex].v1.setParent (connectingEdges [minWeightIndex].v2);
				U.removeVertex (connectingEdges [minWeightIndex].v1);
			} else {
				connectingEdges [minWeightIndex].v2.setParent (connectingEdges [minWeightIndex].v1);
				U.removeVertex (connectingEdges [minWeightIndex].v2);
			}
			U.addEdge(connectingEdges[minWeightIndex]);

			//update the difference again
			foreach (Vertex v in vertexSet) {
				difference.Add (v);
			}
			difference.ExceptWith(U.vertexSet);

			//check for edge from G.V - U to U again because of updated U
			connectingEdges.Clear();
			foreach (Edge e in this.edges) {
				foreach (Vertex a in difference) {
					foreach (Vertex b in U.vertexSet) {
						if (e.contains (a) && e.contains (b)) {
							connectingEdges.Add (e);
						}
					}
				}
			}
		}
		return U;
	}
}
