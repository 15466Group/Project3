﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterBehaviour : MonoBehaviour {

	private Animation anim;
	public Vector3 poi { get; set; }  //point of interest that he reaches goal on
	public float health { get; set; }
	public bool seesPlayer { get; set; }
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
	public bool isReloading { get; set; }
	public int ammoCount { get; set; }

	public ReachGoal reachGoal { get; set; }
	private Wander wander;
	private StandStill standstill;
	private Patrol patrol;
	public string defaultBehaviour;
	private Vector3 velocity;
	private TakeCover takeCover;

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
		seesDeadPeople = false;
		hearsSomething = false;
		disturbed = false;
		isDead = false;
		addToDeadSet = false;
		takingCover = false;
		sniperPosKnown = false;
		sniperPos = sP;

		reachGoal = GetComponent<ReachGoal> ();
		wander = GetComponent<Wander> ();
		standstill = GetComponent<StandStill> ();
		patrol = GetComponent<Patrol> ();
		takeCover = GetComponent<TakeCover> ();
		gc = player.GetComponent<GoalControl> ();

		reachGoal.plane = plane;
		reachGoal.nodeSize = nodeSize;
		reachGoal.goalPos = poi;
		reachGoal.sniperPos = sniperPos;
		reachGoal.Starta ();
		wander.Starta ();
		patrol.Starta ();
		standstill.Starta ();
		takeCover.Starta ();
		takeCover.dist = nodeSize;
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
//			Debug.Log ("not Reaching Goal");
//			wander.Updatea();
//			velocity = wander.velocity;
			if (sniperPosKnown){
				takeCover.setGridAndSniperPos(reachGoal.returnGrid(), sniperPosKnown, reachGoal.state.sGrid.hiddenSpaceCost, reachGoal.state.sGrid.spaceCostScalars);
			}
//			Debug.Log (seesDeadPeople);
			if (seesDeadPeople || sniperPosKnown){
				if (takeCover.inCoverTime < 10.0f){
					takeCover.Updatea();
					velocity = takeCover.velocity;
					takingCover = true;
				} else {
					takeCover.Starta ();
					takingCover = false;
				}
			}
			if (!takingCover){
				if(disturbed) {
					wander.Updatea();
					velocity = wander.velocity;
				}
				else {
					doDefaultBehaviour();
				}
			}
		} else {
			takingCover = false;
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
			patrol.Updatea ();
			velocity = patrol.velocity;
		} else {
			wander.Updatea();
			velocity = wander.velocity;
		}

	}

	public bool isReachingGoal(){
//		return (seesPlayer || seesDeadPeople || hearsSomething) && !isDead;
		return (seesPlayer || hearsSomething) && !isDead;
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
		reachGoal.updateSniperPos ();
//		Debug.Log ("knows sniper pos");
//		Debug.Break ();
	}

	public void raiseAlertLevel(){
		return;
	}
}
