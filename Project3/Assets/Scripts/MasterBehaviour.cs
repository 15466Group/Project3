using UnityEngine;
using System.Collections;

public class MasterBehaviour : MonoBehaviour {

	private Animation anim;
	public Vector3 poi { get; set; }  //point of interest that he reaches goal on
	public float health { get; set; }
	public bool seesPlayer { get; set; }
	public bool seesDeadPeople { get; set; }
	public bool hearsSomething { get; set; }
	public bool isDead { get; set; }
	public bool isShooting { get; set; }
	public bool disturbed { get; set;}

	public ReachGoal reachGoal { get; set; }
	private Wander wander;
	private StandStill standstill;
	private Patrol patrol;
	public string defaultBehaviour;
	private Vector3 velocity;

	public string idle;
	public string walking;
	public string running;
	public string dying;
	public string hit;

	public float seenTime;

	private float walkingSpeed;
	public GameObject player;
	private GoalControl gc;
	private LineRenderer lr;

	private bool fixedDeadCollider;

	private AudioSource gunShot;
	// Use this for initialization
	public void Starta (GameObject plane, GameObject swamps, float nodeSize) {

		fixedDeadCollider = false;

		poi = Vector3.zero;
		health = 100.0f;
		seesPlayer = false;
		seesDeadPeople = false;
		hearsSomething = false;
		disturbed = false;

		reachGoal = GetComponent<ReachGoal> ();
		wander = GetComponent<Wander> ();
		standstill = GetComponent<StandStill> ();
		patrol = GetComponent<Patrol> ();
		gc = player.GetComponent<GoalControl> ();

		reachGoal.plane = plane;
		reachGoal.swamps = swamps;
		reachGoal.nodeSize = nodeSize;
		reachGoal.goalPos = poi;
		reachGoal.Starta ();
		wander.Starta ();
		patrol.Starta ();
		standstill.Starta ();
		anim = GetComponent<Animation> ();
		anim.CrossFade (idle);
		walkingSpeed = 10.0f;
		gunShot = this.GetComponents<AudioSource> ()[0];

		lr = this.GetComponentInParent<LineRenderer> ();
		seenTime = 0f;
//		Debug.Log (transform.name);
	}

	public void Updatea(){
		//decision tree later for different combination of senses being true
		lr.enabled = false;
		if (isDead) {
			if (!fixedDeadCollider){
				transform.gameObject.layer = LayerMask.NameToLayer("Obstacles"); //now an obstacle;
				BoxCollider bc = GetComponent<BoxCollider>();
				bc.center = new Vector3(transform.position.x, -0.5f, transform.position.z);
				fixedDeadCollider = true;
			}
			return;
		}
		//and if the character is facing the character
		if (isShooting && !gunShot.isPlaying && !gc.isDead) {
			shoot ();
		}
		if (!(seesPlayer || seesDeadPeople || hearsSomething)) {
//			Debug.Log ("Wander");
//			wander.Updatea();
//			velocity = wander.velocity;
			if(disturbed) {
				wander.Updatea();
				velocity = wander.velocity;
			}
			else {
				doDefaultBehaviour();
			}
		} else {
			Debug.Log("Update GoalPos to: " + reachGoal.goalPos);
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
			patrol.Updatea ();
			velocity = patrol.velocity;
		} else {
			wander.Updatea();
			velocity = wander.velocity;
		}

	}

	public bool isReachingGoal(){
		return (seesPlayer || seesDeadPeople || hearsSomething) && !isDead;
	}

	public void getHit(int damage) {
		if (isDead) {
			return;
		}
		if (damage >= 3) {
			isDead = true;
//			Debug.Log ("isDead");
//			Debug.Break();
			anim.CrossFade (dying);
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
		if (mag > 0.0f && mag <= walkingSpeed) {
			anim.CrossFade (walking);
		} else if (mag > walkingSpeed) {
			anim.CrossFade (running);
		} else {
			anim.CrossFade (idle);
		}
	}
}
