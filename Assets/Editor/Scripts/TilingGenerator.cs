using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Editor;

public class TilingGenerator : EditorWindow
{
    [MenuItem("Window/Tiling Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(TiledMazeGenerator));
    }

    [MenuItem("Terrain/Tiling Generator")]
    public static void CreateWindow()
    {
        window = EditorWindow.GetWindow(typeof(TilingGenerator));
        window.titleContent = new GUIContent("Tiling Generator");
        window.minSize = new Vector2(500f, 700f);
    }

    // Editor variables
    private static EditorWindow window;
    private Vector2 scrollPosition;

    // Variables related to the generated tiles
    private const int EMPTY = 0;
    private const int THROUGH_HORIZONTAL = 1;
    private const int THROUGH_VERTICAL = 2;
    private const int CROSS = 3;
    private const int TOP_LEFT = 4;
    private const int TOP_RIGHT = 5;
    private const int BOT_LEFT = 6;
    private const int BOT_RIGHT = 7;
    private const int LEFT_T = 8;
    private const int TOP_T = 9;
    private const int RIGHT_T = 10;
    private const int BOT_T = 11;
    private const int LEFT = 12;
    private const int RIGHT = 13;
    private const int TOP = 14;
    private const int BOT = 15;

    private int mazeWidth = 4, mazeHeight = 4;
    private float tileSize;
    private int[] VEBP = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, HEBP = new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0 };
	private bool clearOldMap = false;
    private string VEBPString = "111111111111", HEBPString = "000110000001";
    private List<GameObject> EndTiles, LTiles, TTiles, CrossTiles, ThroughTiles;
    private GameObject tempEndObj, tempLObj, tempTobj, tempCrossObj, tempThroughObj;

    void Awake()
    {
        EndTiles = new List<GameObject>();
        LTiles = new List<GameObject>();
        TTiles = new List<GameObject>();
        CrossTiles = new List<GameObject>();
        ThroughTiles = new List<GameObject>();
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        int oldWidth = mazeWidth;
        int oldHeight = mazeHeight;

        mazeWidth = EditorGUILayout.IntField("Maze width", mazeWidth);
        if (mazeWidth < 2) mazeWidth = 2;
        mazeHeight = EditorGUILayout.IntField("Maze height", mazeHeight);
        if (mazeHeight < 2) mazeHeight = 2;
        tileSize = EditorGUILayout.FloatField("Tile size (m)", tileSize);
        if(tileSize < 0)
        {
            tileSize = 0;
        }
        VEBPString = EditorGUILayout.TextField("VEBP", VEBPString);
        HEBPString = EditorGUILayout.TextField("HEBP", HEBPString);
		if(GUILayout.Button ("Randomize bit vectors")){
			string[] BPs = randomizeBitVectors (mazeWidth, mazeHeight);
			VEBPString = BPs[0];
			HEBPString = BPs[1];
		}
        EditorGUILayout.Space();

		EditorGUILayout.LabelField ("End tiles with the entrance at the bottom of the tile");
        for (int i = 0; i < EndTiles.Count; i++)
        {
            EndTiles[i] = (GameObject)EditorGUILayout.ObjectField("End tile " + i, EndTiles[i], typeof(GameObject), true);
            if(EndTiles[i] == null)
            {
                EndTiles.Remove(EndTiles[i]);
            }
        }
        tempEndObj = (GameObject)EditorGUILayout.ObjectField("Add end tile", tempEndObj, typeof(GameObject), true);
        if (tempEndObj)
        {
            EndTiles.Add(tempEndObj);
            tempEndObj = null;
        }

        EditorGUILayout.Space();

		EditorGUILayout.LabelField ("Through tiles oriented vertically");
        for (int i = 0; i < ThroughTiles.Count; i++)
        {
            ThroughTiles[i] = (GameObject)EditorGUILayout.ObjectField("Through tile " + i, ThroughTiles[i], typeof(GameObject), true);
            if (ThroughTiles[i] == null)
            {
                ThroughTiles.Remove(ThroughTiles[i]);
            }
        }
        tempThroughObj = (GameObject)EditorGUILayout.ObjectField("Add through tile", tempThroughObj, typeof(GameObject), true);
        if(tempThroughObj)
        {
            ThroughTiles.Add(tempThroughObj);
            tempThroughObj = null;            
        }

        EditorGUILayout.Space();

		EditorGUILayout.LabelField ("L tiles oriented as an L");

        for (int i = 0; i < LTiles.Count; i++)
        {
            LTiles[i] = (GameObject)EditorGUILayout.ObjectField("L tile " + i, LTiles[i], typeof(GameObject), true);
            if (LTiles[i] == null)
            {
                LTiles.Remove(LTiles[i]);
            }
        }
        tempLObj = (GameObject)EditorGUILayout.ObjectField("Add L tile", tempLObj, typeof(GameObject), true);
        if (tempLObj)
        {
            LTiles.Add(tempLObj);
            tempLObj = null;
        }

        EditorGUILayout.Space();

		EditorGUILayout.LabelField ("T tiles oriented as a T");

        for (int i = 0; i < TTiles.Count; i++)
        {
            TTiles[i] = (GameObject)EditorGUILayout.ObjectField("T tile " + i, TTiles[i], typeof(GameObject), true);
            if (TTiles[i] == null)
            {
                TTiles.Remove(TTiles[i]);
            }
        }
        tempTobj = (GameObject)EditorGUILayout.ObjectField("Add T tile", tempTobj, typeof(GameObject), true);
        if (tempTobj)
        {
            TTiles.Add(tempTobj);
            tempTobj = null;
        }

        EditorGUILayout.Space();

		EditorGUILayout.LabelField ("Cross tiles");

        for (int i = 0; i < CrossTiles.Count; i++)
        {
            CrossTiles[i] = (GameObject)EditorGUILayout.ObjectField("Cross tile " + i, CrossTiles[i], typeof(GameObject), true);
            if (CrossTiles[i] == null)
            {
                CrossTiles.Remove(CrossTiles[i]);
            }
        }
        tempCrossObj = (GameObject)EditorGUILayout.ObjectField("Add cross tile", tempCrossObj, typeof(GameObject), true);
        if (tempCrossObj)
        {
            CrossTiles.Add(tempCrossObj);
            tempCrossObj = null;
        }

        EditorGUILayout.Space();

		GUILayout.BeginHorizontal ();
		clearOldMap = EditorGUILayout.Toggle ("Clear old map on build", clearOldMap);
        if(GUILayout.Button("Build map"))
        {
			if(ValidateBitVectors ())	
            	buildMap();
        }
		GUILayout.EndHorizontal ();
        GUILayout.EndScrollView();
    }

    private bool ValidateBitVectors()
    {
        bool x = true;
        if (VEBPString.Length != mazeWidth * (mazeHeight - 1))
        {
            //Debug.Log("Bit vector doesn't match expected length based on maze width and height");
            x = false;
        }
        else if (HEBPString.Length != mazeHeight * (mazeWidth - 1))
        {
            //Debug.Log("Bit vector doesn't match expected length based on maze width and height");
            x = false;
        }
        else
        {
            VEBP = new int[mazeWidth * (mazeHeight - 1)];
            HEBP = new int[mazeHeight * (mazeWidth - 1)];
            for (int i = 0; i < VEBPString.Length; i++)
            {
                string a;
                a = VEBPString.Substring(i, 1);
                if ((a != "0" && a != "1"))
                {
                    Debug.Log("All entries in the bit vector should be 1 or 0");
                    x = false;
                }
                else
                {
                    VEBP[i] = int.Parse(a);
                }
            }
            for (int i = 0; i < HEBPString.Length; i++)
            {
                string a;
                a = HEBPString.Substring(i, 1);
                if ((a != "0" && a != "1"))
                {
                    Debug.Log("All entries in the bit vector should be 1 or 0");
                    x = false;
                }
                else
                {
                    HEBP[i] = int.Parse(a);
                }
            }
        }
        return x;
    }
    private int[,] createTilingFromEBP()
    {
        int[,] tilingMap = new int[mazeHeight, mazeWidth];
        for (int r = 0; r < mazeHeight; r++)
        {
            for (int c = 0; c < mazeWidth; c++)
            {
                int horizontalLeft = HEBP[mazeHeight * Mathf.Max(c - 1, 0) + r];
                int horizontalRight = HEBP[Mathf.Min(mazeHeight * c + r, HEBP.Length - 1)];
                int verticalTop = VEBP[mazeWidth * Mathf.Max(r - 1, 0) + c];
                int verticalBot = VEBP[Mathf.Min(mazeWidth * r + c, VEBP.Length - 1)];

                if (c == 0 && r == 0)
                {
                    horizontalLeft = 0;
                    verticalTop = 0;
                    //Debug.Log ("top left");
                }
                else if (c == mazeWidth - 1 && r == mazeHeight - 1)
                {
                    horizontalRight = 0;
                    verticalBot = 0;
                    //Debug.Log ("bot right");
                }
                else if (c == mazeWidth - 1 && r == 0)
                {
                    verticalTop = 0;
                    horizontalRight = 0;
                    //Debug.Log ("top right");
                }
                else if (r == mazeHeight - 1 && c == 0)
                {
                    verticalBot = 0;
                    horizontalLeft = 0;
                    //Debug.Log ("bot left");
                }
                else if (c == 0)
                {
                    horizontalLeft = 0;
                    //Debug.Log ("left");
                }
                else if (r == 0)
                {
                    verticalTop = 0;
                    //Debug.Log ("top");
                }
                else if (c == mazeWidth - 1)
                {
                    horizontalRight = 0;
                    //Debug.Log ("right");
                }
                else if (r == mazeHeight - 1)
                {
                    verticalBot = 0;
                    //Debug.Log ("bot");
                }
                else
                {
                    //Debug.Log ("center");
                }

                if (verticalBot == 0 && verticalTop == 0 && horizontalRight == 0 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = EMPTY;
                }
                else if (verticalBot == 0 && verticalTop == 0 && horizontalRight == 0 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = LEFT;
                }
                else if (verticalBot == 0 && verticalTop == 0 && horizontalRight == 1 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = RIGHT;
                }
                else if (verticalBot == 0 && verticalTop == 0 && horizontalRight == 1 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = THROUGH_HORIZONTAL;
                }
                else if (verticalBot == 0 && verticalTop == 1 && horizontalRight == 0 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = TOP;
                }
                else if (verticalBot == 0 && verticalTop == 1 && horizontalRight == 0 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = TOP_LEFT;
                }
                else if (verticalBot == 0 && verticalTop == 1 && horizontalRight == 1 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = TOP_RIGHT;
                }
                else if (verticalBot == 0 && verticalTop == 1 && horizontalRight == 1 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = TOP_T;
                }
                else if (verticalBot == 1 && verticalTop == 0 && horizontalRight == 0 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = BOT;
                }
                else if (verticalBot == 1 && verticalTop == 0 && horizontalRight == 0 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = BOT_LEFT;
                }
                else if (verticalBot == 1 && verticalTop == 0 && horizontalRight == 1 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = BOT_RIGHT;
                }
                else if (verticalBot == 1 && verticalTop == 0 && horizontalRight == 1 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = BOT_T;
                }
                else if (verticalBot == 1 && verticalTop == 1 && horizontalRight == 0 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = THROUGH_VERTICAL;
                }
                else if (verticalBot == 1 && verticalTop == 1 && horizontalRight == 0 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = LEFT_T;
                }
                else if (verticalBot == 1 && verticalTop == 1 && horizontalRight == 1 && horizontalLeft == 0)
                {
                    tilingMap[r, c] = RIGHT_T;
                }
                else if (verticalBot == 1 && verticalTop == 1 && horizontalRight == 1 && horizontalLeft == 1)
                {
                    tilingMap[r, c] = CROSS;
                }
            }
        }
        return tilingMap;
    }
    private void buildMap()
    {
        int[,] tilemap = createTilingFromEBP();
		if (clearOldMap) {
			GameObject oldMap = GameObject.Find ("Map");
			if(oldMap)
				GameObject.DestroyImmediate (oldMap);
		}
		GameObject parent = new GameObject ();
		parent.name = "Map";
        for(int x = 0; x < mazeWidth; x++)
        {
            for(int y = 0; y < mazeHeight; y++)
            {
				
                // L tiles
                if(tilemap[y, x] == TOP_LEFT || tilemap[y, x] == TOP_RIGHT || tilemap[y, x] == BOT_LEFT || tilemap[y, x] == BOT_RIGHT)
                {
					int rot = 0;
					int rand = (int)(Random.Range (0, LTiles.Count));
					if (tilemap [y, x] == BOT_RIGHT) {
						rot = 1;
					}
					if (tilemap [y, x] == BOT_LEFT) {
						rot = 2;
					}
					if (tilemap [y, x] == TOP_LEFT) {
						rot = 3;
					}
                    GameObject piece = GameObject.Instantiate(LTiles[rand], 
						new Vector3(-x * tileSize, 0, y * tileSize), 
						Quaternion.Euler(new Vector3(0, (LTiles[rand].transform.eulerAngles.y + 90 * rot) % 360, 0)));
					piece.transform.SetParent (parent.transform);
                }
                // T Tiles
                if(tilemap[y, x] == TOP_T || tilemap[y, x] == BOT_T || tilemap[y, x] == RIGHT_T || tilemap[y, x] == LEFT_T)
                {
					int rot = 0;
					int rand = (int)(Random.Range (0, TTiles.Count));
					if (tilemap [y, x] == LEFT_T) {
						rot = 1;
					}
					if (tilemap [y, x] == TOP_T) {
						rot = 2;
					}
					if (tilemap [y, x] == RIGHT_T) {
						rot = 3;
					}
					GameObject piece = GameObject.Instantiate(TTiles[rand],
						new Vector3(-x * tileSize, 0, y * tileSize), 
						Quaternion.Euler(new Vector3(0, (TTiles[rand].transform.eulerAngles.y + 90 * rot) % 360, 0)));
					piece.transform.SetParent (parent.transform);
                }
                // End tiles
                if (tilemap[y, x] == TOP || tilemap[y, x] == BOT || tilemap[y, x] == LEFT || tilemap[y, x] == RIGHT)
                {
					int rot = 0;
					int rand = (int)(Random.Range (0, EndTiles.Count));
					if (tilemap [y, x] == RIGHT) {
						rot = 3;
					}
					if (tilemap [y, x] == TOP) {
						rot = 2;
					}
					if (tilemap [y, x] == LEFT) {
						rot = 1;
					}
					GameObject piece = GameObject.Instantiate(EndTiles[rand],
						new Vector3(-x * tileSize, 0, y * tileSize), 
						Quaternion.Euler(new Vector3(0, (EndTiles[rand].transform.eulerAngles.y + 90 * rot) % 360, 0)));
					piece.transform.SetParent (parent.transform);
                }
                // Through tiles
                if (tilemap[y, x] == THROUGH_HORIZONTAL || tilemap[y, x] == THROUGH_VERTICAL)
                {
					int rot = 0;
					int rand = (int)(Random.Range (0, ThroughTiles.Count));
					if (tilemap [y, x] == THROUGH_HORIZONTAL) {
						rot = 1;
					}
					GameObject piece = GameObject.Instantiate(ThroughTiles[rand],
						new Vector3(-x * tileSize, 0, y * tileSize), 
						Quaternion.Euler(new Vector3(0, (ThroughTiles[rand].transform.eulerAngles.y + 90 * rot) % 360, 0)));
					piece.transform.SetParent (parent.transform);
                }
                // Cross tiles
                if (tilemap[y, x] == CROSS)
                {
					int rand = (int)(Random.Range (0, CrossTiles.Count));
					GameObject piece = GameObject.Instantiate(CrossTiles[rand], 
                        new Vector3(-x * tileSize, 0, y * tileSize), Quaternion.identity);
					piece.transform.SetParent (parent.transform);
                }
            }
        }
    }
	public static string[] randomizeBitVectors(int mazeWidth, int mazeHeight){
		Grid G = new Grid (mazeWidth, mazeHeight);
		G.instantiateVertices ();
		G.randomizeEdgeWeights ();
		Grid U = G.Prims (G.vertices[0, 0]);
		int[] VEBP = new int[mazeWidth * (mazeHeight - 1)];
		int[] HEBP = new int[mazeHeight * (mazeWidth - 1)];
		for (int i = 0; i < VEBP.Length; i++) {
			VEBP [i] = 0;
		}
		for (int i = 0; i < HEBP.Length; i++) {
			HEBP [i] = 0;
		}
		foreach (Edge e in U.edges) {
			if (e.v1.xPos == e.v2.xPos) {
				//Debug.Log ("VEBP part");
				VEBP[mazeWidth * Mathf.Min(e.v1.yPos, e.v2.yPos) + e.v1.xPos]= 1;

			} else {
				//Debug.Log ("HEBP part");
				HEBP [mazeHeight * Mathf.Min (e.v1.xPos, e.v2.xPos) + e.v1.yPos] = 1;
			}
		}
		string s = "";
		for (int i = 0; i < VEBP.Length; i++) {
			s += VEBP [i] + "";
		}
		string VEBPString = s;
		s = "";
		for (int i = 0; i < HEBP.Length; i++) {
			s += HEBP [i] + "";
		}
		string HEBPString = s;
		return new string[] { VEBPString, HEBPString };
	}
}
