using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CS_Worker : MonoBehaviour {
	private List<Vector2> myTargetList;
	private Vector2 myTarget;
	[SerializeField] Vector2 myRandomDestination;
	[SerializeField] float myRandomDestinationPosibility;
	[SerializeField] float myVelocity;
	// Use this for initialization
	void Start () {
		SetNewTarget ();
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 t_myPosition = this.transform.position;
		Vector2 t_direction = myTarget - t_myPosition;
		t_myPosition += t_direction.normalized * myVelocity * Time.deltaTime;
		this.transform.position = t_myPosition;
		if (Vector2.Distance (t_myPosition, myTarget) < myVelocity * Time.deltaTime) {
			//arrived
			SetNewTarget();
		}
	}

	private void SetNewTarget () {
		if (myTargetList == null) {
			CreateRandomDesitination ();
			return;
		}

		if (myTargetList.Count < 1) {
			CreateRandomDesitination ();
			return;
		}

		float t_will = Random.Range (0f, 1f);
		if (t_will > myRandomDestinationPosibility) {
			//want to work
			myTarget = myTargetList [Random.Range (0, myTargetList.Count)];
		} else {
			CreateRandomDesitination ();
		}
	}

	private void CreateRandomDesitination () {
		myTarget = new Vector2 (
			Random.Range (-myRandomDestination.x, myRandomDestination.x), 
			Random.Range (-myRandomDestination.y, myRandomDestination.y)
		);
	}

	public void InitMyTargetList (List<Vector2> t_siteList) {
		myTargetList = t_siteList;
	}
}
