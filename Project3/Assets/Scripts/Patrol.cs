using UnityEngine;
using System.Collections;

public class Patrol : NPCBehaviour {
	
	public float secsTillTurnAround;
	private float seconds;
	public Vector3 direction;

	public override void Starta () {
		base.Starta ();
		speedMax = 10.0f;
		accMag = 50.0f;
		seconds = 0.0f;
		direction = new Vector3 (direction.x, 0.0f, direction.z).normalized * 200.0f;
	}
	
	public override void Updatea () {
		if (seconds < secsTillTurnAround) {
			target = transform.position + direction;
			//target = Vector3.zero;
		} else if (seconds < 2.0f * secsTillTurnAround) {
			target = transform.position + (-1.0f) * direction;
		} else {
			seconds = 0.0f;
		}
		seconds += Time.deltaTime;
		base.Updatea();
	}
}

