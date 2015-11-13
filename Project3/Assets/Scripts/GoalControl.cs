using UnityEngine;
using System.Collections;

public class GoalControl : MonoBehaviour {

	public string idle;
	public string walking;
	public string running;
	public string dying;

	private Animation anim;
	private float smooth;
	private float scaler;
	private Vector3 velocity;
	private float walkingSpeed;

	private Vector3 previousValidPos;
	private float radius;
	private int obstacleLayer;
	private int soldierLayer;
	private int health;

	public bool isDead { get; set; } 
	private Texture2D healthTex;
	
	void Start()
	{
		radius = 2.0f;
		smooth = 5.0f;
		scaler = 20.0f;
		walkingSpeed = 5.0f;
		velocity = Vector3.zero;
		anim = GetComponent<Animation> ();
		anim.CrossFade (idle);
		previousValidPos = transform.position;
		soldierLayer = 1 << (LayerMask.NameToLayer("Soldier"));
		obstacleLayer = 1 << (LayerMask.NameToLayer("Obstacles"));
		health = 5;
		healthTex = new Texture2D(1, 1);
		healthTex.SetPixel(0,0,Color.green);
		healthTex.Apply();
	}
	void Update()
	{
		if (isDead)
			return;
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		velocity = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		velocity = velocity * scaler;
		Vector3 targetPosition = transform.position + velocity * Time.deltaTime;
		if (velocity != new Vector3())
			RotateTo (targetPosition);
		if (checkCollisions ()) {
			previousValidPos = transform.position;
			transform.position += velocity * Time.deltaTime;
		}
		doAnimation ();
		
	}

	void doAnimation(){
		float mag = velocity.magnitude;
		if (mag > 0.0f && mag <= walkingSpeed) {
			anim.CrossFade (walking);
		} else if (mag > walkingSpeed) {
			anim.CrossFade (running);
		} else {
			anim.CrossFade (idle);
		}
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

	void OnDrawGizmos(){
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere (transform.position + (transform.up * transform.localScale.y*1.3f), radius);
	}

	bool checkCollisions(){
		Collider[] hits = Physics.OverlapSphere (transform.position + (transform.up * transform.localScale.y*1.3f), radius, obstacleLayer | soldierLayer);
		if (hits.Length > 0) {
			transform.position = previousValidPos;
			return false;
		}
		return true;
	}

	public void getHit() {
		health --;
		if (health == 0) {
			die ();
		}
	}

	void die() {
		isDead = true;
		anim.CrossFade (dying);
	}

	void OnGUI () {
		for (int i = 0; i < health; i++) {
			DrawQuad (new Rect(15 + 20*i, Screen.height - 50, 0.5f, 30.0f));
		}
	}
	void DrawQuad(Rect position) {
		GUI.skin.box.normal.background = healthTex;
		GUI.Box(position, GUIContent.none);
	}
}
