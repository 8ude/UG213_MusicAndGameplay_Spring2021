using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CS_Subway : MonoBehaviour {

	private int myNextStationNum;
	private List<Vector2> myStationPositionList;

	private List<GameObject> myPassengerList = new List<GameObject>();

	private bool isOn = false;

	[SerializeField] float myVelocity;
	[SerializeField] float myStopTime;
	private float myTimer;

	private bool startedMoving = false;

	private AudioClip trainMovingClip;

	public GameObject playerObject;

	void Start() {
		trainMovingClip = CS_AudioManager.Instance.rideTrainSound;
		GetComponent<AudioSource>().clip = trainMovingClip;
		playerObject = GameObject.FindWithTag(CS_Global.TAG_PLAYER);
	}

	void Update () {
		if (!isOn) {
			return;
		}

		if (myTimer > 0) {
			myTimer -= Time.deltaTime;
			startedMoving = false;
			return;
		}

		if (startedMoving == false && myTimer <= 0f) {
			GetComponent<AudioSource>().Play();
			startedMoving = true;
		}

		Vector2 t_myPosition = this.transform.position;
		Vector2 t_myTargetPosition = myStationPositionList [myNextStationNum];
		Vector2 t_direction = t_myTargetPosition - t_myPosition;
		Vector2 t_deltaPosition;

		if (Vector2.Distance (t_myPosition, t_myTargetPosition) < myVelocity * Time.deltaTime) {
			//arrived
			t_deltaPosition = t_direction.normalized * Vector2.Distance (t_myPosition, t_myTargetPosition);

			myTimer = myStopTime;
			myNextStationNum++;
			if (myNextStationNum >= myStationPositionList.Count) {
				myNextStationNum -= myStationPositionList.Count;
			}
		} else {
			t_deltaPosition = t_direction.normalized * myVelocity * Time.deltaTime;
		}
			
		//move myself
		t_myPosition += t_deltaPosition;
		this.transform.position = t_myPosition;
		//move my passenger
		for (int i = 0; i < myPassengerList.Count; i++) {
			myPassengerList [i].transform.position += (Vector3)t_deltaPosition;
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.tag == CS_Global.TAG_PLAYER || other.tag == CS_Global.TAG_FRIEND || other.tag == CS_Global.TAG_WORKER) {
			Debug.Log ("EnterSubway:" + other.tag);
			
			if (myPassengerList.Contains (other.gameObject) == false)
				myPassengerList.Add (other.gameObject);
			
		}
	}

	void OnTriggerExit2D (Collider2D other) {
		if (other.tag == CS_Global.TAG_PLAYER || other.tag == CS_Global.TAG_FRIEND || other.tag == CS_Global.TAG_WORKER) {
			if (myPassengerList.Contains (other.gameObject))
				myPassengerList.Remove (other.gameObject);
		}
	}

	public void Init (List<Vector2> g_stationPositionList, int g_nextStationNumber) {
		myStationPositionList = g_stationPositionList;
		myNextStationNum = g_nextStationNumber;
		isOn = true;
	} 
}
