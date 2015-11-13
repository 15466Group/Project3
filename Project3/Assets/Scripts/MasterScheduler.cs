using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterScheduler : MonoBehaviour {

	public GameObject player;
	public GameObject characters;

	public GameObject plane;
	public GameObject swamps;
	public float nodeSize;

	private int numChars;

	private MasterBehaviour[] behaviourScripts;

	private PathfindingScheduler pathfinder;
	private LinkedList<GameObject> pathFindingChars;
	private LinkedListNode<GameObject> currCharForPath;

	private float timer;
	private float seenTime;
	private float shootDistance;

	// Use this for initialization
	void Start () {
		timer = 0.0f;
		numChars = characters.transform.childCount;
		Debug.Log (numChars);
		behaviourScripts = new MasterBehaviour[numChars];
		
		for (int i = 0; i < numChars; i++) {
			MasterBehaviour mb = characters.transform.GetChild(i).GetComponent<MasterBehaviour>();
			mb.Starta(plane, swamps, nodeSize);
			behaviourScripts[i] = mb;
		}

		pathfinder = new PathfindingScheduler ();
		pathFindingChars = new LinkedList<GameObject> ();


		//get rid of swamp colliders
		if (swamps != null) {
			int swampCount = swamps.transform.childCount;
			for (int k = 0; k < swampCount; k++) {
				swamps.transform.GetChild (k).GetComponent<MeshCollider> ().enabled = false;
			}
		}
		shootDistance = 10f;
	
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

//		//perform the behaviors now that their staus is updated
//		for (int j = 0; j < numChars; j++) {
//			behaviourScripts[j].Updatea();
//		}

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
//		if (timer < 5.0f) {
//			mb.seesPlayer = false;
//			mb.seesDeadPeople = false;
//			mb.hearsSomething = false;
//			mb.health = 100.0f;
//			Debug.Log(timer);
//		} else if (timer < 25.0f) {
//			Debug.Log ("pathfinding time... " + player.transform.position);
//			mb.seesPlayer = true;
//			mb.poi = player.transform.position;
//			Debug.Log ("pathfinding time start... " + currChar.transform.position);
//		} else {
//			timer = 0.0f;
//		}
		if (mb.isDead) {
			return;
		}
		mb.isShooting = false;
		RaycastHit hit;
		float angle;
		if (mb.seesPlayer)
			angle = 360.0f;
		else
			angle = 30.0f;
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward + currChar.transform.right).normalized * 100.0f, Color.red);
		Debug.DrawRay (currChar.transform.position, (Mathf.Sqrt (3) * currChar.transform.forward - currChar.transform.right).normalized * 100.0f, Color.red);
		if (Vector3.Angle (currChar.transform.forward, player.transform.position - currChar.transform.position) <= angle) {
			if (Physics.Raycast (currChar.transform.position, player.transform.position - currChar.transform.position, out hit, 100.0f)) {
				if (hit.collider.gameObject == player) {
					if(!mb.seesPlayer) {
						currChar.GetComponents <AudioSource> ()[1].Play ();
					}
					mb.seesPlayer = true;
					mb.seenTime += Time.deltaTime;
					if(mb.seenTime > 1f) {
						mb.isShooting = true;
						mb.seenTime = 0f;
					}
					mb.poi = player.transform.position;
					mb.disturbed = true;
				} else if (Vector3.Distance (currChar.transform.position, mb.poi) < 10f) {
					mb.seesPlayer = false;
					mb.seenTime = 0f;
					mb.seesDeadPeople = false;
					mb.hearsSomething = false;
					mb.health = 100.0f;
				} else {
				}
			} else if (Vector3.Distance (currChar.transform.position, mb.poi) < 10f) {
				mb.seesPlayer = false;
				mb.seenTime = 0f;
				mb.seesDeadPeople = false;
				mb.hearsSomething = false;
				mb.health = 100.0f;
			} else {
			}
		}
	}
}
