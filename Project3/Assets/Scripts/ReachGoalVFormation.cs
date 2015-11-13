using UnityEngine;
using System.Collections;

public class ReachGoalVFormation: NPCBehaviour {
	
	public GameObject goal;
	private static int counter = 0;
	private int VFormID;
	
	// Use this for initialization
	public override void Starta () {
		base.Starta ();
		counter ++;
		VFormID = counter;
		target = calculateVPosition ();
		acceleration = base.calculateAcceleration (target);
		isWanderer = false;
		isReachingGoal = true;
		speedMaxDefault = 35.0f;
		accMagDefault = 1000.0f;
		rayDistDefault = 40.0f;
		closeRayDistDefault = 15.0f;
	}
	
	public override void Updatea () {
		target = calculateVPosition ();
		base.Updatea ();
	}

	Vector3 calculateVPosition () {
		Vector3 goalBackwards = (-1.0f) * goal.transform.forward.normalized;
		Vector3 goalRight = goal.transform.right.normalized;
		Vector3 goalLeft = (-1.0f) * goal.transform.right.normalized;
		if (VFormID % 2 == 0) {
			return goal.transform.position + Vector3.ClampMagnitude((goalBackwards + goalRight).normalized * 10000.0f, (VFormID / 2) * (rayDistDefault * 2.0f/3.0f));
		} else {
			return goal.transform.position + Vector3.ClampMagnitude((goalBackwards + goalLeft).normalized * 10000.0f, ((VFormID + 1) / 2) * (rayDistDefault * 2.0f/3.0f));
		}
	}
}