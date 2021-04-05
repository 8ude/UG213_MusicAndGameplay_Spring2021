using UnityEngine;
using System.Collections;

public class CS_Camera : MonoBehaviour {
	private GameObject myPlayer;
	private bool isClear = false;
	[SerializeField] Vector3 myDeltaPostion;
	[SerializeField] float myMoveSpeed;
	[SerializeField] float myPreMoveDistance;
	private Vector3 myPreMovePosition;

	private float mySize = CS_Global.CAMERA_SIZE_DEFAULT;
	[SerializeField] float mySizeRatio;						//0-1	0: don't change	1:fast change 

	// Use this for initialization
	void Start () {
		myPlayer = GameObject.Find (CS_Global.NAME_PLAYER);
		myPreMovePosition = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if (isClear)
			this.transform.position = Vector3.Lerp (
				this.transform.position, 
				Vector3.zero + myDeltaPostion, 
				Time.deltaTime * myMoveSpeed
			);
		else
			this.transform.position = Vector3.Lerp (
				this.transform.position,
				myPlayer.transform.position + myDeltaPostion + myPreMovePosition, 
				Time.deltaTime * myMoveSpeed
			);

		float t_ratio = Time.deltaTime * mySizeRatio;
		if (t_ratio > 1)
			t_ratio = 1;
		GetComponent<Camera> ().orthographicSize = (1 - t_ratio) * GetComponent<Camera> ().orthographicSize + t_ratio * mySize;
	}

	public void SetPreMovePosition (Vector2 t_position) {
		myPreMovePosition = t_position * myPreMoveDistance;
	}

	public void SetIsClear (bool t_isClear) {
		isClear = t_isClear;
	}

	public void SetSize (float t_size) {
		mySize = t_size;
	}

	public Vector3 GetDeltaPostion () {
		return myDeltaPostion;
	}
}
