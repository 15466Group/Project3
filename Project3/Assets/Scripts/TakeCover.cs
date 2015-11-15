using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TakeCover {

	//if the sniper position is not known, then find the closest obstacle and crouch against it because the sniper can be anywhere.
	//if the sniper position is known, then 

	private int numRows;
	private int numCols;
	private float nodeSize;
	private float hiddenSpaceCost;
	private Node[,] nodes;
	private float[,] spaceCosts;

	public TakeCover(float nS, float hSPC, Node[,] n, float[,] sC){
		nodes = n;
		spaceCosts = sC;
		nodeSize = nS;
		hiddenSpaceCost = hSPC;
		numRows = nodes.GetLength (0);
		numCols = nodes.GetLength (1);
	}

	public Vector3 coverPoint(bool sniperPosKnown, Vector3 playerPos){
		if (sniperPosKnown)
			return findClosestNode(playerPos);
		else
			return findClosestBuildingPos(playerPos);
	}

	//searches through his grid and finds a node that provides cover from the sniper
	private Vector3 findClosestNode(Vector3 playerPos){
		float minDist = Mathf.Infinity;
		Vector3 best = playerPos;
		for (int i = 0; i < numRows; i++) {
			for (int j = 0; j < numCols; j++) {
				Node node = nodes[i,j];
				if (node.free && spaceCosts[i,j] == hiddenSpaceCost){
					float distance = Vector3.Distance(playerPos, node.loc);
//					if (distance <= nodeSize){
////						Debug.Log ("found one");
//						return node.loc;
//					} 
					if (distance < minDist){
						minDist = distance;
						best = node.loc;
					}
				}
			}
		}
//		Debug.Log ("finally found one");
		return best;
	}

	private Vector3 findClosestBuildingPos(Vector3 playerPos){
		return Vector3.zero;
	}

}
