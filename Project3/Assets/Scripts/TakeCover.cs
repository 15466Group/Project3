using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TakeCover : NPCBehaviour {

	//if the sniper position is not known, then find the closest obstacle and crouch against it because the sniper can be anywhere.
	//if the sniper position is known, then 

	public bool sniperPosKnown { get; set; }
	public float inCoverTime { get; set; }
	private Node[,] nodes;
	private float[,] spaceCosts;

	public float dist { get; set; }
	private float hiddenSpaceCost;
	private bool foundCover;

	// Use this for initialization
	public override void Starta () {
		base.Starta ();
		sniperPosKnown = false;
		inCoverTime = 0.0f;
		isWanderer = false;
		isReachingGoal = true;
		nodes = null;
		spaceCosts = null;;
		foundCover = false;
		speedMax = 20.0f;
		hiddenSpaceCost = Mathf.Infinity;
	}
	
	// Update is called once per frame
	public override void Updatea () {
		if (!foundCover) {
			if (sniperPosKnown && nodes != null && spaceCosts != null) {
				target = findClosestNode ();
			} else {
				target = findClosestBuildingPos ();
			}
		}
		Debug.DrawRay (transform.position, target - transform.position, Color.cyan);
		if (Vector3.Distance (transform.position, target) < dist) {
			inCoverTime += Time.deltaTime;
		} else {
			base.Updatea ();
		}
	}

	//searches through his grid and finds a node that provides cover from the sniper
	private Vector3 findClosestNode(){
		int numRows = nodes.GetLength (0);
		int numCols = nodes.GetLength (1);
		float minDist = Mathf.Infinity;
		Vector3 best = transform.position;
		for (int i = 0; i < numRows; i++) {
			for (int j = 0; j < numCols; j++) {
				Node node = nodes[i,j];
				if (node.free && spaceCosts[i,j] == hiddenSpaceCost){
					float distance = Vector3.Distance(transform.position, node.loc);
					if (distance < 2 * dist){
						Debug.Log ("found one");
						foundCover = true;
						return node.loc;
					} 
					if (distance < minDist){
						minDist = distance;
						best = node.loc;
					}
				}
			}
		}
		Debug.Log ("finally found one");
		foundCover = true;
		return best;
	}

	private Vector3 findClosestBuildingPos(){
		return Vector3.zero;
	}

	public void setGridAndSniperPos(Node[,] grid, bool sPK, float hSPC, float[,] sPC) {
		sniperPosKnown = sPK;
		nodes = grid;
		hiddenSpaceCost = hSPC;
		spaceCosts = sPC;
	}

}
