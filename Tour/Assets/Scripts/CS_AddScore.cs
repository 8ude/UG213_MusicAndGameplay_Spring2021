using UnityEngine;
using System.Collections;

public class CS_AddScore : MonoBehaviour {
	
	[SerializeField] int myScore;

	void Start () {
		GameObject.Find (CS_Global.NAME_LOADSTAGE).GetComponent<CS_LoadStage> ().AddScore (myScore);
	}

}
