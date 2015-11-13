using UnityEngine;
using System.Collections;

[System.Serializable]
public class NPCBehaviour : MonoBehaviour {
	
	//superclass for wander and reach goal behaviors
	//accelerate object towards a position (either randomly generated [wander] or a goal [reachgoal])
	
	//private Animation anim;
	public Vector3 velocity { get; set; }
	public Vector3 acceleration { get; set; }
	protected float accMag { get; set; }
	protected float accMagDefault { get; set; }
	protected float speedMaxDefault { get; set; }
	protected float speedMax { get; set; }
	protected Vector3 target { get; set; }
	protected float epsilon { get; set; }
	protected float searchRadius { get; set; }
	protected Vector3 targetPosition { get; set; }
	protected float rayDist { get; set; }
	protected float rayDistDefault { get; set; }
	protected float closeRayDist { get; set; }
	protected float closeRayDistDefault { get; set; }

	protected Vector3 biasDir { get; set; }

	private float charWidth;
	private float smooth;
	private float obstacleWeight;
	private float charWeight;
	
	protected bool isWanderer { get; set; }
	protected bool isReachingGoal { get; set; }
	protected bool inArrivalRadius { get; set; }
	

	// Use this for initialization
	public virtual void Starta () {
		velocity = new Vector3 ();
		acceleration = new Vector3 ();
		accMagDefault = 50.0f;
		speedMaxDefault = 30.0f;
		epsilon = 2.0f;
		searchRadius = 100.0f;
		rayDistDefault = 20.0f;
		rayDist = rayDistDefault;
		closeRayDistDefault = 7.0f;
		closeRayDist = closeRayDistDefault;
		charWidth = 5.0f;
		smooth = 5.0f;
		obstacleWeight = 3.0f;
		charWeight = 1.0f;
		accMag = accMagDefault;
		speedMax = speedMaxDefault;
//		anim = GetComponent<Animation> ();
//		anim.CrossFade (idle);
	}
	
	// Update is called once per frame
	public virtual void Updatea () {
		biasDir = Vector3.zero;
		doPositionAndVelocity ();
		doAcceleration ();
		//doAnimation ();
	}

	protected void doPositionAndVelocity(){
		transform.position += velocity * Time.deltaTime;
		velocity = velocity + acceleration * Time.deltaTime;
		velocity = Vector3.ClampMagnitude (velocity, speedMax);
		//update speedMax
		veloCloseToTarget ();
		targetPosition = transform.position + velocity * Time.deltaTime;
		if (velocity != new Vector3())
			RotateTo (targetPosition);
		float targetDist = Vector3.Distance (transform.position, target);
		rayDist = Mathf.Min (rayDistDefault, targetDist);
		closeRayDist = Mathf.Min (closeRayDistDefault, targetDist);
	}

	protected virtual void doAcceleration(){
		acceleration = calculateAcceleration (target);
		acceleration = new Vector3 (acceleration.x, 0.0f, acceleration.z).normalized * accMag;
	}
	
	void RotateTo(Vector3 targetPosition){
		//maxDistance is the maximum ray distance
		Quaternion destinationRotation;
		Vector3 relativePosition;
		relativePosition = targetPosition - transform.position;
//		Debug.DrawRay(transform.position,relativePosition*10,Color.red);
//		Debug.DrawRay(transform.position,velocity.normalized*20,Color.green);
//		Debug.DrawRay(transform.position,acceleration.normalized*10,Color.blue);
		destinationRotation = Quaternion.LookRotation (relativePosition);
		transform.rotation = Quaternion.Slerp (transform.rotation, destinationRotation, Time.deltaTime * smooth);
	}
	
//	void OnDrawGizmos(){
//		Gizmos.color = Color.magenta;
//		Gizmos.DrawWireSphere (transform.position, closeRayDist);
//		Gizmos.DrawWireSphere (transform.position, rayDist);
//	}
	
	//even if something isn't directly in front of the character, should still avoid it if it's too close
	//cus he cant turn instantaneously
	Vector3 checkCloseCalls(Vector3 acceleration) {
		Collider[] hits = Physics.OverlapSphere (transform.position, closeRayDist);
		//don't include hitting self
		if (hits.Length > 1) {
			Vector3 accumulator = obstacleAvoidance(rayDist, hits);
			return accumulator.normalized * accMag;
		} else {
			return acceleration;
		}
	}
	
	protected virtual Vector3 calculateAcceleration(Vector3 target) {
		RaycastHit hitL;
		RaycastHit hitR;

		bool hitLeft = Physics.Raycast (transform.position - transform.right.normalized * charWidth, transform.forward, out hitL, rayDist);
		bool hitRight = Physics.Raycast (transform.position + transform.right.normalized * charWidth, transform.forward, out hitR, rayDist);
		
		//doesn't matter if an object is behind the goal, can still reach it withtout bumping into the object
		float distToTarget = Vector3.Distance (transform.position, target);
		if (distToTarget < hitL.distance && distToTarget < hitR.distance) {
			return (target - transform.position).normalized * accMag;
		}
		
//		Debug.DrawRay(transform.position,transform.forward * 50.0f,Color.red);
		if (hitRight || hitLeft) {
			Collider[] hits = Physics.OverlapSphere(transform.position, rayDist);
			Vector3 accumulator = obstacleAvoidance(rayDist, hits);
			accumulator += transform.forward.normalized;
			return checkCloseCalls(accumulator.normalized * accMag);
		}
		else {
			bool potentialHit = Physics.Raycast (transform.position, target - transform.position, rayDist);
			if(potentialHit) {
				return checkCloseCalls(transform.forward.normalized * accMag);
			}
			else {
				return checkCloseCalls((target - transform.position).normalized * accMag);
			}
		}
	}
	
	protected virtual Vector3 obstacleAvoidance(float radius, Collider[] hits){
		float weight = charWeight;
		Vector3 accumulator = new Vector3 ();
		foreach (Collider obstacle in hits) {
			if (obstacle.gameObject != this.gameObject && obstacle.gameObject.name != "Ground") {
				if (!obstacle.gameObject.CompareTag("Character")){
					weight = obstacleWeight;
				} else {
					//the wanderer only moves out of the way for other wanderer's
					//the reachers always move out of the way for every character
					if(isWanderer && obstacle.gameObject.GetComponent ("Wander") == null) {
						weight = 0.0f;
					}
					else {
						weight = charWeight;
					}
				}
				//doing this to find the closest point on the bounds of the obstacle,
				//	note that builtin ClosestPointOnBounds does not work on rotated objects, as we found out the hard way...
				RaycastHit hit = new RaycastHit();
				RaycastHit hitN;
				RaycastHit[] rays;
				rays = Physics.RaycastAll(transform.position, obstacle.transform.position - transform.position);
				foreach (RaycastHit h in rays){
					if (h.collider == obstacle){
						hit = h;
						break;
					}
				}
//				Debug.DrawRay(hit.point, hit.normal * accMag, Color.white);
				Vector3 normal = hit.normal;
				//raycast from position to potential closest point on bounds (may miss the object though in the else case)
				bool closest = Physics.Raycast (transform.position, normal * (-1.0f), out hitN, radius);
//				Debug.DrawRay (hitN.point, (hitN.normal) * accMag, Color.black);
				if (closest) {
					//as distance gets smaller, hitN.distance/rayDist is smaller so closer to 1.0f, bigger, closer to 0.0f
					accumulator += hitN.normal.normalized * weight * (1.0f - (hitN.distance / radius));
				} else {
					float estim = Mathf.Min (rayDist, hit.distance);
					accumulator += (transform.position - obstacle.transform.position).normalized * weight * (1.0f - (estim / radius));
				}
			}
		}
		biasDir = accumulator.normalized;
		return accumulator;
	}
	
	void veloCloseToTarget () {
//		if (isReachingGoal) {
//			float epsilon = 0.7f;
//			float xDistance = Mathf.Abs (transform.position.x - target.x);
//			float zDistance = Mathf.Abs (transform.position.z - target.z);
//			float distance = Mathf.Sqrt (xDistance * xDistance + zDistance * zDistance);
//			if (distance <= epsilon) {
//				speedMax = 0.0f; 
//			} else { //exponential growth translated up by 10, capped at originalMaxSpeed
//				speedMax = Mathf.Min (Mathf.Pow (1.1f, distance) + 10.0f, speedMaxDefault);
//			}
//		}
//		if (isReachingGoal) {
//			if (inArrivalRadius) {
//				transform.gameObject.layer = 8;
//				speedMax = 0.0f; 
//			} else { //exponential growth translated up by 10, capped at originalMaxSpeed
//				transform.gameObject.layer = 0;
//				speedMax = Mathf.Min (Mathf.Pow (1.1f, Vector3.Distance(transform.position, target)) + 10.0f, speedMaxDefault);
//			}
//		}
	}

	//upon getting hit, momentarily set velocity to zero
//	public virtual void getHit(int damage) {
//		switch (damage) {
//		case 0 :
//			break;
//		case 1 :
//			anim.CrossFade (hit);
//			Debug.Log ("hit");
//			break;
//		case 2 :
//			anim.CrossFade (hit);
//			break;
//		case 3 :
//			anim.CrossFade (dying);
//			break;
//		default:
//			Debug.Log ("default");
//			break;
//		}
//	}
}
