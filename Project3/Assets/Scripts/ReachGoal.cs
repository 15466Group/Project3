using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReachGoal: NPCBehaviour {

	public Vector3 goalPos { get; set; } //the point of interest, poi, where they are trying to get to
	public Vector3 sniperPos { get; set; }
	private bool hitNextNode;

	public List<Node> path { get; set; }
	public Vector3 next { get; set; }
	public Vector3 nextCoords { get; set; }
	public Vector3 transCoords { get; set; }
	public Vector3 endCoords { get; set; }
//	public List<Vector3> deadPeopleSeen { get; set; }

	public GameObject plane { get; set; }
	public float nodeSize { get; set; }
	public State state { get; set; }
	private Grid G;


	private LayerMask dynamicLayer;

	private float arrivalRadius;
	private Node n;

	// Use this for initialization
	public override void Starta () {
		base.Starta ();
		dynamicLayer = 1 << LayerMask.NameToLayer ("Dynamic");
		acceleration = base.calculateAcceleration (target);
		isWanderer = false;
		isReachingGoal = true;
		rayDistDefault = 10.0f;
		rayDist = rayDistDefault;
		closeRayDistDefault = 3.5f;
		closeRayDist = closeRayDistDefault;
		speedMaxDefault = 20.0f;
		speedMax = speedMaxDefault;
		hitNextNode = true;
		next = transform.position;
		nextCoords = next;
		transCoords = next;
		endCoords = new Vector3 (next.x + 10.0f, 0.0f, next.z + 10.0f);
		path = new List<Node> ();
		inArrivalRadius = false;
		arrivalRadius = nodeSize*2;
		G = new Grid(plane, goalPos, nodeSize, sniperPos);
		G.initStart ();
//		Node estimEndNode = new Node(false, Vector3.zero, 0, 0, Mathf.Infinity, 3.0f);
//		
//		List<Node> estimPath = new List<Node> ();
		state = new State (new List<Node> (), new List<Node> (), new Dictionary<Node, Node> (),
		                  null, null, G, null, false, false);
		closeRayDist = nodeSize / 1.5f;
//		deadPeopleSeen = new List<Vector3> ();
	}

	public Node nextStep () {
		for(int i = 0; i < path.Count - 1; i++) {
			Debug.DrawLine (path[i].loc, path[i+1].loc, Color.yellow);
		}
		target = nextTarget();
		checkArrival ();
		base.Updatea ();
		return n;
	}

	//next is the position of the node that the character performs reachGoal on
	Vector3 nextTarget (){
		//if in next position's cell is same cell as the current players position's cell
		if (nextCoords.x == transCoords.x && nextCoords.z == transCoords.z && 
		    (transCoords.x != endCoords.x || transCoords.z != endCoords.z)) {
			hitNextNode = true;
		}
		n = null;
		if (hitNextNode && path.Count > 0){
			n = path[0];
			next = n.loc;
			path.RemoveAt (0);
			hitNextNode = false;
		}
		Debug.DrawLine (transform.position, next, Color.red);
		return next;
	}

	//scheduler gives current player a new path, so set the next node accordingly
	public void assignedPath(List<Node> p){
		path = p;
//		hitNextNode = true;
		hitNextNode = false;
		if(path.Count > 0)
			next = path [0].loc;
	}

	public void assignGridCoords(Vector3 nxtCrds, Vector3 trnsCrds, Vector3 endCrds){
		endCoords = endCrds;
		nextCoords = nxtCrds;
		transCoords = trnsCrds;
	}

	void checkArrival(){
		Collider[] hits = Physics.OverlapSphere (transform.position, arrivalRadius, dynamicLayer);
		if (hits.Length > 0) {
			inArrivalRadius = false;
		} else {
			inArrivalRadius = Vector3.Distance (goalPos, transform.position) <= arrivalRadius;
		}
	}

	public void updateGridSniperPos(){
		state.sGrid.sniperPosKnown = true;
	}

	public Node[,] returnGrid(){
		return state.sGrid.grid;
	}
}