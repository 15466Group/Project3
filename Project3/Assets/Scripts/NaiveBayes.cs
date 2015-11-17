using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NaiveBayes : Object {

	//this is used when an npc was chasing the player, but lost sight of the player
	//so the npc either chooses to search to the right or to the left of the player's last seen position

	private float hiddenSpaceCost;
	private float maxDist;
	public NaiveBayes(float hspc, float md){
		hiddenSpaceCost = hspc;
		maxDist = md;
	}

	public class Tuple
	{
		public int yes { get; set; }
		public int no { get; set; }

		public Tuple(int y, int n){
			yes = y;
			no = n;
		}

		public void increment(bool y){
			if (y) 
				yes++;
			else
				no++;
		}

		//if y then get probability of y else get probability of no
		public float getProbability(bool y){
			float total = (float)yes + no;
			if (total <= 0f) return 0f;
			if (y) {
				return yes / total;
			} else {
				return no / total;
			}
		}
	}
	
	//these are the inpute to the naive bayes predictor
	//right is to the right of the forwardDirection
	//each Tuple[] is of size 2, where index 0 represents notRight, and index 1 represents toRight
	public Tuple[] rmd { get; set; } //rightMoreDense - there are more buildings on the right side than on the left
	public Tuple[] rmv { get; set; } //rightMoreVisibleBySniper - only used if sniper pos known
	public Tuple[] rhb { get; set; } //rightHasClosestBuilding - the right side is the closer building

	public bool rMoreDense { get; set; }
	public bool rMoreVisible { get; set; }
	public bool rHasClosestBuild { get; set; }
	public int toSearch { get; set; } //1 to search the right, 0 to search to left
	private Vector3 rightDir; //vector for the local x direction of the player
	private Vector3 lastSeen; //the player's last known location
	private int rightCount;
	private int notRightCount;

	public void Starta(){
		rmd = new Tuple[2];
		rmd[0] = new Tuple (0, 0); //rightMoreDense given 0 => rightMoreDense given notRight? = (yesCount, noCount)
		rmd[1] = new Tuple (0, 0); //rightMoreDense given 1 => rightMoreDense given right? = (yesCount, noCount)

		rmv = new Tuple[2];
		rmv[0] = new Tuple (0, 0);
		rmv[1] = new Tuple (0, 0);

		rhb = new Tuple[2];
		rhb[0] = new Tuple (0, 0);
		rhb[1] = new Tuple (0, 0);

		rMoreDense = false;
		rMoreVisible = false;
		rHasClosestBuild = false;
		toSearch = 1;
		rightCount = 0;
		notRightCount = 0;
		rightDir = Vector3.zero;
		lastSeen = Vector3.zero;
	}

	//returns 0 or 1, 1 to search right, 0 to search notRight/left
	//angle between a and rightDirection is 0 - 90, then that's to the right, 90 - 180 is 
	public int pointsToSearch(Vector3 rightDirection, Vector3 lastSeenPos, Node[,] nodes){
		float closestBuildOnLeft = Mathf.Infinity; //for closestBuild
		float closestBuildOnRight = Mathf.Infinity;
		int densityRight = 0; //for de
		int densityLeft = 0;
		int sniperCanSeeRight = 0;
		int sniperCanSeeLeft = 0;
		float currDist;
		rightDir = rightDirection;
		lastSeen = lastSeenPos;
		foreach (Node n in nodes) {
//			Debug.DrawLine(lastSeen, n.loc, Color.cyan, 2f);
			currDist = Vector3.Distance(n.loc, lastSeen);
			if (currDist <= maxDist){
				float angleBtwn = calcAngleBtwn(n.loc, lastSeen);
				if (isLeft(angleBtwn)){//to the left
					Debug.DrawLine(lastSeen, n.loc, Color.green, 3f);
					if (currDist < closestBuildOnLeft)
						closestBuildOnLeft = currDist;
					if (n.spaceCost == 0f) //or !n.free 
						densityLeft++;
					if (n.spaceCost > hiddenSpaceCost)
						sniperCanSeeLeft++;
				} else if (isRight(angleBtwn)){ //to the right
					Debug.DrawLine(lastSeen, n.loc, Color.blue, 3f);
					if (currDist < closestBuildOnRight)
						closestBuildOnRight = currDist;
					if (n.spaceCost == 0f) //or !n.free 
						densityRight++;
					if (n.spaceCost > hiddenSpaceCost)
						sniperCanSeeRight++;
				} else {
				}
			}
		}
		setLatestInputs (closestBuildOnLeft, closestBuildOnRight, sniperCanSeeLeft, sniperCanSeeRight, densityRight, densityLeft);
		//now that we set the inputs, use them to compute probabilities 
		calculateProbabilities ();
		if (toSearch == 1)
			return toSearch;
		else
			return -1;
	}

	//between 0 and 360
	float calcAngleBtwn(Vector3 from, Vector3 to){
		Vector3 relative = from - to;
		float angle = Vector3.Angle(rightDir, relative);
		float sign = Mathf.Sign (Vector3.Dot(Vector3.up, Vector3.Cross(rightDir, relative)));
		return (angle * sign);
	}

	void calculateProbabilities(){
		float toRightProb = calculateInputProb (1) * getDirProbability(true);
		float notToRightProb = calculateInputProb (0) * getDirProbability(false);
		if (toRightProb > notToRightProb)
			toSearch = 1;
		else
			toSearch = 0;
	}

	float calculateInputProb(int index){
		float densityCondProb = rmd [index].getProbability (rMoreDense);
		float visCondProb = rmv [index].getProbability (rMoreVisible);
		float closeBuildCondProb = rhb [index].getProbability (rHasClosestBuild);
		return densityCondProb * visCondProb * closeBuildCondProb;
	}

	float getDirProbability(bool onRight){
		float total = (float)rightCount + notRightCount;
		if (total <= 0)
			return 0;
		if (onRight)
			return rightCount / total;
		else 
			return notRightCount / total;
	}

	void setLatestInputs(float closestBuildOnLeft, float closestBuildOnRight, 
	                     float sniperCanSeeLeft, float sniperCanSeeRight, 
	                     float densityRight, float densityLeft){
		if (closestBuildOnRight < closestBuildOnLeft)
			rHasClosestBuild = true;
		else
			rHasClosestBuild = false;
		
		if (sniperCanSeeRight < sniperCanSeeLeft)
			rMoreVisible = false;
		else
			rMoreVisible = true;
		
		if (densityRight > densityLeft)
			rMoreDense = true;
		else
			rMoreDense = false;
	}


	//if foundplayer, then using the values set by a character rMoreDense, rMoreVisible, rHasClosestBuild,
	//and toSearch, update rightCount, notRightCount and rmd, rmv, rhb, the inputs
	public void updateInputs(bool foundPlayer, Vector3 playerPos){
		int index;
		if (foundPlayer) {
			//check where player actually is with respect to the right direction, rightDir
			float angleBtwn = calcAngleBtwn (playerPos, lastSeen);
			if (isLeft(angleBtwn)) { //left
				notRightCount++;
				index = 0;
			} else if (isRight (angleBtwn)) {
				rightCount++;
				index = 1;
			} else {
				index = (toSearch + 1) % 2; //increment the 'other one'
			}
		} else {
			//did not find him so assume he went other way
			if (toSearch == 1){
				index = 0;
				notRightCount++;
			} else {
				index = 1;
				rightCount++;
			}
		}
		rmd[index].increment(rMoreDense);
		rmv[index].increment(rMoreVisible);
		rhb[index].increment(rHasClosestBuild);
	}

	bool isLeft(float angleBtwn){
		return (-120 <= angleBtwn && angleBtwn <= 0);
	}

	bool isRight(float angleBtwn){
		return (0 < angleBtwn && angleBtwn <= 120);
	}
}
