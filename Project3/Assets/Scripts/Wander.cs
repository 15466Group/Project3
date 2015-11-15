using UnityEngine;
using System.Collections;

public class Wander : NPCBehaviour {
	
	private Vector3 newPos;
	private float radius;
	
	private Vector3 targetDir;
	private Vector3 tempDir;
	protected float rotationSpeedDeg { get; set; }
	protected float rotationSpeedDegDefault { get; set; }
	
	// Use this for initialization
	public override void Starta () {
		base.Starta ();
		newPos = new Vector3 ();
		radius = 2.0f;
		accMagDefault = 50.0f;
		rotationSpeedDegDefault = 1.0f;
		rotationSpeedDeg = rotationSpeedDegDefault;
		accMag = accMagDefault;
		
		targetDir = transform.position + transform.forward.normalized * radius;
		tempDir = targetDir;
		isWanderer = true;
		isReachingGoal = false;
		speedMax = 10.0f;
		
	}
	
	// Update is called once per frame
	public override void Updatea () {
		//choosing a new position to accelerate towards
		if (biasDir == Vector3.zero) {
			//the character is not being affected by any obstacle
			if (Vector3.Angle (tempDir.normalized, targetDir.normalized) < 2.0f) {
				float tempT = Mathf.Acos (transform.forward.normalized.x);
				if(transform.forward.z < 0) {
					tempT = 2*Mathf.PI - tempT;
				}
				//				float theta = Random.Range (0.0f, 360.0f) * Mathf.Deg2Rad;
				float theta = Random.Range (tempT - Mathf.PI/4f, tempT + Mathf.PI/4f);
				//				float theta = Random.Range (tempT - 0.05f, tempT + 0.05f);
				float newX = Mathf.Cos (theta) * radius;
				float newZ = Mathf.Sin (theta) * radius;
				targetDir = new Vector3 (newX, 0.0f, newZ);
				Debug.DrawRay (transform.position, targetDir * 200f, Color.green, 5f);
				rotationSpeedDeg = rotationSpeedDegDefault;
			}
		} else {
			//the character is being affected by an obstacle
			rotationSpeedDeg = rotationSpeedDegDefault * 20.0f;
			if (Vector3.Angle (targetDir, biasDir) > 90.0f) {
				targetDir = biasDir;
			}
		}
		
		
		//for smooth turning, we know the new position we want to get to, choose points along a radius
		//to turn towards that new point until the tempDir and the targetDir are about the same
		tempDir = Vector3.RotateTowards (tempDir, targetDir, Mathf.Deg2Rad * rotationSpeedDeg, 0.0f);
		//		Debug.DrawRay (transform.position, tempDir.normalized * rayDist * 1.25f, Color.yellow);
		//		Debug.DrawRay (transform.position, targetDir.normalized * rayDist * 1.25f, Color.cyan);
		newPos = transform.position + tempDir;
		
		target = newPos;
		base.Updatea ();
	}
}
