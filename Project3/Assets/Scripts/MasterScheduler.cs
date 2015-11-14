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

	private float timer;
	private float seenTime;
	private float shootDistance;
	private float sightDist;

	// Use this for initialization
	void Start () {
		timer = 0.0f;
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
	
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		//loop through the characters, update their status (ie senses, poi, etc)
		//if the character is not reaching a goal, then wipe it's search state clean

		//loop through chatacters again and do behavior.Updatea

		//finally, pass in array of characters to pathfinding scheduler and let it do its thing

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

		//add dead characters to seenDeadSet if an alive character sees a dead one
		float sightAngle = 30.0f;
		bool updatedDeadSet = false;
		for (int i = 0; i < numChars; i++) {
			GameObject currChar = characters.transform.GetChild (i).gameObject;
			MasterBehaviour mb = behaviourScripts [i];
			if (!mb.isDead){
				updatedDeadSet = checkToSeeDead(currChar, mb, sightAngle);
			}
		}

		//seenDeadSet now updated so pass this along to every character because assumed they are now notified of all dead positions
		if (updatedDeadSet) {
			for (int i = 0; i < numChars; i++) {
				GameObject currChar = characters.transform.GetChild (i).gameObject;
				MasterBehaviour mb = behaviourScripts [i];
				if (!mb.isDead) {
					mb.updateDeadSet (seenDeadSet);
					//fixme get rid of this
					mb.updateSniperPos();
				}
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
			if (mb.addToDeadSet){
				//just died, need to make a noise when dying
				mb.addToDeadSet = false;
				deadSet.Add (currChar);
			}
			return;
		}
		mb.isShooting = false;
		float playerAngle;
		if (mb.seesPlayer)
			playerAngle = 360.0f;
		else
			playerAngle = 30.0f;
		updatePlayerInfoForChar (currChar, mb, playerAngle);
	}

	bool checkToSeeDead(GameObject currChar, MasterBehaviour mb, float sightAngle){
		Vector3 deadPos;
		RaycastHit hit;
		bool updatedSeen = false;
		List<int> wasRemoved = new List<int> ();
		int i = 0;
		foreach (GameObject deadChar in deadSet) {
			deadPos = deadChar.transform.position;
			Debug.DrawRay(currChar.transform.position, deadPos - currChar.transform.position, Color.yellow);
			if (Vector3.Angle(currChar.transform.forward, deadPos - currChar.transform.forward) <= sightAngle){
				if (Physics.Raycast(currChar.transform.position, deadPos - currChar.transform.position, out hit, sightDist)){
					if (hit.collider.gameObject == deadChar){
						seenDeadSet.Add(deadPos);
						wasRemoved.Add(i);
						updatedSeen = true;
						Debug.Log ("dead ones seen: " + seenDeadSet.Count);
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

	void updatePlayerInfoForChar(GameObject currChar, MasterBehaviour mb, float playerAngle){
		RaycastHit hit;
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward + currChar.transform.right).normalized * sightDist, Color.red);
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward - currChar.transform.right).normalized * sightDist, Color.red);
		if (Vector3.Angle (currChar.transform.forward, player.transform.position - currChar.transform.position) <= playerAngle) {
			if (Physics.Raycast (currChar.transform.position, player.transform.position - currChar.transform.position, out hit, sightDist)) {
				if (hit.collider.gameObject == player) {
					if(!mb.seesPlayer) {
						currChar.GetComponents <AudioSource> ()[1].Play ();
					}
					mb.seesPlayer = true;
					mb.seenTime += Time.deltaTime;
					if(mb.seenTime > 2f) {
						mb.isShooting = true;
						mb.seenTime = 0f;
					}
					mb.poi = player.transform.position;
					mb.disturbed = true;
				} else if (Vector3.Distance (currChar.transform.position, mb.poi) < 10f) {
					mb.seesPlayer = false;
					mb.seenTime = 0f;
//					mb.seesDeadPeople = false;
//					mb.hearsSomething = false;
//					mb.health = 100.0f;
				} else {
				}
			} else if (Vector3.Distance (currChar.transform.position, mb.poi) < 10f) {
				mb.seesPlayer = false;
				mb.seenTime = 0f;
//				mb.seesDeadPeople = false;
//				mb.hearsSomething = false;
//				mb.health = 100.0f;
			} else {
			}
		}
	}
}
