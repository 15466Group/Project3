using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public GameObject cameraLead;
	private Vector3 offset = new Vector3 (-16, 155, -25);
	
	void Start()
	{

	}
	void Update()
	{
		/*float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		Vector3 velocity = new Vector3 (moveHorizontal, 0.0f, moveVertical);
		velocity = velocity * 100.0f;
		transform.position += velocity * Time.deltaTime;*/

		transform.position = cameraLead.transform.position + offset;
//		transform.forward = cameraLead.transform.position - transform.position;
//		transform.LookAt (new Vector3 (60, 0, 0));
	}
}
