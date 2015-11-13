using UnityEngine;
using System.Collections;

public class StandStill : NPCBehaviour {

	public override void Starta () {
		base.Starta ();
		accMagDefault = 0.0f;
		speedMaxDefault = 0.0f;
		accMag = accMagDefault;
		speedMax = speedMaxDefault;
	}
	
	public override void Updatea () {
	
	}
}
