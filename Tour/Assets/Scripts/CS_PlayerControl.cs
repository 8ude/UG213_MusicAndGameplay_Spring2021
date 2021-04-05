using UnityEngine;
using System.Collections;

public class CS_PlayerControl : MonoBehaviour {
	[SerializeField] Camera UICamera;
	private CS_Player myPlayer;
	private CS_LoadStage myLoadStage;

	[SerializeField] float doubleClickScale = 0.2f;
	private float doubleClickTimer = 0;

	void Start () {
		myPlayer = GameObject.Find (CS_Global.NAME_PLAYER).GetComponent<CS_Player> ();
		myLoadStage = GameObject.Find (CS_Global.NAME_LOADSTAGE).GetComponent<CS_LoadStage> ();
	}

	void Update () {
		doubleClickTimer += Time.deltaTime;
	}

	void OnMouseDown () {
		myPlayer.SetOnMove (true);
//		myPlayer.SetDirection (m);
		if (doubleClickTimer < doubleClickScale)
			DoubleClick ();
		doubleClickTimer = 0;
	}

	void OnMouseDrag () {
		//Vector2 t_direction = UICamera.ScreenToWorldPoint (Input.mousePosition) - this.transform.position;
		Vector3 t_position = Camera.main.ScreenToWorldPoint (Input.mousePosition) - Camera.main.GetComponent<CS_Camera>().GetDeltaPostion();
		float t_positionY = t_position.y - t_position.z * 1.717f;
		t_position = new Vector3 (t_position.x, t_positionY, 0);

		Vector2 t_direction = t_position - myPlayer.transform.position;
		//t_direction = new Vector2(t_direction.x * 0.58f, t_direction.y);
		t_direction.Normalize ();
		//Debug.Log (t_direction);
		myPlayer.SetDirection (t_direction);
	}

	void OnMouseUp () {
		myPlayer.SetOnMove (false);
	}

	private void DoubleClick () {
		Debug.Log ("DoubleClick:" + doubleClickTimer);
		myLoadStage.ShowPause ();
	}
}
