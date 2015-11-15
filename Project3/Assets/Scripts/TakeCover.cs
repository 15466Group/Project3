using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TakeCover : NPCBehaviour {

	//if the sniper position is not known, then find the closest obstacle and crouch against it because the sniper can be anywhere.
	//if the sniper position is known, then 

	public bool sniperPosKnown { get; set; }
	public float inCoverTime { get; set; }
	public Node[,] nodes { get; set; }

	private float dist;
	private float hiddenSpaceCost;

	// Use this for initialization
	public override void Starta () {
		base.Starta ();
		sniperPosKnown = false;
		inCoverTime = 0.0f;
		isWanderer = false;
		isReachingGoal = true;
		nodes = null;
		dist = 10.0f;
		speedMax = 20.0f;
		hiddenSpaceCost = Mathf.Infinity;
	}
	
	// Update is called once per frame
	public override void Updatea () {
		if (Vector3.Distance (target, transform.position) > dist) {
			if (sniperPosKnown && nodes != null) {
				target = findClosestNode ();
			} else {
				target = findClosestBuildingPos ();
			}
			base.Updatea ();
		} else {
			inCoverTime += Time.deltaTime;
		}
	}

	//searches through his grid and finds a node that provides cover from the sniper
	private Vector3 findClosestNode(){
		int numRows = nodes.GetLength (0);
		int numCols = nodes.GetLength (1);
		for (int i = 0; i < numRows; i++) {
			for (int j = 0; j < numCols; j++){
				Node node = nodes[i,j];
				if (Vector3.Distance(transform.position, node.loc) < 5 * dist){
					if (node.spaceCost == hiddenSpaceCost){
						return node.loc;
					}
				}
			}
		}
		return transform.position;
	}

	private Vector3 findClosestBuildingPos(){
		return Vector3.zero;
	}

	public void setGridAndSniperPos(Node[,] grid, bool sPK, float hSPC) {
		sniperPosKnown = sPK;
		nodes = grid;
		hiddenSpaceCost = hSPC;
	}

}
