using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingScheduler {
	
//	public GameObject goal;
	public LinkedList<GameObject> characters { get; set; } //empty gameobject containing children of in game characters
	public LinkedListNode<GameObject> currCharNode { get; set; }
//	public GameObject plane;
//	public GameObject swamps;
//	public float nodeSize;

//	private Grid G;
	private Graph graph;
//	private ReachGoal reachGoal;

	public PathfindingScheduler(){
		graph = new Graph (2.0f);
	}

//	private State[] states;

	// Use this for initialization
//	void Starta () {
//		iChar = 0;
//		numChars = characters.transform.childCount;
//		graph = new Graph (2.0f);
//		states = new State[numChars];
//		for (int i = 0; i < numChars; i++) {
//			Transform child = characters.transform.GetChild(i);
//			reachGoal = child.GetComponent<ReachGoal> ();
//			reachGoal.Starta();
//			Grid G = new Grid(plane, goal, nodeSize, swamps);
//			G.initStart();
//			states[i] = new State(new List<Node> (), new List<Node> (), new Dictionary<Node, Node> (),
//			                      null, null, reachGoal.swampCost, G, null, false, false);
//		}
//		if (swamps != null) {
//			int swampCount = swamps.transform.childCount;
//			for (int k = 0; k < swampCount; k++) {
//				swamps.transform.GetChild (k).GetComponent<MeshCollider> ().enabled = false;
//			}
//		}
//	}

	//have to update reachGoal.G.goalPos = behaviour[i].poi
	
	// Update is called once per frame
	public void Updatea () {
		//every char is moving at each frame, but at frame i % numChars,
		//	char i is moving according to new graph and everyone else is moving according to their 'current' graphs
		if (currCharNode != null && characters != null){
			GameObject currChar = currCharNode.Value;

			ReachGoal reachGoal = currChar.GetComponent<MasterBehaviour> ().reachGoal;
			Vector3 start = currChar.transform.position;
			Vector3 end = reachGoal.goalPos;
			Debug.Log ("End: " + reachGoal.goalPos);
			Debug.Log ("Start: " + start);

			//graph sets a threshold for how many nodes to search.
			//if the threshold is hit before the goalNode is found, graphsearch returns an estimated path
			//and ongoing is set to true.
			//otherwise, ongoing is set to false, and continue searching as everything is initalized
			State s = reachGoal.state;
			graph.setGrid (reachGoal.state.sGrid);
			s.sGrid.updateGrid (reachGoal.goalPos);
			if (!s.ongoing) {
				Vector3 startCoords = s.sGrid.getGridCoords (start);
				int startI = (int)startCoords.x;
				int startJ = (int)startCoords.z;
				s.startNode = s.sGrid.grid [startI, startJ];
				s.startNode.g = 0.0f;
				s.startNode.f = s.startNode.g + graph.weight * s.startNode.h;
				s.open.Add (s.startNode);
			}
			Vector3 endCoords = s.sGrid.getGridCoords (end);
			int endI = (int)endCoords.x;
			int endJ = (int)endCoords.z;
			s.endNode = s.sGrid.grid [endI, endJ];
			Vector3 charCoords = s.sGrid.getGridCoords (start);
			int charI = (int)charCoords.x;
			int charJ = (int)charCoords.z;

			//graph needs to know the current characters position
			graph.setCharNode (s.sGrid.grid [charI, charJ]);

			reachGoal.state = graph.getPath (s);
			List<Node> path = reachGoal.state.path;
			reachGoal.assignedPath (path);

			//regardless of which character's graph search turn it is, move all of them
			int numChars = characters.Count;
			GameObject[] charArray = new GameObject[numChars];
			characters.CopyTo(charArray,0);
			for (int i = 0; i < numChars; i++) {
				reachGoal = charArray[i].GetComponent<MasterBehaviour>().reachGoal;
				State s_i = reachGoal.state;
				reachGoal.assignGridCoords (s_i.sGrid.getGridCoords (reachGoal.next), 
				                            s_i.sGrid.getGridCoords (charArray[i].transform.position),
			                            s_i.sGrid.getGridCoords (reachGoal.goalPos));

				//character has removed this node from its path so remove it from the dictionary
				Node r;
				r = reachGoal.nextStep ();
				if (r != null) {
					foreach (Node key in s_i.dictPath.Keys) {
						if (Node.Equals (s_i.dictPath [key], r)) {
							s_i.dictPath.Remove (key);
							break;
						}
					}
				}
			}
		}
	}
}
