using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid {

	public float nodeSize;
	public Vector3 goalPos { get; set; }
	private GameObject plane;

	private int obstacleLayer;
	private int swampLayer;
	private int dynamicLayer;

	public Node[,] grid;
	private int gridWidth;
	private int gridHeight;
	private float worldWidth;
	private float worldHeight;
	private Vector3 worldNW; //world north west, top left corner of map/plane
	
	public GameObject swamps;

	public Grid (GameObject p, Vector3 goalp, float nS, GameObject s) {
		plane = p;
		goalPos = goalp;
		nodeSize = nS;
		swamps = s;
	}

	// Use this for initialization
	public void initStart () {
		worldWidth = plane.transform.lossyScale.x * 10.0f; //plane
		worldHeight = plane.transform.lossyScale.z * 10.0f; //plane

		gridWidth = Mathf.RoundToInt(worldWidth / nodeSize);
		gridHeight = Mathf.RoundToInt(worldHeight / nodeSize);

		grid = new Node[gridWidth, gridHeight];

		worldNW = plane.transform.position - (plane.transform.right * worldWidth / 2.0f) + (plane.transform.forward * worldHeight / 2.0f);

		obstacleLayer = 1 << LayerMask.NameToLayer ("Obstacles");
		swampLayer = 1 << LayerMask.NameToLayer ("Swamp");
		dynamicLayer = 1 << LayerMask.NameToLayer ("Dynamic");

		initializeGrid ();
		updateGrid (goalPos);
	}

	public void initializeGrid(){
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j ++) {
				float xp = i * nodeSize + (nodeSize/2.0f) + worldNW.x;
				float zp = -(j * nodeSize + (nodeSize/2.0f)) + worldNW.z;
				Vector3 nodeCenter = new Vector3(xp, 0.0f, zp);
				Collider[] hits = Physics.OverlapSphere(nodeCenter, nodeSize/2.0f, obstacleLayer | swampLayer | dynamicLayer);
				float h = Vector3.Distance(nodeCenter, goalPos);
				int len = hits.Length;
				if(len == 0) { 
					grid[i,j] = new Node(true, nodeCenter, i, j, h, false);
				}
				else {
					bool isSwamp = checkIfContainsTag(hits, "Swamp");
					bool free;
					if ((isSwamp) && len == 1){
						free = true;
					}
					else {
						free = false;
					}
					grid[i,j] = new Node(free, nodeCenter, i, j, h, isSwamp);
				}
			}
		}
	}
	
	public void updateGrid(Vector3 g){
		goalPos = g;
		for (int i = 0; i < gridWidth; i++) {
			for (int j = 0; j < gridHeight; j ++) {
				float xp = i * nodeSize + (nodeSize/2.0f) + worldNW.x;
				float zp = -(j * nodeSize + (nodeSize/2.0f)) + worldNW.z;
				Vector3 nodeCenter = new Vector3(xp, 0.0f, zp);
				Collider[] hits = Physics.OverlapSphere(nodeCenter, nodeSize/2.0f, obstacleLayer | swampLayer | dynamicLayer);
				float h = Vector3.Distance(nodeCenter, goalPos);
				int len = hits.Length;
				if(len == 0) {
					grid[i,j] = new Node(true, nodeCenter, i, j, h, grid[i,j].isSwamp);
				}
				else {
					bool isSwamp = checkIfContainsTag(hits, "Swamp");
					bool free;
					if ((isSwamp) && len == 1){
						free = true;
					}
					else {
						free = false;
					}
					grid[i,j] = new Node(free, nodeCenter, i, j, h, grid[i,j].isSwamp);
				}
			}
		}
	}

//	void OnDrawGizmos() {
//		for (int i = 0; i < gridWidth; i++) {
//			for (int j = 0; j < gridHeight; j ++) {
//				Gizmos.color = Color.red;
//				if (grid[i,j].isGoal){
//					Gizmos.color = Color.green;
//					Gizmos.DrawCube (grid [i, j].loc, new Vector3 (nodeSize, 1.0f, nodeSize));
//				}
//				if (!grid[i,j].free) {
//					Gizmos.DrawCube (grid [i, j].loc, new Vector3 (nodeSize, 1.0f, nodeSize));
//				}
//			}
//		}
//	}

	bool checkIfContainsTag(Collider[] hits, string tag){
		bool foundTag = false;
		foreach (Collider hit in hits) {
			if (hit.CompareTag(tag)){
				foundTag = true;
				break;
			}
		}
		return foundTag;
	}

	public Vector3 getGridCoords(Vector3 location) {
		float newx = location.x + worldWidth / 2.0f;
		float newz = -location.z + worldHeight / 2.0f;

		int i = (int)(newx / nodeSize);
		int j = (int)(newz / nodeSize);

		if (i < 0)
			i = 0;
		if (i > gridWidth)
			i = gridWidth - 1;
		if (j < 0)
			j = 0;
		if (j > gridHeight)
			j = gridHeight - 1;

		return new Vector3(i, 0.0f, j);
	}


}
