using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterScheduler : MonoBehaviour {

	public GameObject player;
	public GameObject characters;
	public GameObject sniper;

	public GameObject plane;
	public float nodeSize;

	private int numChars;

	private MasterBehaviour[] behaviourScripts;

	private PathfindingScheduler pathfinder;
	private LinkedList<GameObject> pathFindingChars;
	private LinkedListNode<GameObject> currCharForPath;
	private List<GameObject> deadSet;
	private List<Vector3> seenDeadSet;

	private float seenTime;
	private float shootDistance;
	private float sightDist;
	
	private NaiveBayes NB;
	private int numSeesPlayer;
	private int numSearchingForPlayer;
	private float maxDist;

	// Use this for initialization
	void Start () {
		numChars = characters.transform.childCount;
		Debug.Log (numChars);
		behaviourScripts = new MasterBehaviour[numChars];
		
		for (int i = 0; i < numChars; i++) {
			MasterBehaviour mb = characters.transform.GetChild(i).GetComponent<MasterBehaviour>();
			mb.Starta(plane, nodeSize, sniper.transform.position);
			behaviourScripts[i] = mb;
		}

		pathfinder = new PathfindingScheduler ();
		pathFindingChars = new LinkedList<GameObject> ();
		deadSet = new List<GameObject>();
		seenDeadSet = new List<Vector3> ();

		shootDistance = 10f;
		sightDist = 100.0f;

		numSeesPlayer = 0;
		numSearchingForPlayer = 0;
		maxDist = 100f;
		NB = new NaiveBayes (behaviourScripts [0].reachGoal.state.sGrid.hiddenSpaceCost, maxDist);
		NB.Starta ();
	
	}
	
	// Update is called once per frame
	void Update () {
		//loop through the characters, update their status (ie senses, poi, etc)
		//if the character is not reaching a goal, then wipe it's search state clean
		//loop through chatacters again and do behavior.Updatea
		//finally, pass in array of characters to pathfinding scheduler and let it do its thing


		//update anything that has to do with guards with respect to other guards
		checkGuardRelationship ();

		//update guard status and check relationship between player and handle pathfinding
		for (int i = 0; i < numChars; i++){
			GameObject currChar = characters.transform.GetChild(i).gameObject;
			MasterBehaviour mb = behaviourScripts[i];
			updateStatus(currChar, mb);
			mb.Updatea();
			bool contained = pathFindingChars.Contains(currChar);
			bool findingAPath = mb.isReachingGoal();
			if (findingAPath && !contained){
				LinkedListNode<GameObject> c = new LinkedListNode<GameObject>(currChar);
				pathFindingChars.AddLast(c);
				if (currCharForPath == null)
					currCharForPath = c;
			} 
			else if (!findingAPath && contained){
				if (currCharForPath != null && (currChar.name == currCharForPath.Value.name)){
					currCharForPath = currCharForPath.Next;
				}
				pathFindingChars.Remove(currChar); //not sure if it finds the actualy character though
				//if we're doing continue search thing, need to reinit the characters search state
			}
		}

		//put the pathfinding characters in the pathfinder
		pathfinder.characters = pathFindingChars;
		pathfinder.currCharNode = currCharForPath;
		//do pathfinding stuff
		pathfinder.Updatea ();
		if (currCharForPath != null)
			currCharForPath = currCharForPath.Next;
		if (currCharForPath == null)
			currCharForPath = pathFindingChars.First;
	
	}

	void updateStatus(GameObject currChar, MasterBehaviour mb){
		if (mb.isDead) {
			if (mb.addToDeadSet) {
				//just died, need to make a noise when dying
				mb.addToDeadSet = false;
				deadSet.Add (currChar);
			}
			return;
		}
		mb.isShooting = false;
		int oldAlertLevel = mb.alertLevel;
		float playerAngle;
		if (mb.seesPlayer)
			playerAngle = 360.0f;
		else
			playerAngle = 30.0f + (10.0f * mb.alertLevel);
//		Debug.Log ("isGoaling0: " + mb.isGoaling);
		updateSniperInfoForChar (currChar, mb);
//		Debug.Log ("isGoaling1: " + mb.isGoaling);
		updatePlayerInfoForChar (currChar, mb, playerAngle);
//		Debug.Log ("isGoaling2: " + mb.isGoaling);
	}

	void updateSniperInfoForChar(GameObject currChar, MasterBehaviour mb){
		if (mb.knowsOfSniper ()) {
			if (!mb.seesPlayer)
				mb.takingCover = true;
			if (!mb.isGoingToSeenPlayerPos && !mb.reachedCover) {
				mb.poi = mb.takeCover.coverPoint (currChar.transform.position);
				mb.isGoaling = true;
				mb.isGoingToCover = true;
				mb.isGoingToSeenPlayerPos = false;
				mb.coverSpot = mb.poi;
			}
			if (Vector3.Distance (mb.coverSpot, currChar.transform.position) <= nodeSize && !mb.seesPlayer){
				mb.reachedCover = true;
				mb.isGoingToCover = false;
//				Debug.Log(mb.gameObject.name + " reached cover");
				mb.isGoaling = false;
			} else {
//				Debug.Log (mb.gameObject.name + " not reached cover");
				mb.reachedCover = false;
			}
		} else {
			mb.takingCover = false;
		}
	}

	void updatePlayerInfoForChar(GameObject currChar, MasterBehaviour mb, float playerAngle){
		RaycastHit hit;
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward + currChar.transform.right).normalized * sightDist, Color.red);
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward - currChar.transform.right).normalized * sightDist, Color.red);
		if (Physics.Raycast (currChar.transform.position, player.transform.position - currChar.transform.position, out hit, sightDist)) {
			float angle = Vector3.Angle (currChar.transform.forward, player.transform.position - currChar.transform.position);
			if ((hit.collider.gameObject == player) && (angle <= playerAngle)) {
				if (!mb.seesPlayer) {
					currChar.GetComponents <AudioSource> () [1].Play ();
				}
				mb.lastSeen = player.transform.position;
				mb.needsToRaiseAlertLevel = true;
				mb.seesPlayer = true;
				mb.seenTime += Time.deltaTime;
				if (mb.seenTime > 2f) {
					mb.isShooting = true;
					mb.seenTime = 0f;
				}
				mb.poi = player.transform.position;
				mb.isGoaling = true;
				mb.isGoingToSeenPlayerPos = true;
				mb.isGoingToCover = false;
				mb.disturbed = true;
				mb.takingCover = false;
			} else {
				mb.seesPlayer = false;
			}
		} else {
			mb.seesPlayer = false;
		}

		//reached the poi of where he last saw the player
		if (Vector3.Distance (mb.poi, currChar.transform.position) <= nodeSize && mb.isGoingToSeenPlayerPos) {
			mb.isGoingToSeenPlayerPos = false;
			mb.isGoaling = false;
			guessDirection(mb);
		} 


//		if (Vector3.Angle (currChar.transform.forward, player.transform.position - currChar.transform.position) <= playerAngle) {
//			if (Physics.Raycast (currChar.transform.position, player.transform.position - currChar.transform.position, out hit, sightDist)) {
//				if (hit.collider.gameObject == player) {
//					if(!mb.seesPlayer) {
//						currChar.GetComponents <AudioSource> ()[1].Play ();
//					}
//					mb.lastSeen = player.transform.position;
//					mb.needsToRaiseAlertLevel = true;
//					mb.seesPlayer = true;
//					mb.seenTime += Time.deltaTime;
//					if(mb.seenTime > 2f) {
//						mb.isShooting = true;
//						mb.seenTime = 0f;
//					}
//					mb.poi = player.transform.position;
//					mb.isGoaling = true;
//					mb.disturbed = true;
//					mb.takingCover = false;
//				//reaches poi
//				} else if (Vector3.Distance (currChar.transform.position, mb.poi) <= nodeSize) {
//					guessDirection(mb);
//				} else {
//					if(mb.seesPlayer) { //use naive bayes to determine where the character should search next
//						Debug.Log ("extending");
//						mb.poi = player.transform.position + player.transform.forward * 10f;
//						mb.isGoaling = true;
//						mb.takingCover = false;
//					}
//					mb.seesPlayer = false;
//				}
//			//reaches poi
//			} else if (Vector3.Distance (currChar.transform.position, mb.poi) <= nodeSize) {
//				guessDirection(mb);
//			} else {
//			}
//		}
	}

	void guessDirection(MasterBehaviour mb){
		Debug.Log ("time to guess dir");
//		mb.seesPlayer = false;
		mb.seenTime = 0f;
		int toSearch = NB.pointsToSearch(player.transform.forward, mb.lastSeen, mb.reachGoal.state.sGrid.grid);
//		mb.seesDeadPeople = false;
//		mb.hearsSomething = false;
//		mb.health = 100.0f;
	}







	void checkGuardRelationship(){
		//add dead characters to seenDeadSet if an alive character sees a dead one
		float sightAngle = 30.0f;
		int updatedDeadSet = 0;
		for (int i = 0; i < numChars; i++) {
			GameObject currChar = characters.transform.GetChild (i).gameObject;
			MasterBehaviour mb = behaviourScripts [i];
			if (!mb.isDead){
				updatedDeadSet += checkToSeeDead(currChar, mb, sightAngle);
			}
		}
		
		//seenDeadSet now updated so pass this along to every character because assumed they are now notified of all dead positions
		if (updatedDeadSet > 0) {
			for (int i = 0; i < numChars; i++) {
				GameObject currChar = characters.transform.GetChild (i).gameObject;
				MasterBehaviour mb = behaviourScripts [i];
				if (!mb.isDead) {
					mb.updateDeadSet (seenDeadSet);
//					mb.updateSniperPos();
					mb.needsToRaiseAlertLevel = true;
					//should stop and stare for a few frames
				}
			}
		}
		
		for (int i = 0; i < numChars; i++) {
			GameObject currChar = characters.transform.GetChild (i).gameObject;
			MasterBehaviour mb = behaviourScripts [i];
			if (mb.needsToRaiseAlertLevel && !mb.isDead)
				mb.raiseAlertLevel();
			mb.needsToRaiseAlertLevel = false;
		}
	}

	int checkToSeeDead(GameObject currChar, MasterBehaviour mb, float sightAngle){
		Vector3 deadPos;
		RaycastHit hit;
		int updatedSeen = 0;
		List<int> wasRemoved = new List<int> ();
		int i = 0;
		foreach (GameObject deadChar in deadSet) {
			deadPos = deadChar.transform.position;
			Debug.DrawRay(currChar.transform.position, deadPos - currChar.transform.position, Color.yellow);
			if (Vector3.Angle(currChar.transform.forward, deadPos - currChar.transform.position) <= sightAngle){
				if (Physics.Raycast(currChar.transform.position, deadPos - currChar.transform.position, out hit, sightDist)){
					if (hit.collider.gameObject == deadChar){
						seenDeadSet.Add(deadPos);
						wasRemoved.Add(i);
						updatedSeen += 1;
						Debug.Log ("dead ones seen: " + seenDeadSet.Count);
						//he's seen a dead patrol man so he starts going to hide, and while hes going he tells others to hide
						//this takes a while
					}
				}
			}
			i++;
		}
		foreach (int j in wasRemoved) {
			deadSet.RemoveAt(j);
		}
		return updatedSeen;
	}
}
