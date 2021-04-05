using UnityEngine;
using System.Collections;

public class CS_Site : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject.Find (CS_Global.NAME_LOADSTAGE).GetComponent<CS_LoadStage> ().FindASite ();
	}
}
