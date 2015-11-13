using UnityEngine;
using System.Collections;

public class Flee : NPCBehaviour {

	public GameObject goal;

	public override void Starta(){
		base.Starta ();
		target = goal.transform.position;
		doAcceleration ();
		isWanderer = false;
	}

	public override void Updatea(){
		target = goal.transform.position;
		base.Updatea ();
	}

	protected override void doAcceleration(){
		//accelerate away from the goal, but keep the obstacle avoidance accleration the same
		acceleration = (-1.0f) * calculateAcceleration (target);
		acceleration = new Vector3 (acceleration.x, 0.0f, acceleration.z).normalized * accMag;
	}

	protected override Vector3 obstacleAvoidance (float radius, Collider[] hits)
	{
		Vector3 accel =  base.obstacleAvoidance (radius, hits);
		return (-1.0f) * accel;
	}

}
