using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Graph : Object {

	public Node[,] nodes;
	public Grid g;
	private int numRows;
	private int numCols;
	public float weight;
	private int totalNodesToSearch;
	private int numNodesSeen;
	public State oldState;
	private Node charNode;

	public Graph(float w){
		weight = w;
	}

	public void setGrid(Grid G){
		g = G;
		nodes = G.grid;
		weight = 2.0f;
		numRows = nodes.GetLength (0);
		numCols = nodes.GetLength (1);
	}

	//want to reuse old searches if they have not seen the endNode after short circuiting the search
	public State getPath(State s) {

		//search less nodes as the dictpath gets large, initially search 100 nodes at a time
		totalNodesToSearch = 100;
//		int dictlen = s.dictPath.Count;
//		totalNodesToSearch = (int)((-1.0f / 10.0f) * (float)dictlen + 100.0f);
//		totalNodesToSearch = Mathf.Max (20, totalNodesToSearch);

		numNodesSeen = 0;

		//infinite heuristic
		Node estimEndNode = new Node(false, Vector3.zero, 0, 0, Mathf.Infinity, false);

		List<Node> failPath = new List<Node> ();
		failPath.Add (s.endNode);

		Debug.Log ("EndNode: " + s.endNode.loc);

		while (s.open.Count > 0) {
			Node current = findSmallestVal(s.open);
			numNodesSeen += 1;
			if (Vector3.Distance(s.endNode.loc, current.loc) <= 0.5f){
				s.path = makePath (s.dictPath, s.endNode);
				s.open = new List<Node> ();
				s.closed = new List<Node> ();
				s.startNode = null;
				s.endNode = null;
				s.ongoing = false;
				s.dictPath = new Dictionary<Node, Node> ();
				s.hasFullPath = true;
				return s;
			}
			if (current.h < estimEndNode.h){
				estimEndNode = current;
			}
			if (numNodesSeen >= totalNodesToSearch){
				//do not keep using this search because endNode is not free
//				if (!s.endNode.free){
				s.path = makePath (s.dictPath, estimEndNode);
				s.open = new List<Node> ();
				s.closed = new List<Node> ();
				s.startNode = null;
				s.endNode = null;
				s.ongoing = false;
				s.dictPath = new Dictionary<Node, Node> ();
//				} 
//				else {
//					s.ongoing = true;
//					//only use estimated path if the current character's old state
//					//does not already have a full path to the endNode
//					if (!s.hasFullPath)
//						s.path = makePath(s.dictPath, estimEndNode);
//				}
				return s;
			}
			s.open.Remove (current);
			s.closed.Add (current);
			foreach (Node successor in getNeighbors(current)){
				Debug.DrawLine (successor.loc, current.loc, Color.blue);
				if (s.closed.Contains (successor)){
					continue; //in the closed set
				}
				float newCost = current.g + costOfStep(current, successor, s.swampCost);
				if (!s.open.Contains(successor)){
					s.open.Add (successor);
				}
				else if (successor.g <= newCost){
					continue;
				}

				successor.g = newCost;
				successor.f = successor.g + weight * successor.h;
				if(s.dictPath.ContainsKey (successor)) {
					s.dictPath[successor] = current;
				}
				else {
					s.dictPath.Add(successor, current); //successor came from smallestVal, to reconstruct path backwards
				}
			}
		}
		//This should never happen . . . unless you use wallhax
		s.path = new List<Node>();
		s.path.Add (s.endNode);
		s.open = new List<Node> ();
		s.closed = new List<Node> ();
		s.startNode = null;
		s.endNode = null;
		s.ongoing = false;
		s.dictPath = new Dictionary<Node, Node> ();
		s.hasFullPath = false;
		return s;
	}

	List<Node> makePath(Dictionary<Node, Node> dictPath, Node endNode){
		List<Node> path = new List<Node> ();
		Node currentNode = endNode;
		path.Add (currentNode);
		while (dictPath.ContainsKey(currentNode)) {
			//dont need to keep reconstructing if the currentNode is close
			//enough to the character
			currentNode = dictPath[currentNode];
			path.Add(currentNode);
//			if (!withinDistance(currentNode, charNode)){
//				currentNode = dictPath[currentNode];
//				path.Add(currentNode);
//			}
//			else {
//				break;
//			}
		}
		path.Reverse ();
		Node n = null;
		if (path.Count > 0) {
			n = path[0];
			path.RemoveAt (0);
		} 
		return path;
	}

	Node findSmallestVal(List<Node> open){
		Node smallestVal = open[0];
		float min = smallestVal.f;
		foreach (Node n in open) {
			float potentialMin = n.f;
			if (potentialMin < min){
				min = potentialMin;
				smallestVal = n;
			}
		}
		return smallestVal;
	}

	//swamp is for preferred path, some players may prefer swamps
	float costOfStep(Node currNode, Node nextNode, float swampCost){
		float cost = Vector3.Distance (currNode.loc, nextNode.loc);
		if (currNode.isSwamp || nextNode.isSwamp){
			cost *= swampCost;
		}
		return cost;
	}

	List<Node> getNeighbors(Node n) {
		List <Node> neighbors = new List<Node> ();
		for (int newi = n.i - 1; newi <= n.i + 1; newi++) {
			for (int newj = n.j - 1; newj <= n.j + 1; newj++) {
				if(validNeighborIndexes (n.i, n.j, newi, newj)) {
					neighbors.Add(nodes[newi,newj]);
				}
			}
		}
		return neighbors;
	}


	bool validNeighborIndexes(int i, int j, int newi, int newj) {
		return (newi >= 0 && newj >= 0 && 
				newi < numRows && newj < numCols &&
				(i != newi || j != newj) &&
				nodes [newi, newj].free);
	}
	
	float estimateHeuristic (Node n, Vector3 end) {
		return Vector3.Distance (n.loc, end);
	}

	public void setCharNode(Node c){
		charNode = c;
	}

	bool withinDistance(Node n, Node m){
		float distance = g.nodeSize * 2.5f;
		return (Vector3.Distance (n.loc, m.loc) <= distance);
	}
}
