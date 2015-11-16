using UnityEngine;
using System.Collections;

public class StandStill : NPCBehaviour {

	private int dir;
//	private int fSince;
//	private bool lastFree;
	private float shortestDist;
	
	public override void Starta () {
		base.Starta ();
		accMagDefault = 0.0f;
		speedMaxDefault = 0.0f;
		accMag = accMagDefault;
		speedMax = speedMaxDefault;
		smooth = 0.25f;
		dir = 1;
//		fSince = 0;
//		lastFree = true;
		shortestDist = 100f;
	}
	
	public override void Updatea () {
		RaycastHit hitR;
		bool currFree = Physics.Raycast (transform.position, (Mathf.Sqrt (3) * transform.forward + dir * transform.right).normalized, out hitR, shortestDist);
		if (currFree) {
			if(hitR.distance < shortestDist) {
				shortestDist = hitR.distance;
			}
			if (hitR.collider.gameObject.CompareTag("Obstacle")){// && fSince > 60 && lastFree) {
				dir *= -1;
//				fSince = 0;
			}
		}
		RotateTo (transform.position + dir*transform.right);
//		fSince += 1;
//		lastFree = !currFree;
	}

//	public void setTurnThreshold
}
