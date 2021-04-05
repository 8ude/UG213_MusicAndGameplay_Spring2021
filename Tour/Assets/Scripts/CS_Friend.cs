using UnityEngine;
using System.Collections;

public class CS_Friend : MonoBehaviour {
	[SerializeField] Rigidbody2D myRigidbody2D;
	[SerializeField] float mySpeed;
	private Vector2 myDirection;
	private GameObject myTarget;
	private Vector2 myTargetPositionDelta;
	// Use this for initialization
	void Start () {
		
		//AUDIO STUFF
		if (CS_AudioManager.Instance.friendFlyingSounds.Length > 0) {
			int randClipIndex = Random.Range(0, CS_AudioManager.Instance.friendFlyingSounds.Length - 1);
			GetComponent<AudioSource>().clip = CS_AudioManager.Instance.friendFlyingSounds[randClipIndex];
			GetComponent<AudioSource>().loop = true;
			GetComponent<AudioSource>().Play();
		}
		
		myTarget = GameObject.Find (CS_Global.NAME_PLAYER);
		myTargetPositionDelta = new Vector2 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
		myTargetPositionDelta.Normalize ();
		//Debug.Log (myTargetPositionDelta.magnitude);

		myDirection = myTarget.transform.position - this.transform.position;
		myDirection += myTargetPositionDelta;
	}
	
	// Update is called once per frame
	void Update () {
		myDirection = myTarget.transform.position - this.transform.position;
		myDirection += myTargetPositionDelta;

		myRigidbody2D.velocity = myRigidbody2D.velocity.normalized;
//		myRigidbody2D.velocity.Normalize();
		myRigidbody2D.velocity += myDirection * Time.deltaTime;
		myRigidbody2D.velocity = myRigidbody2D.velocity.normalized;
//		myRigidbody2D.velocity.Normalize();
		//Debug.Log (myRigidbody2D.velocity);
		myRigidbody2D.velocity *= mySpeed;

	}
}
