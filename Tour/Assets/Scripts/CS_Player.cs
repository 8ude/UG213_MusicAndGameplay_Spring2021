using UnityEngine;
using System.Collections;

public class CS_Player : MonoBehaviour {
	[SerializeField] Rigidbody2D myRigidbody2D;

	private Vector2 myDirection;
	private bool onMove;

	private Vector2 moveAxis;
	[SerializeField] float mySpeed;
	[SerializeField] float moveGravity;
	[SerializeField] float moveSensitivity;

	//AUDIO THINGS
	[SerializeField] private float moveSoundDampening = 0.8f;
	[SerializeField] private float moveSoundMaxVolume;
	private AudioSource mySource;
	
	// Use this for initialization
	void Start () {
		mySource = GetComponent<AudioSource>();
		
		if (CS_AudioManager.Instance.playerMoveSound != null) {
			mySource.clip = CS_AudioManager.Instance.playerMoveSound;
			mySource.volume = 0f;
			mySource.Play();
		} 
		onMove = false;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMove ();
	}

	public void UpdateMove () {
		if (onMove) {
			moveAxis += myDirection * moveSensitivity;
			if (moveAxis.magnitude > 1)
				moveAxis.Normalize ();
		}
//		Debug.Log ("ControlMove" + myDirection + " : " +moveAxis);

		//set the speed of the player
		myRigidbody2D.velocity = moveAxis * mySpeed;

		float t_moveAxisReduce = Time.deltaTime * moveGravity;
		if (moveAxis.magnitude < t_moveAxisReduce)
			moveAxis = Vector2.zero;
		else
			moveAxis *= (moveAxis.magnitude - t_moveAxisReduce);

		Camera.main.GetComponent<CS_Camera> ().SetPreMovePosition (moveAxis);
		//Debug.Log ("ControlMove" + myDirection + " : " +moveAxis);
		
		///////////////////
		/// AUDIO
		///////////////////

		float audioDestVolume = moveSoundMaxVolume * myRigidbody2D.velocity.magnitude / mySpeed;

		mySource.volume = Mathf.Lerp(mySource.volume, audioDestVolume, moveSoundDampening);

	}

	public void SetDirection (Vector2 g_direction) {
		myDirection = g_direction;
	}

	public void SetOnMove (bool g_onMove) {
		onMove = g_onMove;
	}
}
