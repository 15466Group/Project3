using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterBehaviour : MonoBehaviour {

	private Animation anim;
	public Vector3 poi { get; set; }  //point of interest that he reaches goal on
	public float health { get; set; }
	public bool seesPlayer { get; set; }
	public Vector3 lastSeen { get; set; }
	public Vector3 lastSeenForward { get; set; }
	public bool seesDeadPeople { get; set; }
	private List<Vector3> deadPeopleSeen;

	public bool hearsSomething { get; set; }
	public bool isDead { get; set; }
	public bool addToDeadSet { get; set; }
	public bool isShooting { get; set; }
	public bool disturbed { get; set; }
	public bool sniperPosKnown { get; set; }
	public Vector3 sniperPos { get; set; }
	public int alertLevel { get; set; }
	public int maxAlertLevel { get; set; }
	public bool needsToRaiseAlertLevel { get; set; }
	public bool takingCover { get; set; }
	public Vector3 coverSpot { get; set; }
	public bool reachedCover { get; set; }
	public bool isReloading { get; set; }
	public bool isGoaling { get; set; }
	public bool isGoingToCover { get; set; }
	public bool isGoingToSeenPlayerPos { get; set; }
	public int ammoCount { get; set; }
	public int wanderDir { get; set; }

	public float dirSearchCountDown;

	public ReachGoal reachGoal { get; set; }
	private Wander wander;
	private StandStill standstill;
	private Patrol patrol;
	public string defaultBehaviour;
	private Vector3 velocity;
	public TakeCover takeCover { get; set; }

	public string idle;
	public string walking;
	public string running;
	public string dying;
	public string hit;
	public string crouchIdle;
	public string crouchRun;

	public float seenTime;

	private float walkingSpeed;
	public GameObject player;
	private GoalControl gc;
	private LineRenderer lr;

	private bool fixedDeadCollider;

	private AudioSource gunShot;
	// Use this for initialization
	public void Starta (GameObject plane, float nodeSize, Vector3 sP) {

		fixedDeadCollider = false;

		poi = Vector3.zero;
		health = 100.0f;
		seesPlayer = false;
		lastSeen = Vector3.zero;
		lastSeenForward = Vector3.zero;
		seesDeadPeople = false;
		hearsSomething = false;
		disturbed = false;
		isDead = false;
		addToDeadSet = false;
		takingCover = false;
		coverSpot = Vector3.zero;
		reachedCover = false;
		sniperPosKnown = false;
		sniperPos = sP;
		isGoaling = false;
		isGoingToSeenPlayerPos = false;
		isGoingToCover = false;
		dirSearchCountDown = 0.0f;

		reachGoal = GetComponent<ReachGoal> ();
		wander = GetComponent<Wander> ();
		standstill = GetComponent<StandStill> ();
		patrol = GetComponent<Patrol> ();
		gc = player.GetComponent<GoalControl> ();

		reachGoal.plane = plane;
		reachGoal.nodeSize = nodeSize;
		reachGoal.goalPos = poi;
		reachGoal.sniperPos = sniperPos;
		reachGoal.Starta ();
		wander.Starta ();
		patrol.Starta ();
		standstill.Starta ();
		takeCover = new TakeCover (nodeSize, reachGoal.state.sGrid.hiddenSpaceCost, 
		                           reachGoal.state.sGrid.grid, reachGoal.state.sGrid.spaceCostScalars);

		anim = GetComponent<Animation> ();
		anim.CrossFade (idle);
		walkingSpeed = 10.0f;
		gunShot = this.GetComponents<AudioSource> ()[0];

		lr = this.GetComponentInParent<LineRenderer> ();
		seenTime = 0f;
		alertLevel = 0;
		maxAlertLevel = 3;
		needsToRaiseAlertLevel = false;
		isReloading = false;
		ammoCount = 0;
//		Debug.Log (transform.name);
	}

	public void Updatea(){
		//decision tree later for different combination of senses being true
		lr.enabled = false;
		if (isDead) {
			if (!fixedDeadCollider){
				transform.gameObject.layer = LayerMask.NameToLayer("Dead"); //now dead so avoid this space;
				transform.gameObject.tag = "Dead";
				BoxCollider bc = GetComponent<BoxCollider>();
				bc.center = new Vector3(0f, -0.5f, 0f);
				fixedDeadCollider = true;
			}
			return;
		}
		//and if the character is facing the character
		if (isShooting && !gunShot.isPlaying && !gc.isDead && !isReloading) {
			shoot ();
		}
//		if (!(seesPlayer || seesDeadPeople || hearsSomething)) {
		if (!isReachingGoal()) {
//			if (!takingCover){
			if (takingCover && dirSearchCountDown <= 0) {
				//donothing;
				standstill.Updatea ();
				velocity = standstill.velocity;
			}
			else if(disturbed) {
				if(wanderDir == -1 && dirSearchCountDown > 0f) {
					float tempT = Mathf.Acos (lastSeenForward.x);
					if(lastSeenForward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.minT = tempT;
					wander.maxT = tempT + (2f*Mathf.PI / 3f);
					dirSearchCountDown -= Time.deltaTime;
				}
				else if(wanderDir == 1 && dirSearchCountDown > 0f) {
					float tempT = Mathf.Acos (lastSeenForward.x);
					if(lastSeenForward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.maxT = tempT;
					wander.minT = tempT - (2f*Mathf.PI / 3f);
					dirSearchCountDown -= Time.deltaTime;
				}
				else {
					float tempT = Mathf.Acos (transform.forward.normalized.x);
					if(transform.forward.z < 0) {
						tempT = 2*Mathf.PI - tempT;
					}
					wander.minT = tempT - Mathf.PI/4f;
					wander.maxT = tempT + Mathf.PI/4f;
				}
				wander.Updatea();
				velocity = wander.velocity;
			}
			else {

				doDefaultBehaviour();
			}
//			}
		} else {
//			takingCover = false;
//			Debug.Log("Update GoalPos to: " + reachGoal.goalPos);
			reachGoal.goalPos = poi;
			velocity = reachGoal.velocity;
		}
		//reaching goal is done with pathfinding which is handled by the pathfinder schedule and the masterscheduler

		doAnimation ();
	}

	void doDefaultBehaviour(){
		if (string.Compare("StandStill", defaultBehaviour) == 0) {
			standstill.Updatea ();
			velocity = standstill.velocity;
		} else if (string.Compare("Patrol", defaultBehaviour) == 0) {
//			Debug.Log (transform.gameObject.name + ": defaulting");
			patrol.Updatea ();
			velocity = patrol.velocity;
		} else {
			wander.Updatea();
			velocity = wander.velocity;
		}

	}

	public bool isReachingGoal(){
		//		return (seesPlayer || seesDeadPeople || hearsSomething) && !isDead;
		//		return (seesPlayer || hearsSomething) && !isDead;
		return ((isGoingToCover || isGoingToSeenPlayerPos) && !isDead);
	}

	public void getHit(int damage) {
		if (isDead) {
			return;
		}
		if (damage >= 3) {
			isDead = true;
			addToDeadSet = true;
			anim.CrossFade (dying);
			//need to make a noise when dying
		}
	}

	public void shoot () {
		gunShot.Play ();
		lr.SetPosition (0, transform.position + Vector3.up);
		lr.SetPosition (1, player.transform.position + Vector3.up);
		lr.SetWidth (1f, 1f);
		lr.enabled = true;
		gc.getHit ();

	}

	public void doAnimation(){
//		Debug.Log ("doinganimation");
		if (isDead) {
			return;
		}
		float mag = velocity.magnitude;
		if (takingCover){
			if (mag > 0.0f && mag <= walkingSpeed) {
				anim.CrossFade (crouchRun);
			} else if (mag > walkingSpeed) {
				anim.CrossFade (crouchRun);
			} else {
				anim.CrossFade (crouchIdle);
			}
		} else {
			if (mag > 0.0f && mag <= walkingSpeed) {
				anim.CrossFade (walking);
			} else if (mag > walkingSpeed) {
				anim.CrossFade (running);
			} else {
				anim.CrossFade (idle);
			}
		}
	}

	public void updateDeadSet(List<Vector3> seenDeadSet){
		//do the animation or draw the alert thing
		seesDeadPeople = true;
		deadPeopleSeen = seenDeadSet;
	}

	public void updateSniperPos(){
		sniperPosKnown = true;
//		reachGoal.state.sGrid.sniperPosKnown = true;
		reachGoal.updateGridSniperPos ();
//		Debug.Log ("knows sniper pos");
//		Debug.Break ();
	}

	public void raiseAlertLevel(){
		return;
	}

	//either from hearing, seeing, or knowing another guard was taken out
	public bool knowsOfSniper(){
		return (sniperPosKnown || seesDeadPeople);
	}
}
