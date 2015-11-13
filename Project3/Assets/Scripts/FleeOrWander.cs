using UnityEngine;
using System.Collections;

public class FleeOrWander : NPCBehaviour {

	public GameObject goal;
	private Flee fleeScript;
	private Wander wanderScript;
	private float fleeDist;
	private float interDist;
	private float wanderDist;
	private float actualDist;

	//indicates the last zone the guy was in, 0 for flee zone and 1 for wander zone
	private int lastZone;

	// Use this for initialization
	public override void Starta () {
		fleeScript = GetComponent<Flee>();
		wanderScript = GetComponent<Wander> ();
		base.Starta ();
		fleeDist = rayDist;
		interDist = 3.0f * rayDist;
		isReachingGoal = false;

		actualDist = Vector3.Distance (transform.position, goal.transform.position);
		setScripts ();
	}

	void setScripts(){
		if (actualDist < fleeDist) {
			lastZone = 0;
			if (!fleeScript.enabled){
				fleeScript.acceleration = wanderScript.acceleration;
			}
			fleeScript.enabled = true;
			wanderScript.enabled = false;
		} else if (actualDist < interDist) {
			if(lastZone == 0) {
				if (!fleeScript.enabled){
					fleeScript.acceleration = wanderScript.acceleration;
				}
				fleeScript.enabled = true;
				wanderScript.enabled = false;
			}
			else {
				if (!wanderScript.enabled) {
					wanderScript.acceleration = fleeScript.acceleration;
				}
				fleeScript.enabled = false;
				wanderScript.enabled = true;
			}
		} else {
			lastZone = 1;
			if (!wanderScript.enabled) {
				wanderScript.acceleration = fleeScript.acceleration;
			}
			fleeScript.enabled = false;
			wanderScript.enabled = true;
		}
	}

//	void OnDrawGizmos(){
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawWireSphere (transform.position, fleeDist);
//		Gizmos.DrawWireSphere (transform.position, interDist);
//	}

	// Update is called once per frame
	public override void Updatea () {
		actualDist = Vector3.Distance (transform.position, goal.transform.position);
		setScripts ();
	}
}
